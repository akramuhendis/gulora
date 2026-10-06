import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Loader2 } from 'lucide-react'
import {
  ApiError,
  catalogApi,
  sellerPortalApi,
  type CategoryResponse,
  type ProductDetailResponse,
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
    if (c.children?.length) out.push(...flattenCategories(c.children, label))
  }
  return out
}

export default function ProductEditPage() {
  usePageMeta('Məhsulu redaktə et')
  const navigate = useNavigate()
  const { slug = '' } = useParams()

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [product, setProduct] = useState<ProductDetailResponse | null>(null)
  const [categories, setCategories] = useState<CategoryResponse[]>([])

  const [name, setName] = useState('')
  const [productSlug, setProductSlug] = useState('')
  const [slugManual, setSlugManual] = useState(true)
  const [categoryId, setCategoryId] = useState('')
  const [price, setPrice] = useState('')
  const [discountedPrice, setDiscountedPrice] = useState('')
  const [stockQuantity, setStockQuantity] = useState('')
  const [shortDescription, setShortDescription] = useState('')
  const [description, setDescription] = useState('')
  const [brand, setBrand] = useState('')
  const [isFeatured, setIsFeatured] = useState(false)

  const flatCategories = useMemo(() => flattenCategories(categories), [categories])

  const load = useCallback(async () => {
    if (!slug.trim()) {
      setError('Məhsul slug tapılmadı.')
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    try {
      const [cats, detail] = await Promise.all([
        catalogApi.listCategories(false),
        sellerPortalApi.getProductBySlug(slug),
      ])
      setCategories(cats)
      setProduct(detail)
      setName(detail.name)
      setProductSlug(detail.slug)
      setCategoryId(detail.categoryId)
      setPrice(String(detail.price))
      setDiscountedPrice(detail.discountedPrice == null ? '' : String(detail.discountedPrice))
      setStockQuantity(String(detail.stockQuantity))
      setShortDescription(detail.shortDescription ?? '')
      setDescription(detail.description ?? '')
      setBrand(detail.brand ?? '')
      setIsFeatured(detail.isFeatured)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Məhsul məlumatları yüklənmədi.')
    } finally {
      setLoading(false)
    }
  }, [slug])

  useEffect(() => {
    queueMicrotask(() => {
      void load()
    })
  }, [load])

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError(null)
    try {
      if (!product) throw new Error('Məhsul tapılmadı.')
      if (!name.trim()) throw new Error('Məhsul adını daxil edin.')
      if (!productSlug.trim()) throw new Error('Slug boş ola bilməz.')
      if (!categoryId) throw new Error('Kateqoriya seçin.')

      const priceNum = Number(price.replace(',', '.'))
      if (!Number.isFinite(priceNum) || priceNum <= 0) {
        throw new Error('Qiymət müsbət ədəd olmalıdır.')
      }
      const discRaw = discountedPrice.trim()
      const discNum = discRaw === '' ? null : Number(discRaw.replace(',', '.'))
      const stockNum = Number.parseInt(stockQuantity, 10)
      if (discNum != null && (!Number.isFinite(discNum) || discNum <= 0 || discNum >= priceNum)) {
        throw new Error('Endirimli qiymət əsas qiymətdən kiçik və müsbət olmalıdır.')
      }
      if (!Number.isFinite(stockNum) || stockNum < 0) {
        throw new Error('Stok 0 və ya daha böyük tam ədəd olmalıdır.')
      }

      await sellerPortalApi.updateProduct(product.id, {
        name: name.trim(),
        slug: productSlug.trim(),
        description: description.trim() || null,
        shortDescription: shortDescription.trim() || null,
        brand: brand.trim() || null,
        categoryId,
        isFeatured,
      })

      await sellerPortalApi.updateProductPrice(product.id, priceNum, discNum)
      await sellerPortalApi.updateProductStock(product.id, stockNum)

      navigate('/products', {
        replace: false,
        state: { updated: true, productName: name.trim() },
      })
    } catch (err) {
      if (err instanceof ApiError) setError(err.message)
      else if (err instanceof Error) setError(err.message)
      else setError('Məhsul yenilənmədi.')
    } finally {
      setSaving(false)
    }
  }

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
        to="/products"
        className="mb-6 inline-flex items-center gap-1 text-sm font-medium text-slate-600 hover:text-slate-900"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden />
        Məhsullara qayıt
      </Link>

      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Məhsulu redaktə et</h1>
      <p className="mt-2 text-slate-600">Məhsul məlumatlarını yeniləyin və dəyişiklikləri saxlayın.</p>

      {error && (
        <p className="mt-6 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </p>
      )}

      <form
        className="mt-8 max-w-2xl space-y-5 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
        onSubmit={(e) => void onSubmit(e)}
        noValidate
      >
        <div>
          <label htmlFor="e-name" className="block text-sm font-medium text-slate-700">
            Ad
          </label>
          <input
            id="e-name"
            type="text"
            required
            value={name}
            onChange={(e) => {
              const v = e.target.value
              setName(v)
              if (!slugManual) setProductSlug(slugifyAz(v))
            }}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
          />
        </div>

        <div>
          <label htmlFor="e-slug" className="block text-sm font-medium text-slate-700">
            Slug
          </label>
          <input
            id="e-slug"
            type="text"
            required
            value={productSlug}
            onChange={(e) => {
              setSlugManual(true)
              setProductSlug(
                e.target.value
                  .toLowerCase()
                  .replace(/[^a-z0-9-]/g, '')
                  .replace(/-+/g, '-')
              )
            }}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 font-mono text-sm text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
          />
        </div>

        <div>
          <label htmlFor="e-category" className="block text-sm font-medium text-slate-700">
            Kateqoriya
          </label>
          <select
            id="e-category"
            required
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
          >
            <option value="">Kateqoriya seçin...</option>
            {flatCategories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.label}
              </option>
            ))}
          </select>
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <div>
            <label htmlFor="e-price" className="block text-sm font-medium text-slate-700">
              Qiymət (AZN)
            </label>
            <input
              id="e-price"
              inputMode="decimal"
              required
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 tabular-nums text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>
          <div>
            <label htmlFor="e-discount" className="block text-sm font-medium text-slate-700">
              Endirimli qiymət (AZN)
            </label>
            <input
              id="e-discount"
              inputMode="decimal"
              value={discountedPrice}
              onChange={(e) => setDiscountedPrice(e.target.value)}
              placeholder="Boş buraxın"
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 tabular-nums text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>
          <div>
            <label htmlFor="e-stock" className="block text-sm font-medium text-slate-700">
              Stok
            </label>
            <input
              id="e-stock"
              inputMode="numeric"
              required
              value={stockQuantity}
              onChange={(e) => setStockQuantity(e.target.value)}
              className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 tabular-nums text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
            />
          </div>
        </div>

        <div>
          <label htmlFor="e-short" className="block text-sm font-medium text-slate-700">
            Qısa təsvir
          </label>
          <input
            id="e-short"
            type="text"
            value={shortDescription}
            onChange={(e) => setShortDescription(e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
          />
        </div>

        <div>
          <label htmlFor="e-description" className="block text-sm font-medium text-slate-700">
            Təsvir
          </label>
          <textarea
            id="e-description"
            rows={4}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
          />
        </div>

        <div>
          <label htmlFor="e-brand" className="block text-sm font-medium text-slate-700">
            Brend
          </label>
          <input
            id="e-brand"
            type="text"
            value={brand}
            onChange={(e) => setBrand(e.target.value)}
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-900 focus:border-[#006437] focus:outline-none focus:ring-2 focus:ring-[#006437]"
          />
        </div>

        <div className="flex items-start gap-3">
          <input
            id="e-featured"
            type="checkbox"
            checked={isFeatured}
            onChange={(e) => setIsFeatured(e.target.checked)}
            className="mt-1 h-4 w-4 rounded border-slate-300 text-[#006437] focus:ring-[#006437]"
          />
          <label htmlFor="e-featured" className="text-sm text-slate-700">
            Seçilmiş məhsul kimi işarələ
          </label>
        </div>

        <div className="flex flex-wrap gap-3 pt-2">
          <button
            type="submit"
            disabled={saving}
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-[#006437] px-5 py-2.5 text-sm font-semibold text-white hover:bg-[#005029] disabled:opacity-50"
          >
            {saving ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
                Gözləyin...
              </>
            ) : (
              'Yenilə'
            )}
          </button>
          <Link
            to="/products"
            className="inline-flex items-center justify-center rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            Ləğv et
          </Link>
        </div>
      </form>
    </div>
  )
}

