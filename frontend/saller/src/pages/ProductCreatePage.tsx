import { useCallback, useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { ArrowLeft, Loader2 } from 'lucide-react'
import {
  ApiError,
  catalogApi,
  sellerPortalApi,
  type CategoryResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

function slugifyAz(text: string): string {
  const map: Record<string, string> = {
    ə: 'e',
    Ə: 'e',
    ı: 'i',
    İ: 'i',
    ö: 'o',
    Ö: 'o',
    ü: 'u',
    Ü: 'u',
    ç: 'c',
    Ç: 'c',
    ş: 's',
    Ş: 's',
    ğ: 'g',
    Ğ: 'g',
  }
  let s = text.trim().toLowerCase()
  for (const [k, v] of Object.entries(map)) {
    s = s.split(k).join(v)
  }
  return s
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

function flattenCategories(
  cats: CategoryResponse[],
  prefix = ''
): { id: string; label: string }[] {
  const out: { id: string; label: string }[] = []
  for (const c of cats) {
    const label = prefix ? `${prefix} › ${c.name}` : c.name
    out.push({ id: c.id, label })
    if (c.children?.length) {
      out.push(...flattenCategories(c.children, label))
    }
  }
  return out
}

export default function ProductCreatePage() {
  usePageMeta('Yeni məhsul')

  const navigate = useNavigate()
  const [categories, setCategories] = useState<CategoryResponse[]>([])
  const [categoriesLoading, setCategoriesLoading] = useState(true)
  const [categoriesError, setCategoriesError] = useState<string | null>(null)
  /** Aktiv siyahı boş olduğu üçün API-yə onlyActive=false ilə yenidən sorğu olunub. */
  const [inactiveCategoriesFallback, setInactiveCategoriesFallback] = useState(false)

  const [name, setName] = useState('')
  const [slug, setSlug] = useState('')
  const [slugManual, setSlugManual] = useState(false)
  const [categoryId, setCategoryId] = useState('')
  const [price, setPrice] = useState('')
  const [stockQuantity, setStockQuantity] = useState('0')
  const [shortDescription, setShortDescription] = useState('')
  const [description, setDescription] = useState('')
  const [sku, setSku] = useState('')
  const [brand, setBrand] = useState('')
  const [discountedPrice, setDiscountedPrice] = useState('')
  const [isFeatured, setIsFeatured] = useState(false)

  const [coverFile, setCoverFile] = useState<File | null>(null)
  const [coverUrlInput, setCoverUrlInput] = useState('')
  const [coverPreview, setCoverPreview] = useState<string | null>(null)
  const coverFileInputRef = useRef<HTMLInputElement>(null)

  const [submitError, setSubmitError] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const flatCategories = useMemo(() => flattenCategories(categories), [categories])

  const loadCategories = useCallback(async () => {
    setCategoriesLoading(true)
    setCategoriesError(null)
    setInactiveCategoriesFallback(false)
    try {
      let list = await catalogApi.listCategories(true)
      let flat = flattenCategories(list)
      if (flat.length === 0) {
        list = await catalogApi.listCategories(false)
        flat = flattenCategories(list)
        if (flat.length > 0) {
          setInactiveCategoriesFallback(true)
        }
      }
      setCategories(list)
    } catch (e) {
      setCategoriesError(e instanceof ApiError ? e.message : 'Kateqoriyalar yüklənmədi.')
      setCategories([])
      setInactiveCategoriesFallback(false)
    } finally {
      setCategoriesLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => {
      void loadCategories()
    })
  }, [loadCategories])

  useEffect(() => {
    return () => {
      if (coverPreview) URL.revokeObjectURL(coverPreview)
    }
  }, [coverPreview])

  function onCoverFileChange(f: File | null) {
    setCoverPreview((prev) => {
      if (prev) URL.revokeObjectURL(prev)
      return f ? URL.createObjectURL(f) : null
    })
    setCoverFile(f)
    if (f) setCoverUrlInput('')
    if (!f && coverFileInputRef.current) coverFileInputRef.current.value = ''
  }

  function syncSlugFromName(nextName: string) {
    if (!slugManual) {
      setSlug(slugifyAz(nextName))
    }
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setSubmitError(null)

    const priceNum = Number(price.replace(',', '.'))
    const stockNum = Number.parseInt(stockQuantity, 10)
    const discRaw = discountedPrice.trim()
    const discNum = discRaw === '' ? null : Number(discRaw.replace(',', '.'))

    if (!name.trim()) {
      setSubmitError('Məhsul adını daxil edin.')
      return
    }
    if (!slug.trim()) {
      setSubmitError('Slug boş ola bilməz.')
      return
    }
    if (!categoryId) {
      setSubmitError('Kateqoriya seçin.')
      return
    }
    if (!Number.isFinite(priceNum) || priceNum <= 0) {
      setSubmitError('Qiymət müsbət ədəd olmalıdır.')
      return
    }
    if (!Number.isFinite(stockNum) || stockNum < 0) {
      setSubmitError('Stok 0 və ya daha böyük tam ədəd olmalıdır.')
      return
    }
    if (discNum != null) {
      if (!Number.isFinite(discNum) || discNum <= 0 || discNum >= priceNum) {
        setSubmitError('Endirimli qiymət əsas qiymətdən kiçik və müsbət olmalıdır.')
        return
      }
    }

    const maxCoverBytes = 5 * 1024 * 1024
    let coverUrlFromField: string | undefined
    if (coverFile) {
      if (coverFile.size > maxCoverBytes) {
        setSubmitError('Kapak şəkli 5 MB-dan böyük ola bilməz.')
        return
      }
    } else {
      const u = coverUrlInput.trim()
      if (!u) {
        setSubmitError(
          'Kapak üçün yerində şəkil seçin və ya tam https (və ya http) URL daxil edin.'
        )
        return
      }
      try {
        const parsed = new URL(u)
        if (parsed.protocol !== 'http:' && parsed.protocol !== 'https:') {
          throw new Error('protocol')
        }
      } catch {
        setSubmitError('Şəkil linki keçərli http və ya https ünvanı olmalıdır.')
        return
      }
      coverUrlFromField = u
    }

    const body = {
      name: name.trim(),
      slug: slug.trim(),
      price: priceNum,
      stockQuantity: stockNum,
      categoryId,
      shortDescription: shortDescription.trim() || null,
      description: description.trim() || null,
      sku: sku.trim() || null,
      brand: brand.trim() || null,
      discountedPrice: discNum,
      isFeatured,
    }

    setPending(true)
    try {
      const imageUrl =
        coverFile != null
          ? (await sellerPortalApi.uploadProductCover(coverFile)).url
          : coverUrlFromField!

      const productId = await sellerPortalApi.createProduct(body)

      await sellerPortalApi.addProductImage(productId, {
        url: imageUrl,
        altText: name.trim(),
        displayOrder: 0,
      })

      navigate('/products', { replace: false, state: { created: true } })
    } catch (err) {
      setSubmitError(err instanceof ApiError ? err.message : 'Məhsul və ya şəkil əlavə olunmadı.')
    } finally {
      setPending(false)
    }
  }

  return (
    <div>
      <Link
        to="/products"
        className="mb-6 inline-flex items-center gap-1 text-sm font-medium text-slate-600 hover:text-slate-900 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden />
        Məhsullar siyahısı
      </Link>

      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Yeni məhsul</h1>
      <p className="mt-2 text-slate-600">
        Məcburi sahələri doldurun; slug vitrin URL üçün təkrarsız olmalıdır. Kapak şəkli yerində yükləmə
        və ya tam http(s) link ilə əlavə olunur.
      </p>

      {categoriesError && (
        <p className="mt-6 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {categoriesError}
        </p>
      )}

      {categoriesLoading ? (
        <div className="mt-12 flex justify-center text-slate-500">
          <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : flatCategories.length === 0 && !categoriesError ? (
        <div className="mt-8 space-y-3 rounded-lg border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-950">
          <p className="font-medium">Verilənlər bazasında kateqoriya yoxdur.</p>
          <p className="text-amber-900">
            Məhsul üçün ən azı bir kateqoriya lazımdır. Admin rolü ilə API üzərindən kateqoriya yaradın
            (məs. Swagger-da <span className="font-mono text-xs">POST /api/v1/categories</span>), sonra
            səhifəni yeniləyin.
          </p>
        </div>
      ) : (
        <form
          className="mt-8 max-w-2xl space-y-5 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
          onSubmit={(e) => void onSubmit(e)}
          noValidate
        >
          {inactiveCategoriesFallback && (
            <p
              className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900"
              role="status"
            >
              Aktiv kateqoriya tapılmadığı üçün aktiv olmayan kateqoriyalar da siyahıda göstərilir.
              Canlı mühitdə administrator ən azı bir aktiv kateqoriya təyin etməlidir.
            </p>
          )}
          {submitError && (
            <p className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
              {submitError}
            </p>
          )}

          <div>
            <label htmlFor="p-name" className="block text-sm font-medium text-slate-700">
              Ad <span className="text-red-600">*</span>
            </label>
            <input
              id="p-name"
              type="text"
              required
              value={name}
              onChange={(e) => {
                const v = e.target.value
                setName(v)
                syncSlugFromName(v)
              }}
              onBlur={() => syncSlugFromName(name)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>

          <div>
            <label htmlFor="p-slug" className="block text-sm font-medium text-slate-700">
              Slug <span className="text-red-600">*</span>
            </label>
            <input
              id="p-slug"
              type="text"
              required
              value={slug}
              onChange={(e) => {
                setSlugManual(true)
                setSlug(e.target.value)
              }}
              autoComplete="off"
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
            <p className="mt-1 text-xs text-slate-500">
              Ad dəyişəndə avtomatik doldurulur; ehtiyac varsa əl ilə düzəldə bilərsiniz.
            </p>
          </div>

          <fieldset className="rounded-lg border border-slate-200 bg-slate-50/80 p-4">
            <legend className="px-1 text-sm font-semibold text-slate-800">
              Kapak şəkli <span className="text-red-600">*</span>
            </legend>
            <p className="text-xs text-slate-600">
              JPG, PNG, WEBP və ya GIF (ən çox 5 MB) seçin — və ya CDN-də yerləşən tam şəkil URL-sini
              yazın (biri kifayətdir).
            </p>
            <div className="mt-3">
              <label htmlFor="p-cover-file" className="block text-xs font-medium text-slate-600">
                Fayldan yüklə
              </label>
              <div className="mt-1 flex flex-wrap items-center gap-2">
                <input
                  ref={coverFileInputRef}
                  id="p-cover-file"
                  type="file"
                  accept="image/jpeg,image/png,image/webp,image/gif"
                  onChange={(e) => onCoverFileChange(e.target.files?.[0] ?? null)}
                  className="block min-w-0 flex-1 text-sm text-slate-700 file:mr-3 file:rounded-lg file:border-0 file:bg-[#006437] file:px-3 file:py-2 file:text-sm file:font-semibold file:text-white hover:file:bg-[#005029]"
                />
                {(coverFile || coverPreview) && (
                  <button
                    type="button"
                    onClick={() => onCoverFileChange(null)}
                    className="rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-white"
                  >
                    Faylı sil
                  </button>
                )}
              </div>
            </div>
            {coverPreview && (
              <div className="mt-3">
                <p className="text-xs font-medium text-slate-600">Önizləmə</p>
                <img
                  src={coverPreview}
                  alt=""
                  className="mt-1 h-36 max-w-full rounded-lg border border-slate-200 object-contain bg-white"
                />
              </div>
            )}
            <div className="mt-4">
              <label htmlFor="p-cover-url" className="block text-xs font-medium text-slate-600">
                və ya şəkil URL-si (fayl seçilməyibsə)
              </label>
              <input
                id="p-cover-url"
                type="url"
                inputMode="url"
                placeholder="https://..."
                value={coverUrlInput}
                disabled={Boolean(coverFile)}
                onChange={(e) => setCoverUrlInput(e.target.value)}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-900 placeholder:text-slate-400 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437] disabled:cursor-not-allowed disabled:bg-slate-100"
              />
            </div>
          </fieldset>

          <div>
            <label htmlFor="p-category" className="block text-sm font-medium text-slate-700">
              Kateqoriya <span className="text-red-600">*</span>
            </label>
            <select
              id="p-category"
              required
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            >
              <option value="">Kateqoriya seçin…</option>
              {flatCategories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.label}
                </option>
              ))}
            </select>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="p-price" className="block text-sm font-medium text-slate-700">
                Qiymət (₼) <span className="text-red-600">*</span>
              </label>
              <input
                id="p-price"
                inputMode="decimal"
                required
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                placeholder="0.00"
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 tabular-nums text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
              />
            </div>
            <div>
              <label htmlFor="p-stock" className="block text-sm font-medium text-slate-700">
                Stok <span className="text-red-600">*</span>
              </label>
              <input
                id="p-stock"
                inputMode="numeric"
                required
                value={stockQuantity}
                onChange={(e) => setStockQuantity(e.target.value)}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 tabular-nums text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
              />
            </div>
          </div>

          <div>
            <label htmlFor="p-short" className="block text-sm font-medium text-slate-700">
              Qısa təsvir
            </label>
            <input
              id="p-short"
              type="text"
              value={shortDescription}
              onChange={(e) => setShortDescription(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>

          <div>
            <label htmlFor="p-desc" className="block text-sm font-medium text-slate-700">
              Təsvir
            </label>
            <textarea
              id="p-desc"
              rows={4}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label htmlFor="p-sku" className="block text-sm font-medium text-slate-700">
                SKU
              </label>
              <input
                id="p-sku"
                type="text"
                value={sku}
                onChange={(e) => setSku(e.target.value)}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
              />
            </div>
            <div>
              <label htmlFor="p-brand" className="block text-sm font-medium text-slate-700">
                Brend
              </label>
              <input
                id="p-brand"
                type="text"
                value={brand}
                onChange={(e) => setBrand(e.target.value)}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
              />
            </div>
          </div>

          <div>
            <label htmlFor="p-discount" className="block text-sm font-medium text-slate-700">
              Endirimli qiymət (₼)
            </label>
            <input
              id="p-discount"
              inputMode="decimal"
              value={discountedPrice}
              onChange={(e) => setDiscountedPrice(e.target.value)}
              placeholder="Boş buraxın"
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 tabular-nums text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>

          <div className="flex items-start gap-3">
            <input
              id="p-featured"
              type="checkbox"
              checked={isFeatured}
              onChange={(e) => setIsFeatured(e.target.checked)}
              className="mt-1 h-4 w-4 rounded border-slate-300 text-[#006437] focus:ring-[#006437]"
            />
            <label htmlFor="p-featured" className="text-sm text-slate-700">
              Seçilmiş məhsul kimi işarələ
            </label>
          </div>

          <div className="flex flex-wrap gap-3 pt-2">
            <button
              type="submit"
              disabled={pending || flatCategories.length === 0}
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-[#006437] px-5 py-2.5 text-sm font-semibold text-white hover:bg-[#005029] disabled:opacity-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
            >
              {pending ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
                  Gözləyin…
                </>
              ) : (
                'Yarat'
              )}
            </button>
            <Link
              to="/products"
              className="inline-flex items-center justify-center rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
            >
              Ləğv et
            </Link>
          </div>
        </form>
      )}
    </div>
  )
}
