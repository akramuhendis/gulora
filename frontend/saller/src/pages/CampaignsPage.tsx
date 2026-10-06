import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  AlertCircle,
  CheckCircle2,
  Copy,
  Download,
  Loader2,
  Megaphone,
  Percent,
  Trash2,
  Truck,
  Upload,
} from 'lucide-react'
import { usePageMeta } from '../hooks/usePageMeta'
import {
  ApiError,
  catalogApi,
  sellerCampaignApi,
  sellerPortalApi,
  type CategoryResponse,
  type ProductSummaryResponse,
  type SellerCampaignPlanWire,
  type UpsertSellerCampaignPlanBody,
} from '../services/api'
import {
  loadPlans,
  migrateV1SessionToV2,
  savePlans,
  clearPlansLocalStorage,
  scopeSummary,
  parseImportedPlans,
  type CampaignPlan,
  type CampaignScope,
} from '../lib/campaignDrafts'

const MAX_PERCENT = 100
const MIN_PERCENT = 1

function toDatetimeLocalValue(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

function fromDatetimeLocalValue(s: string): Date | null {
  const d = new Date(s)
  return Number.isNaN(d.getTime()) ? null : d
}

function flattenCategories(nodes: CategoryResponse[], depth = 0): { id: string; label: string }[] {
  const out: { id: string; label: string }[] = []
  for (const n of nodes) {
    const prefix = depth > 0 ? `${'— '.repeat(depth)}` : ''
    out.push({ id: n.id, label: `${prefix}${n.name}` })
    if (n.children?.length) out.push(...flattenCategories(n.children, depth + 1))
  }
  return out
}

function wireToPlan(w: SellerCampaignPlanWire): CampaignPlan {
  return {
    id: w.id,
    name: w.name,
    kind: w.kind,
    percent: w.percent,
    startsAt: w.startsAt,
    endsAt: w.endsAt,
    minCartAmount: w.minCartAmount,
    scope: w.scope,
    notes: w.notes,
    createdAt: w.createdAt,
    updatedAt: w.updatedAt ?? w.createdAt,
  }
}

function planToUpsertBody(p: CampaignPlan): UpsertSellerCampaignPlanBody {
  return {
    name: p.name.trim(),
    kind: p.kind,
    percent: p.percent,
    startsAt: p.startsAt,
    endsAt: p.endsAt,
    minCartAmount: p.minCartAmount,
    scope: p.scope,
    notes: p.notes,
  }
}

export default function CampaignsPage() {
  usePageMeta('Kampaniyalar', {
    description:
      'Endirim və çatdırılma kampaniya planları: tarix, əhatə və faiz. Planlar PostgreSQL-də saxlanılır (satıcı JWT).',
  })

  const [plans, setPlans] = useState<CampaignPlan[]>([])
  const [listLoading, setListLoading] = useState(true)
  const [apiOffline, setApiOffline] = useState(false)

  const [name, setName] = useState('')
  const [kind, setKind] = useState<'discount' | 'shipping'>('discount')
  const [percent, setPercent] = useState('')
  const [startsLocal, setStartsLocal] = useState(() => toDatetimeLocalValue(new Date()))
  const [endsLocal, setEndsLocal] = useState(() => {
    const e = new Date()
    e.setDate(e.getDate() + 14)
    return toDatetimeLocalValue(e)
  })
  const [minCart, setMinCart] = useState('')
  const [notes, setNotes] = useState('')
  const [scopeType, setScopeType] = useState<'all' | 'category' | 'products'>('all')
  const [categoryId, setCategoryId] = useState('')
  const [selectedProducts, setSelectedProducts] = useState<Map<string, string>>(() => new Map())

  const [editingId, setEditingId] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [toast, setToast] = useState<string | null>(null)

  const [categories, setCategories] = useState<{ id: string; label: string }[]>([])
  const [categoriesLoading, setCategoriesLoading] = useState(true)
  const [categoriesError, setCategoriesError] = useState<string | null>(null)

  const [productSearch, setProductSearch] = useState('')
  const [productRows, setProductRows] = useState<ProductSummaryResponse[]>([])
  const [productLoading, setProductLoading] = useState(false)
  const [productError, setProductError] = useState<string | null>(null)

  const importRef = useRef<HTMLInputElement>(null)
  const formRef = useRef<HTMLFormElement>(null)
  const toastTimer = useRef<ReturnType<typeof setTimeout> | null>(null)

  const showToast = useCallback((msg: string) => {
    setToast(msg)
    if (toastTimer.current) clearTimeout(toastTimer.current)
    toastTimer.current = setTimeout(() => setToast(null), 4500)
  }, [])

  useEffect(() => {
    return () => {
      if (toastTimer.current) clearTimeout(toastTimer.current)
    }
  }, [])

  useEffect(() => {
    let cancelled = false
    queueMicrotask(() => {
      if (cancelled) return
      setCategoriesLoading(true)
      setCategoriesError(null)
      catalogApi
        .listCategories(true)
        .then((tree) => {
          if (cancelled) return
          setCategories(flattenCategories(tree))
        })
        .catch((e) => {
          if (!cancelled) setCategoriesError(e instanceof ApiError ? e.message : 'Kateqoriyalar yüklənmədi.')
        })
        .finally(() => {
          if (!cancelled) setCategoriesLoading(false)
        })
    })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (scopeType !== 'products') {
      let cancelled = false
      queueMicrotask(() => {
        if (!cancelled) {
          setProductRows([])
          setProductError(null)
        }
      })
      return () => {
        cancelled = true
      }
    }
    let cancelled = false
    const handle = setTimeout(() => {
      setProductLoading(true)
      setProductError(null)
      sellerPortalApi
        .listProducts({
          page: 1,
          pageSize: 80,
          query: productSearch.trim() || undefined,
          isActive: true,
        })
        .then((res) => {
          if (!cancelled) setProductRows(res.items)
        })
        .catch((e) => {
          if (!cancelled) setProductError(e instanceof ApiError ? e.message : 'Məhsullar yüklənmədi.')
        })
        .finally(() => {
          if (!cancelled) setProductLoading(false)
        })
    }, 320)
    return () => {
      cancelled = true
      clearTimeout(handle)
    }
  }, [productSearch, scopeType])

  const refreshFromServer = useCallback(async () => {
    const remote = await sellerCampaignApi.list()
    setPlans(remote.map(wireToPlan))
  }, [])

  useEffect(() => {
    let cancelled = false
    queueMicrotask(() => {
      void (async () => {
        setListLoading(true)
        setApiOffline(false)
        try {
          migrateV1SessionToV2()
          let remote = await sellerCampaignApi.list()
          if (cancelled) return
          if (remote.length === 0) {
            const local = loadPlans()
            if (local.length > 0) {
              for (const p of local) {
                if (cancelled) return
                try {
                  await sellerCampaignApi.create(planToUpsertBody(p))
                } catch {
                  /* tək plan */
                }
              }
              clearPlansLocalStorage()
              remote = await sellerCampaignApi.list()
            }
          }
          if (cancelled) return
          setPlans(remote.map(wireToPlan))
        } catch {
          if (cancelled) return
          setApiOffline(true)
          setPlans(loadPlans())
        } finally {
          if (!cancelled) setListLoading(false)
        }
      })()
    })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    if (apiOffline) savePlans(plans)
  }, [plans, apiOffline])

  const sorted = useMemo(
    () => [...plans].sort((a, b) => (a.startsAt < b.startsAt ? 1 : -1)),
    [plans]
  )

  const resetForm = useCallback(() => {
    setEditingId(null)
    setName('')
    setKind('discount')
    setPercent('')
    setStartsLocal(toDatetimeLocalValue(new Date()))
    const e = new Date()
    e.setDate(e.getDate() + 14)
    setEndsLocal(toDatetimeLocalValue(e))
    setMinCart('')
    setNotes('')
    setScopeType('all')
    setCategoryId('')
    setSelectedProducts(new Map())
    setProductSearch('')
    setFormError(null)
  }, [])

  const fillFormFromPlan = useCallback((p: CampaignPlan) => {
    setEditingId(p.id)
    setName(p.name)
    setKind(p.kind)
    setPercent(String(p.percent))
    const s = fromDatetimeLocalValue(p.startsAt) ?? new Date(p.startsAt)
    const e = fromDatetimeLocalValue(p.endsAt) ?? new Date(p.endsAt)
    setStartsLocal(toDatetimeLocalValue(Number.isNaN(s.getTime()) ? new Date() : s))
    setEndsLocal(toDatetimeLocalValue(Number.isNaN(e.getTime()) ? new Date() : e))
    setMinCart(p.minCartAmount != null ? String(p.minCartAmount) : '')
    setNotes(p.notes ?? '')
    if (p.scope.type === 'all') {
      setScopeType('all')
      setCategoryId('')
      setSelectedProducts(new Map())
    } else if (p.scope.type === 'category') {
      setScopeType('category')
      setCategoryId(p.scope.categoryId)
      setSelectedProducts(new Map())
    } else {
      setScopeType('products')
      setCategoryId('')
      const m = new Map<string, string>()
      for (const x of p.scope.products) m.set(x.id, x.name)
      setSelectedProducts(m)
    }
    setFormError(null)
    queueMicrotask(() => formRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' }))
  }, [])

  function buildScope(): CampaignScope | null {
    if (scopeType === 'all') return { type: 'all' }
    if (scopeType === 'category') {
      const row = categories.find((c) => c.id === categoryId)
      if (!categoryId || !row) return null
      return { type: 'category', categoryId, categoryName: row.label.replace(/^—+\s*/g, '').trim() || row.label }
    }
    const products = [...selectedProducts.entries()].map(([id, n]) => ({ id, name: n }))
    if (products.length === 0) return null
    return { type: 'products', products }
  }

  function validate(): string | null {
    const n = name.trim()
    if (!n) return 'Kampaniya adı mütləqdir.'
    const p = Number(percent)
    if (!Number.isFinite(p) || p < MIN_PERCENT || p > MAX_PERCENT) return `Faiz ${MIN_PERCENT}–${MAX_PERCENT} aralığında tam ədəd olmalıdır.`
    const start = fromDatetimeLocalValue(startsLocal)
    const end = fromDatetimeLocalValue(endsLocal)
    if (!start || !end) return 'Başlanğıc və bitmə tarixləri düzgün seçilməlidir.'
    if (end.getTime() <= start.getTime()) return 'Bitmə tarixi başlanğıcdan sonra olmalıdır.'
    const scope = buildScope()
    if (!scope) {
      if (scopeType === 'category') return 'Kateqoriya seçin.'
      if (scopeType === 'products') return 'Ən azı bir məhsul seçin.'
      return 'Əhatə seçimi yoxlanılmadı.'
    }
    if (kind === 'shipping' && minCart.trim()) {
      const m = Number(minCart.replace(',', '.'))
      if (!Number.isFinite(m) || m < 0) return 'Minimum səbət məbləği müsbət ədəd olmalıdır.'
    }
    return null
  }

  async function submitPlan(e: React.FormEvent) {
    e.preventDefault()
    const err = validate()
    if (err) {
      setFormError(err)
      return
    }
    setFormError(null)
    const scope = buildScope()!
    const start = fromDatetimeLocalValue(startsLocal)!
    const end = fromDatetimeLocalValue(endsLocal)!
    const pVal = Math.round(Number(percent))
    let minCartAmount: number | null = null
    if (kind === 'shipping' && minCart.trim()) {
      minCartAmount = Number(minCart.replace(',', '.'))
    }
    const now = new Date().toISOString()
    const base: CampaignPlan = {
      id: '',
      name: name.trim(),
      kind,
      percent: pVal,
      startsAt: start.toISOString(),
      endsAt: end.toISOString(),
      minCartAmount,
      scope,
      notes: notes.trim() ? notes.trim().slice(0, 2000) : null,
      createdAt: '',
      updatedAt: '',
    }

    if (apiOffline) {
      if (editingId) {
        setPlans((rows) =>
          rows.map((r) =>
            r.id === editingId
              ? {
                  ...r,
                  ...base,
                  id: r.id,
                  createdAt: r.createdAt,
                  updatedAt: now,
                }
              : r
          )
        )
        showToast('Plan yalnız brauzerdə yeniləndi (API əlçatan deyil).')
      } else {
        const lid = `local-${Date.now()}`
        const row: CampaignPlan = {
          ...base,
          id: lid,
          createdAt: now,
          updatedAt: now,
        }
        setPlans((d) => [row, ...d])
        showToast('Plan yalnız brauzerdə saxlanıldı (API əlçatan deyil).')
      }
      resetForm()
      return
    }

    try {
      if (editingId) {
        const updated = await sellerCampaignApi.update(editingId, planToUpsertBody(base))
        setPlans((rows) => rows.map((r) => (r.id === editingId ? wireToPlan(updated) : r)))
        showToast('Kampaniya planı serverdə yeniləndi.')
      } else {
        const created = await sellerCampaignApi.create(planToUpsertBody(base))
        setPlans((d) => [wireToPlan(created), ...d])
        showToast('Yeni plan serverdə yaradıldı.')
      }
      resetForm()
    } catch (ex) {
      setFormError(ex instanceof ApiError ? ex.message : 'Serverə yazılmadı.')
    }
  }

  async function removePlan(id: string) {
    const row = plans.find((x) => x.id === id)
    const ok = window.confirm(row ? `"${row.name}" silinsin?` : 'Bu qeyd silinsin?')
    if (!ok) return
    if (apiOffline) {
      setPlans((d) => d.filter((x) => x.id !== id))
      if (editingId === id) resetForm()
      showToast('Plan silindi (yalnız brauzer).')
      return
    }
    try {
      await sellerCampaignApi.remove(id)
      setPlans((d) => d.filter((x) => x.id !== id))
      if (editingId === id) resetForm()
      showToast('Plan serverdən silindi.')
    } catch (ex) {
      setFormError(ex instanceof ApiError ? ex.message : 'Silinmədi.')
    }
  }

  async function duplicatePlan(p: CampaignPlan) {
    const body = planToUpsertBody({
      ...p,
      name: `${p.name} (surət)`,
    })
    if (apiOffline) {
      const now = new Date().toISOString()
      setPlans((d) => [
        {
          ...p,
          id: `local-${Date.now()}`,
          name: body.name,
          createdAt: now,
          updatedAt: now,
        },
        ...d,
      ])
      showToast('Surət yalnız brauzerdə yaradıldı.')
      return
    }
    try {
      const created = await sellerCampaignApi.create(body)
      setPlans((d) => [wireToPlan(created), ...d])
      showToast('Surət serverdə yaradıldı.')
    } catch (ex) {
      setFormError(ex instanceof ApiError ? ex.message : 'Surət yaradılmadı.')
    }
  }

  function exportJson() {
    const blob = new Blob([JSON.stringify(plans, null, 2)], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `gulora-kampaniya-planlari-${new Date().toISOString().slice(0, 10)}.json`
    a.click()
    URL.revokeObjectURL(url)
    showToast('JSON faylı endirildi.')
  }

  function onImportFile(f: File | null) {
    if (!f) return
    const reader = new FileReader()
    reader.onload = () => {
      void (async () => {
        const text = typeof reader.result === 'string' ? reader.result : ''
        const res = parseImportedPlans(text)
        if (!res.ok) {
          setFormError(res.error)
          return
        }
        setFormError(null)
        if (apiOffline) {
          setPlans((prev) => {
            const byId = new Map(prev.map((x) => [x.id, x]))
            for (const p of res.plans) {
              byId.set(p.id, p)
            }
            return [...byId.values()].sort((a, b) => (a.updatedAt < b.updatedAt ? 1 : -1))
          })
          showToast(`${res.plans.length} qeyd idxal olundu (brauzer).`)
          return
        }
        try {
          let n = 0
          for (const p of res.plans) {
            await sellerCampaignApi.create(planToUpsertBody(p))
            n++
          }
          await refreshFromServer()
          showToast(`${n} plan serverə idxal olundu.`)
        } catch (ex) {
          setFormError(ex instanceof ApiError ? ex.message : 'Idxal alınmadı.')
        }
      })()
    }
    reader.readAsText(f, 'utf-8')
    if (importRef.current) importRef.current.value = ''
  }

  function toggleProductPick(pr: ProductSummaryResponse) {
    setSelectedProducts((prev) => {
      const next = new Map(prev)
      if (next.has(pr.id)) next.delete(pr.id)
      else next.set(pr.id, pr.name)
      return next
    })
  }

  return (
    <div>
      <div
        role="status"
        aria-live="polite"
        aria-atomic="true"
        className={`fixed bottom-6 right-6 z-50 flex max-w-sm items-start gap-2 rounded-lg border border-emerald-200 bg-white px-4 py-3 text-sm text-emerald-900 shadow-lg transition-opacity ${
          toast ? 'opacity-100' : 'pointer-events-none opacity-0'
        }`}
      >
        {toast ? (
          <>
            <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-emerald-600" aria-hidden />
            <span>{toast}</span>
          </>
        ) : (
          <span className="sr-only">Boş</span>
        )}
      </div>

      <nav className="mb-6 flex flex-wrap items-center gap-2 text-sm text-slate-500" aria-label="Çörək qırıntısı">
        <Link to="/" className="font-medium text-[#006437] hover:underline">
          İcmal
        </Link>
        <span aria-hidden className="text-slate-300">
          /
        </span>
        <span className="text-slate-800">Kampaniyalar</span>
      </nav>

      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Kampaniyalar</h1>
      <p className="mt-2 max-w-3xl text-slate-600">
        Endirim və çatdırılma kampaniya <strong>planları</strong> tarix, faiz və məhsul əhatəsi ilə serverdə
        saxlanılır (<code className="rounded bg-slate-100 px-1 text-xs">GET/POST/PUT/DELETE /api/v1/seller/campaigns</code>
        ). Köhnə brauzer planları ilk uğurlu yükləmədə avtomatik köçürülür.
      </p>

      {listLoading && (
        <div className="mt-6 flex items-center gap-2 text-sm text-slate-600" aria-live="polite">
          <Loader2 className="h-5 w-5 animate-spin shrink-0" aria-hidden />
          Planlar yüklənir…
        </div>
      )}

      {apiOffline && (
        <div
          className="mt-6 rounded-xl border border-red-200 bg-red-50/90 p-4 text-sm text-red-950"
          role="alert"
        >
          <div className="flex gap-2">
            <AlertCircle className="mt-0.5 h-5 w-5 shrink-0 text-red-700" aria-hidden />
            <div>
              <p className="font-semibold">API əlçatan deyil</p>
              <p className="mt-1 text-red-900/90">
                Planlar müvəqqəti olaraq brauzer <code className="rounded bg-white/80 px-1 text-xs">localStorage</code>{' '}
                ünvanında göstərilir; əlaqə bərpa olanda səhifəni yeniləyin. Dəyişikliklər serverə yazılmır.
              </p>
            </div>
          </div>
        </div>
      )}

      {!apiOffline && !listLoading && (
        <div
          className="mt-6 rounded-xl border border-emerald-200 bg-emerald-50/90 p-4 text-sm text-emerald-950"
          role="note"
        >
          <div className="flex gap-2">
            <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-emerald-700" aria-hidden />
            <div>
              <p className="font-semibold">Vitrin və qiymət</p>
              <p className="mt-1 text-emerald-900/90">
                <strong>Faiz endirimi</strong> aktiv tarix aralığında və əhatəyə düşən məhsullar üçün{' '}
                <code className="rounded bg-white/80 px-1 text-xs">GET /api/v1/products</code> cavabında avtomatik
                əks olunur (5173 vitrin). <strong>Çatdırılma endirimi</strong> məhsul qiymətini dəyişmir; yalnız kampaniya
                rozeti / məlumat üçün göstərilir (çatdırılma xərcinin özü ayrı hesablanır).
              </p>
              <p className="mt-2 text-emerald-900/90">
                Əhatə <strong>seçilmiş məhsullar</strong>dırsa, endirim yalnız cədvəldə göstərilən həmin məhsul(lar)ın
                səhifəsində görünür — bütün vitrin üçün &quot;Bütün məhsullar&quot; və ya kateqoriya seçin.
              </p>
            </div>
          </div>
        </div>
      )}

      <section className="mt-10 grid gap-6 lg:grid-cols-2" aria-labelledby="campaign-cards">
        <h2 id="campaign-cards" className="sr-only">
          Kampaniya növləri
        </h2>
        <article className="rounded-xl border border-slate-200 bg-linear-to-br from-white to-slate-50 p-6 shadow-sm">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-[#006437]/10 text-[#006437]">
            <Percent className="h-6 w-6" aria-hidden />
          </div>
          <h3 className="mt-4 text-lg font-semibold text-slate-900">Faiz endirimi</h3>
          <p className="mt-2 text-sm text-slate-600">
            Seçilmiş və ya bütün məhsullar üçün müddətli endirim faizi. Plan təqvimi ilə vitrin təşkilini əvvəlcədən
            qeyd edin.
          </p>
        </article>
        <article className="rounded-xl border border-slate-200 bg-linear-to-br from-white to-emerald-50/50 p-6 shadow-sm">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-100 text-emerald-800">
            <Truck className="h-6 w-6" aria-hidden />
          </div>
          <h3 className="mt-4 text-lg font-semibold text-slate-900">Çatdırılma endirimi</h3>
          <p className="mt-2 text-sm text-slate-600">
            Çatdırılma xərcinə endirim faizi; istəyə görə minimum səbət şərti əlavə edə bilərsiniz.
          </p>
        </article>
      </section>

      <div className="mt-8 flex flex-wrap gap-2">
        <button
          type="button"
          onClick={exportJson}
          className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          <Download className="h-4 w-4" aria-hidden />
          JSON ixrac
        </button>
        <button
          type="button"
          onClick={() => importRef.current?.click()}
          className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          <Upload className="h-4 w-4" aria-hidden />
          JSON idxal
        </button>
        <input
          ref={importRef}
          type="file"
          accept="application/json,.json"
          className="sr-only"
          aria-label="Kampaniya planları JSON faylı"
          onChange={(e) => onImportFile(e.target.files?.[0] ?? null)}
        />
      </div>

      <form
        ref={formRef}
        onSubmit={submitPlan}
        className="mt-10 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
        aria-labelledby="draft-campaign"
        noValidate
      >
        <div className="flex flex-wrap items-center gap-3">
          <Megaphone className="h-8 w-8 text-[#006437]" aria-hidden />
          <div>
            <h2 id="draft-campaign" className="text-lg font-semibold text-slate-900">
              {editingId ? 'Kampaniya planını redaktə et' : 'Yeni kampaniya planı'}
            </h2>
            <p className="text-sm text-slate-600">
              Enter ilə formu göndərin.
              {apiOffline
                ? ' API əlçatan olmayanda planlar yalnız brauzer yaddaşında saxlanılır.'
                : ' Dəyişikliklər dərhal serverə yazılır.'}
            </p>
          </div>
        </div>

        {formError && (
          <div
            role="alert"
            className="mt-4 flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-900"
          >
            <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" aria-hidden />
            <span>{formError}</span>
          </div>
        )}

        <div className="mt-6 grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label htmlFor="camp-name" className="block text-xs font-medium text-slate-600">
              Kampaniya adı <span className="text-red-600">*</span>
            </label>
            <input
              id="camp-name"
              name="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Məs: Yaz endirimi"
              required
              maxLength={200}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
            />
          </div>

          <fieldset className="sm:col-span-2">
            <legend className="text-xs font-medium text-slate-600">Kampaniya növü</legend>
            <div className="mt-2 flex flex-wrap gap-3" role="radiogroup" aria-label="Kampaniya növü">
              <label className="inline-flex cursor-pointer items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm has-checked:border-[#006437] has-checked:bg-[#006437] has-checked:text-white">
                <input
                  type="radio"
                  name="kind"
                  value="discount"
                  checked={kind === 'discount'}
                  onChange={() => setKind('discount')}
                  className="sr-only"
                />
                Faiz endirimi
              </label>
              <label className="inline-flex cursor-pointer items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm has-checked:border-[#006437] has-checked:bg-[#006437] has-checked:text-white">
                <input
                  type="radio"
                  name="kind"
                  value="shipping"
                  checked={kind === 'shipping'}
                  onChange={() => setKind('shipping')}
                  className="sr-only"
                />
                Çatdırılma endirimi
              </label>
            </div>
          </fieldset>

          <div>
            <label htmlFor="camp-pct" className="block text-xs font-medium text-slate-600">
              Faiz (%) <span className="text-red-600">*</span>
            </label>
            <input
              id="camp-pct"
              name="percent"
              inputMode="numeric"
              value={percent}
              onChange={(e) => setPercent(e.target.value.replace(/\D/g, '').slice(0, 3))}
              placeholder="15"
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm tabular-nums focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
              aria-describedby="camp-pct-hint"
            />
            <p id="camp-pct-hint" className="mt-1 text-[11px] text-slate-500">
              {MIN_PERCENT}–{MAX_PERCENT} arası tam ədəd.
            </p>
          </div>

          {kind === 'shipping' && (
            <div>
              <label htmlFor="camp-min" className="block text-xs font-medium text-slate-600">
                Minimum səbət (₼, istəyə bağlı)
              </label>
              <input
                id="camp-min"
                name="minCart"
                inputMode="decimal"
                value={minCart}
                onChange={(e) => setMinCart(e.target.value.replace(/[^\d.,]/g, ''))}
                placeholder="50"
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm tabular-nums focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
              />
            </div>
          )}

          <div>
            <label htmlFor="camp-start" className="block text-xs font-medium text-slate-600">
              Başlanğıc <span className="text-red-600">*</span>
            </label>
            <input
              id="camp-start"
              name="starts"
              type="datetime-local"
              value={startsLocal}
              onChange={(e) => setStartsLocal(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
            />
          </div>
          <div>
            <label htmlFor="camp-end" className="block text-xs font-medium text-slate-600">
              Bitmə <span className="text-red-600">*</span>
            </label>
            <input
              id="camp-end"
              name="ends"
              type="datetime-local"
              value={endsLocal}
              onChange={(e) => setEndsLocal(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
            />
          </div>

          <div className="sm:col-span-2">
            <label htmlFor="camp-notes" className="block text-xs font-medium text-slate-600">
              Qeyd (daxili)
            </label>
            <textarea
              id="camp-notes"
              name="notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              maxLength={2000}
              placeholder="Komanda üçün qısa şərh…"
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
            />
          </div>

          <fieldset className="sm:col-span-2">
            <legend className="text-xs font-medium text-slate-600">Əhatə</legend>
            <div className="mt-2 flex flex-wrap gap-2" role="radiogroup" aria-label="Kampaniya əhatəsi">
              {(
                [
                  ['all', 'Bütün məhsullar'],
                  ['category', 'Kateqoriya'],
                  ['products', 'Seçilmiş məhsullar'],
                ] as const
              ).map(([v, label]) => (
                <label
                  key={v}
                  className="inline-flex cursor-pointer items-center gap-2 rounded-full border border-slate-200 px-4 py-2 text-sm has-checked:border-[#006437] has-checked:bg-[#006437] has-checked:text-white"
                >
                  <input
                    type="radio"
                    name="scopeType"
                    value={v}
                    checked={scopeType === v}
                    onChange={() => setScopeType(v)}
                    className="sr-only"
                  />
                  {label}
                </label>
              ))}
            </div>
          </fieldset>

          {scopeType === 'category' && (
            <div className="sm:col-span-2">
              <label htmlFor="camp-cat" className="block text-xs font-medium text-slate-600">
                Kateqoriya <span className="text-red-600">*</span>
              </label>
              <select
                id="camp-cat"
                name="categoryId"
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value)}
                disabled={categoriesLoading || !!categoriesError}
                className="mt-1 w-full max-w-xl rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25 disabled:bg-slate-100"
              >
                <option value="">— Seçin —</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.label}
                  </option>
                ))}
              </select>
              {categoriesLoading && (
                <p className="mt-1 flex items-center gap-1 text-xs text-slate-500">
                  <Loader2 className="h-3 w-3 animate-spin" aria-hidden />
                  Kateqoriyalar yüklənir…
                </p>
              )}
              {categoriesError && <p className="mt-1 text-xs text-red-600">{categoriesError}</p>}
            </div>
          )}

          {scopeType === 'products' && (
            <div className="sm:col-span-2 space-y-3 rounded-lg border border-slate-100 bg-slate-50/80 p-4">
              <div>
                <label htmlFor="camp-prod-q" className="block text-xs font-medium text-slate-600">
                  Məhsul axtarışı
                </label>
                <input
                  id="camp-prod-q"
                  type="search"
                  value={productSearch}
                  onChange={(e) => setProductSearch(e.target.value)}
                  placeholder="Ad və ya SKU ilə…"
                  className="mt-1 w-full max-w-md rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]/25"
                />
              </div>
              {productError && <p className="text-xs text-red-600">{productError}</p>}
              <div className="max-h-56 overflow-y-auto rounded-md border border-slate-200 bg-white">
                {productLoading ? (
                  <div className="flex items-center justify-center gap-2 py-8 text-sm text-slate-500">
                    <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
                    Yüklənir…
                  </div>
                ) : productRows.length === 0 ? (
                  <p className="py-6 text-center text-sm text-slate-500">Məhsul tapılmadı.</p>
                ) : (
                  <ul className="divide-y divide-slate-100">
                    {productRows.map((pr) => {
                      const checked = selectedProducts.has(pr.id)
                      return (
                        <li key={pr.id}>
                          <label className="flex cursor-pointer items-start gap-3 px-3 py-2 hover:bg-slate-50">
                            <input
                              type="checkbox"
                              checked={checked}
                              onChange={() => toggleProductPick(pr)}
                              className="mt-1 rounded border-slate-300"
                            />
                            <span className="min-w-0 flex-1">
                              <span className="block text-sm font-medium text-slate-900">{pr.name}</span>
                              <span className="text-xs text-slate-500">{pr.categoryName}</span>
                            </span>
                          </label>
                        </li>
                      )
                    })}
                  </ul>
                )}
              </div>
              <p className="text-xs text-slate-600">
                Seçilmiş: <strong>{selectedProducts.size}</strong> məhsul
                {selectedProducts.size > 0 && (
                  <span className="ml-2 text-slate-500">
                    ({[...selectedProducts.values()].slice(0, 3).join(', ')}
                    {selectedProducts.size > 3 ? '…' : ''})
                  </span>
                )}
              </p>
            </div>
          )}
        </div>

        <div className="mt-6 flex flex-wrap gap-2">
          <button
            type="submit"
            className="rounded-lg bg-[#006437] px-4 py-2.5 text-sm font-semibold text-white hover:bg-[#005029] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
          >
            {editingId ? 'Yadda saxla' : 'Planı əlavə et'}
          </button>
          {editingId && (
            <button
              type="button"
              onClick={resetForm}
              className="rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400 focus-visible:ring-offset-2"
            >
              Ləğv et
            </button>
          )}
        </div>
      </form>

      <section className="mt-10" aria-labelledby="draft-table">
        <h2 id="draft-table" className="text-lg font-semibold text-slate-900">
          Plan cədvəli
        </h2>
        <div className="mt-4 overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
          <table className="w-full min-w-[720px] text-left text-sm">
            <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Ad</th>
                <th className="px-4 py-3">Növ</th>
                <th className="px-4 py-3 text-right">%</th>
                <th className="px-4 py-3">Əhatə</th>
                <th className="px-4 py-3">Başlanğıc</th>
                <th className="px-4 py-3">Bitmə</th>
                <th className="px-4 py-3">Min. səbət</th>
                <th className="w-[1%] px-4 py-3 text-right" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {sorted.map((d) => (
                <tr key={d.id} className="hover:bg-slate-50/80">
                  <td className="px-4 py-3 font-medium text-slate-900">{d.name}</td>
                  <td className="px-4 py-3 text-slate-700">{d.kind === 'discount' ? 'Endirim' : 'Çatdırılma'}</td>
                  <td className="px-4 py-3 text-right tabular-nums">{d.percent}</td>
                  <td className="max-w-[200px] truncate px-4 py-3 text-slate-600" title={scopeSummary(d.scope)}>
                    {scopeSummary(d.scope)}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                    {new Date(d.startsAt).toLocaleString('az-AZ', { dateStyle: 'short', timeStyle: 'short' })}
                  </td>
                  <td className="whitespace-nowrap px-4 py-3 text-slate-600">
                    {new Date(d.endsAt).toLocaleString('az-AZ', { dateStyle: 'short', timeStyle: 'short' })}
                  </td>
                  <td className="px-4 py-3 tabular-nums text-slate-600">
                    {d.minCartAmount != null ? `${d.minCartAmount} ₼` : '—'}
                  </td>
                  <td className="space-x-1 whitespace-nowrap px-4 py-3 text-right">
                    <button
                      type="button"
                      onClick={() => fillFormFromPlan(d)}
                      className="inline-flex items-center rounded-lg border border-slate-200 px-2 py-1 text-xs font-medium text-slate-800 hover:bg-slate-50"
                    >
                      Redaktə
                    </button>
                    <button
                      type="button"
                      onClick={() => duplicatePlan(d)}
                      className="inline-flex items-center gap-1 rounded-lg border border-slate-200 px-2 py-1 text-xs font-medium text-slate-800 hover:bg-slate-50"
                      aria-label="Surət çıxar"
                    >
                      <Copy className="h-3.5 w-3.5" aria-hidden />
                    </button>
                    <button
                      type="button"
                      onClick={() => removePlan(d.id)}
                      className="inline-flex items-center gap-1 rounded-lg border border-red-100 px-2 py-1 text-xs font-medium text-red-700 hover:bg-red-50"
                      aria-label="Sil"
                    >
                      <Trash2 className="h-3.5 w-3.5" aria-hidden />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {sorted.length === 0 && (
            <p className="px-4 py-10 text-center text-sm text-slate-600">Hələ plan yoxdur.</p>
          )}
        </div>
        <p className="mt-4 text-xs text-slate-500">
          Köhnə sessiya (v1) planları bir dəfə serverə və ya brauzer v2-yə köçürülür. Ehtiyat üçün JSON ixracından
          istifadə edin.
        </p>
      </section>
    </div>
  )
}
