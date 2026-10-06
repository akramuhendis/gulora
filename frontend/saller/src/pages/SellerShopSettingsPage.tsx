import { useCallback, useEffect, useRef, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { ArrowLeft, Loader2, Store } from 'lucide-react'
import { ApiError, sellerShopApi, type SellerShopMyResponse } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

function slugifyShop(text: string): string {
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

export default function SellerShopSettingsPage() {
  usePageMeta('Mağaza vitrinim')

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [existing, setExisting] = useState<SellerShopMyResponse | null>(null)

  const [storeName, setStoreName] = useState('')
  const [storeSlug, setStoreSlug] = useState('')
  const [slugManual, setSlugManual] = useState(false)
  const [description, setDescription] = useState('')
  const [logoUrl, setLogoUrl] = useState('')
  const [bannerUrl, setBannerUrl] = useState('')
  const [logoFile, setLogoFile] = useState<File | null>(null)
  const [logoPreview, setLogoPreview] = useState<string | null>(null)
  const logoFileInputRef = useRef<HTMLInputElement>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const shop = await sellerShopApi.getMine()
      setExisting(shop)
      if (shop) {
        setStoreName(shop.storeName)
        setStoreSlug(shop.storeSlug)
        setSlugManual(true)
        setDescription(shop.description ?? '')
        setLogoUrl(shop.logoUrl ?? '')
        setBannerUrl(shop.bannerUrl ?? '')
      }
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Məlumat yüklənmədi.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => {
      void load()
    })
  }, [load])

  useEffect(() => {
    return () => {
      if (logoPreview) URL.revokeObjectURL(logoPreview)
    }
  }, [logoPreview])

  function onLogoFileChange(file: File | null) {
    setLogoPreview((prev) => {
      if (prev) URL.revokeObjectURL(prev)
      return file ? URL.createObjectURL(file) : null
    })
    setLogoFile(file)
    if (file) setLogoUrl('')
    if (!file && logoFileInputRef.current) logoFileInputRef.current.value = ''
  }

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setError(null)
    setSuccess(null)
    try {
      const maxLogoBytes = 5 * 1024 * 1024
      const allowedLogoTypes = new Set(['image/jpeg', 'image/png', 'image/webp', 'image/gif'])
      let resolvedLogoUrl = logoUrl.trim() || null
      if (logoFile) {
        if (logoFile.size > maxLogoBytes) {
          setError('Profil şəkli 5 MB-dan böyük ola bilməz.')
          return
        }
        if (logoFile.type && !allowedLogoTypes.has(logoFile.type)) {
          setError('Yalnız JPG, PNG, WEBP və ya GIF profil şəkli yükləyə bilərsiniz.')
          return
        }
        resolvedLogoUrl = (await sellerShopApi.uploadShopLogo(logoFile)).url
      }

      const updated = await sellerShopApi.upsert({
        storeName: storeName.trim(),
        storeSlug: storeSlug.trim(),
        description: description.trim() || null,
        logoUrl: resolvedLogoUrl,
        bannerUrl: bannerUrl.trim() || null,
      })
      setExisting(updated)
      setSuccess('Mağaza məlumatları saxlanıldı.')
      onLogoFileChange(null)
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else if (err instanceof Error && err.message.trim()) {
        setError(err.message)
      } else {
        setError('Saxlanılmadı.')
      }
    } finally {
      setSaving(false)
    }
  }

  const publicStoreBase = import.meta.env.VITE_PUBLIC_STORE_URL ?? ''

  const previewHref =
    existing && publicStoreBase
      ? `${publicStoreBase.replace(/\/$/, '')}/magaza/${encodeURIComponent(existing.storeSlug)}`
      : null

  if (loading) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center text-slate-500">
        <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
      </div>
    )
  }

  return (
    <div>
      <Link
        to="/"
        className="mb-6 inline-flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-[#006437]"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden />
        İcmala qayıt
      </Link>

      <div className="mb-6 flex items-start gap-3">
        <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-[#006437]/10 text-[#006437]">
          <Store className="h-7 w-7" aria-hidden />
        </div>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Mağaza vitrinim</h1>
          <p className="mt-1 text-sm text-slate-600">
            Müştəri vitrinində mağaza adınız, ünvan (slug) və təsvir görünür — Trendyol tipli pazaryeri.
          </p>
        </div>
      </div>

      {error && (
        <div
          className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800"
          role="alert"
        >
          {error}
        </div>
      )}
      {success && (
        <div
          className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-900"
          role="status"
        >
          {success}
        </div>
      )}

      <form
        onSubmit={(e) => void onSubmit(e)}
        className="max-w-2xl space-y-5 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
      >
        <div>
          <label htmlFor="storeName" className="block text-sm font-medium text-slate-700">
            Mağaza adı
          </label>
          <input
            id="storeName"
            type="text"
            required
            maxLength={200}
            value={storeName}
            onChange={(e) => {
              const nextName = e.target.value
              setStoreName(nextName)
              if (!slugManual) setStoreSlug(slugifyShop(nextName))
            }}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 shadow-sm focus:border-[#006437] focus:outline-none focus:ring-1 focus:ring-[#006437]"
            placeholder="Məs. Gül Bağı Vitrini"
          />
        </div>

        <div>
          <label htmlFor="storeSlug" className="block text-sm font-medium text-slate-700">
            Mağaza ünvanı (slug)
          </label>
          <input
            id="storeSlug"
            type="text"
            required
            maxLength={120}
            value={storeSlug}
            onChange={(e) => {
              setSlugManual(true)
              setStoreSlug(
                e.target.value
                  .toLowerCase()
                  .replace(/[^a-z0-9-]/g, '')
                  .replace(/-+/g, '-')
              )
            }}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm text-slate-900 shadow-sm focus:border-[#006437] focus:outline-none focus:ring-1 focus:ring-[#006437]"
            placeholder="gul-bagi-vitrini"
          />
          <p className="mt-1 text-xs text-slate-500">
            Yalnız kiçik hərf, rəqəm və tire. Müştəri vitrinində: /magaza/<strong>{storeSlug || '…'}</strong>
          </p>
        </div>

        <div>
          <label htmlFor="description" className="block text-sm font-medium text-slate-700">
            Təsvir
          </label>
          <textarea
            id="description"
            rows={4}
            maxLength={8000}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 shadow-sm focus:border-[#006437] focus:outline-none focus:ring-1 focus:ring-[#006437]"
            placeholder="Mağazanızı təqdim edin…"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700">Profil şəkli</label>
          <div className="mt-1 rounded-lg border border-slate-200 bg-slate-50 p-3">
            <p className="text-xs text-slate-600">JPG, PNG, WEBP və ya GIF (maks. 5 MB)</p>
            <div className="mt-2 flex flex-wrap items-center gap-2">
              <input
                ref={logoFileInputRef}
                type="file"
                accept="image/jpeg,image/png,image/webp,image/gif"
                onChange={(e) => onLogoFileChange(e.target.files?.[0] ?? null)}
                className="block min-w-0 flex-1 text-sm text-slate-700 file:mr-3 file:rounded-lg file:border-0 file:bg-[#006437] file:px-3 file:py-2 file:text-sm file:font-semibold file:text-white hover:file:bg-[#005029]"
              />
              {(logoFile || logoPreview) && (
                <button
                  type="button"
                  onClick={() => onLogoFileChange(null)}
                  className="rounded-lg border border-slate-300 px-3 py-2 text-xs font-medium text-slate-700 hover:bg-white"
                >
                  Sil
                </button>
              )}
            </div>
            {(logoPreview || logoUrl) && (
              <div className="mt-3">
                <p className="text-xs font-medium text-slate-600">Önizləmə</p>
                <img
                  src={logoPreview ?? logoUrl}
                  alt="Mağaza profil şəkli önizləmə"
                  className="mt-1 h-24 w-24 rounded-full border border-slate-200 object-cover bg-white"
                />
              </div>
            )}
          </div>
        </div>

        <div>
          <label htmlFor="logoUrl" className="block text-sm font-medium text-slate-700">
            Logo URL
          </label>
          <input
            id="logoUrl"
            type="url"
            value={logoUrl}
            onChange={(e) => setLogoUrl(e.target.value)}
            disabled={Boolean(logoFile)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 shadow-sm focus:border-[#006437] focus:outline-none focus:ring-1 focus:ring-[#006437] disabled:cursor-not-allowed disabled:bg-slate-100"
            placeholder="https://..."
          />
          <p className="mt-1 text-xs text-slate-500">
            Fayl seçsəniz bu sahə avtomatik olaraq yüklənən URL ilə əvəz olunacaq.
          </p>
        </div>

        <div>
          <label htmlFor="bannerUrl" className="block text-sm font-medium text-slate-700">
            Banner URL
          </label>
          <input
            id="bannerUrl"
            type="url"
            value={bannerUrl}
            onChange={(e) => setBannerUrl(e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 shadow-sm focus:border-[#006437] focus:outline-none focus:ring-1 focus:ring-[#006437]"
            placeholder="https://..."
          />
        </div>

        {previewHref && (
          <p className="text-sm text-slate-600">
            Müştəri vitrinində baxış:{' '}
            <a
              href={previewHref}
              target="_blank"
              rel="noopener noreferrer"
              className="font-medium text-[#006437] underline-offset-2 hover:underline"
            >
              {previewHref}
            </a>
          </p>
        )}
        {!previewHref && existing && (
          <p className="text-sm text-slate-500">
            Önizləmə linki üçün satıcı paneli <code className="rounded bg-slate-100 px-1">.env</code>{' '}
            faylına <code className="rounded bg-slate-100 px-1">VITE_PUBLIC_STORE_URL</code> əlavə edin
            (məs. <code className="rounded bg-slate-100 px-1">http://localhost:5173</code>).
          </p>
        )}

        <button
          type="submit"
          disabled={saving}
          className="inline-flex items-center justify-center rounded-lg bg-[#006437] px-5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-[#004d2a] disabled:opacity-60"
        >
          {saving ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden />
              Saxlanılır…
            </>
          ) : (
            'Saxla'
          )}
        </button>
      </form>
    </div>
  )
}
