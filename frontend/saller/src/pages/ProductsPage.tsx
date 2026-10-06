import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router-dom'
import { CheckCircle2, ChevronLeft, ChevronRight, Loader2, Plus, RotateCcw, Trash2 } from 'lucide-react'
import {
  ApiError,
  sellerPortalApi,
  type ProductSummaryResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

function formatMoney(n: number): string {
  return (
    new Intl.NumberFormat('az-AZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
      n
    ) + ' ₼'
  )
}

export default function ProductsPage() {
  usePageMeta('Məhsullar')
  const location = useLocation()
  const [searchParams, setSearchParams] = useSearchParams()
  const [page, setPage] = useState(() => Math.max(1, Number(searchParams.get('page') || '1') || 1))
  const pageSize = 12
  const [queryInput, setQueryInput] = useState(searchParams.get('q') ?? '')
  const [statusFilter, setStatusFilter] = useState(searchParams.get('status') ?? 'all')
  const [stockFilter, setStockFilter] = useState(searchParams.get('stock') ?? 'all')
  const [sortBy, setSortBy] = useState(searchParams.get('sort') ?? 'newest')
  const [totalPages, setTotalPages] = useState(1)
  const [items, setItems] = useState<ProductSummaryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [productActionId, setProductActionId] = useState<string | null>(null)

  const normalizedStatus = useMemo(() => {
    if (statusFilter === 'active') return true
    if (statusFilter === 'inactive') return false
    return undefined
  }, [statusFilter])

  const normalizedStock = useMemo(() => {
    if (stockFilter === 'in') return true
    if (stockFilter === 'out') return false
    return undefined
  }, [stockFilter])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await sellerPortalApi.listProducts({
        page,
        pageSize,
        query: queryInput,
        isActive: normalizedStatus,
        inStock: normalizedStock,
        sortBy,
      })
      setItems(res.items)
      const tp =
        res.totalPages ??
        (res.pageSize > 0 ? Math.ceil(res.totalCount / res.pageSize) : 0)
      setTotalPages(Math.max(1, tp || 1))
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Məhsullar yüklənmədi.')
      setItems([])
    } finally {
      setLoading(false)
    }
  }, [normalizedStatus, normalizedStock, page, queryInput, sortBy])

  useEffect(() => {
    queueMicrotask(() => {
      void load()
    })
  }, [load])

  useEffect(() => {
    const next = new URLSearchParams()
    next.set('page', String(page))
    if (queryInput.trim()) next.set('q', queryInput.trim())
    if (statusFilter !== 'all') next.set('status', statusFilter)
    if (stockFilter !== 'all') next.set('stock', stockFilter)
    if (sortBy !== 'newest') next.set('sort', sortBy)
    setSearchParams(next, { replace: true })
  }, [page, queryInput, setSearchParams, sortBy, statusFilter, stockFilter])

  const flashState = location.state as
    | { created?: boolean; updated?: boolean; productName?: string }
    | undefined

  async function handleDeactivate(product: ProductSummaryResponse) {
    if (productActionId) return
    const ok = window.confirm(`"${product.name}" məhsulunu passiv etmək istədiyinizə əminsiniz?`)
    if (!ok) return
    setProductActionId(product.id)
    setError(null)
    try {
      await sellerPortalApi.deleteProduct(product.id)
      if (items.length === 1 && page > 1) {
        setPage((p) => Math.max(1, p - 1))
      } else {
        await load()
      }
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Məhsul passiv edilə bilmədi.')
    } finally {
      setProductActionId(null)
    }
  }

  async function handleActivate(product: ProductSummaryResponse) {
    if (productActionId) return
    const ok = window.confirm(`"${product.name}" məhsulunu aktivləşdirmək istədiyinizə əminsiniz?`)
    if (!ok) return
    setProductActionId(product.id)
    setError(null)
    try {
      await sellerPortalApi.activateProduct(product.id)
      await load()
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Məhsul aktivləşdirilmədi.')
    } finally {
      setProductActionId(null)
    }
  }

  function resetFilters() {
    setQueryInput('')
    setStatusFilter('all')
    setStockFilter('all')
    setSortBy('newest')
    setPage(1)
  }

  return (
    <div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
            Məhsullarım
          </h1>
          <p className="mt-2 text-slate-600">
            Qiymət, stok və satış statusunu bir cədvəldə görüntüləyin; siyahı səhifələrə bölünür.
          </p>
        </div>
        <Link
          to="/products/new"
          className="inline-flex shrink-0 items-center justify-center gap-2 rounded-lg bg-[#006437] px-4 py-2.5 text-sm font-semibold text-white hover:bg-[#005029] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          <Plus className="h-4 w-4" aria-hidden />
          Yeni məhsul
        </Link>
      </div>

      {flashState?.created && (
        <p className="mt-4 rounded-lg bg-green-50 px-4 py-3 text-sm text-green-900" role="status">
          Məhsul uğurla yaradıldı.
        </p>
      )}
      {flashState?.updated && (
        <p className="mt-4 rounded-lg bg-emerald-50 px-4 py-3 text-sm text-emerald-900" role="status">
          {(flashState.productName ?? 'Məhsul')} uğurla yeniləndi.
        </p>
      )}

      {error && (
        <div className="mt-6 flex items-center justify-between gap-3 rounded-lg bg-red-50 px-4 py-3">
          <p className="text-sm text-red-800" role="alert">
            {error}
          </p>
          <button
            type="button"
            onClick={() => void load()}
            className="inline-flex items-center gap-1 rounded-md border border-red-200 bg-white px-2.5 py-1 text-xs font-medium text-red-800 hover:bg-red-100"
          >
            <RotateCcw className="h-3.5 w-3.5" aria-hidden />
            Yenidən yoxla
          </button>
        </div>
      )}

      <div className="mt-6 flex flex-wrap items-end gap-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div className="min-w-[220px] flex-1">
          <label htmlFor="product-search" className="block text-xs font-medium text-slate-600">
            Məhsul axtar
          </label>
          <input
            id="product-search"
            type="search"
            value={queryInput}
            onChange={(e) => {
              setQueryInput(e.target.value)
              setPage(1)
            }}
            placeholder="Ad, slug, SKU, brend..."
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label htmlFor="status-filter" className="block text-xs font-medium text-slate-600">
            Aktivlik
          </label>
          <select
            id="status-filter"
            value={statusFilter}
            onChange={(e) => {
              setStatusFilter(e.target.value)
              setPage(1)
            }}
            className="mt-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="all">Hamısı</option>
            <option value="active">Aktiv</option>
            <option value="inactive">Passiv</option>
          </select>
        </div>
        <div>
          <label htmlFor="stock-filter" className="block text-xs font-medium text-slate-600">
            Stok
          </label>
          <select
            id="stock-filter"
            value={stockFilter}
            onChange={(e) => {
              setStockFilter(e.target.value)
              setPage(1)
            }}
            className="mt-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="all">Hamısı</option>
            <option value="in">Stokda var</option>
            <option value="out">Stokda yoxdur</option>
          </select>
        </div>
        <div>
          <label htmlFor="sort-filter" className="block text-xs font-medium text-slate-600">
            Sıralama
          </label>
          <select
            id="sort-filter"
            value={sortBy}
            onChange={(e) => {
              setSortBy(e.target.value)
              setPage(1)
            }}
            className="mt-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="newest">Ən yeni</option>
            <option value="oldest">Ən köhnə</option>
            <option value="name_asc">Ad A-Z</option>
            <option value="name_desc">Ad Z-A</option>
            <option value="price_asc">Qiymət artan</option>
            <option value="price_desc">Qiymət azalan</option>
            <option value="stock_asc">Stok artan</option>
            <option value="stock_desc">Stok azalan</option>
          </select>
        </div>
        <button
          type="button"
          onClick={resetFilters}
          className="rounded-lg border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
        >
          Sıfırla
        </button>
      </div>

      {loading ? (
        <div className="mt-12 flex justify-center text-slate-500">
          <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : (
        <>
          <div className="mt-6 overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
            <table className="min-w-[820px] w-full text-left text-sm">
              <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Məhsul</th>
                  <th className="px-4 py-3">Kateqoriya</th>
                  <th className="px-4 py-3 text-right">Qiymət</th>
                  <th className="px-4 py-3 text-right">Stok</th>
                  <th className="px-4 py-3">Satış</th>
                  <th className="px-4 py-3 text-right">Əməliyyat</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-4 py-10 text-center text-slate-500">
                      Məhsul tapılmadı.
                    </td>
                  </tr>
                ) : (
                  items.map((p) => (
                    <tr key={p.id} className="hover:bg-slate-50/80">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          {p.primaryImageUrl ? (
                            <img
                              src={p.primaryImageUrl}
                              alt={`${p.name} şəkli`}
                              className="h-10 w-10 rounded-md object-cover"
                            />
                          ) : (
                            <div className="h-10 w-10 rounded-md bg-slate-200" />
                          )}
                          <div>
                            <p className="font-medium text-slate-900">{p.name}</p>
                            <p className="text-xs text-slate-500">{p.slug}</p>
                            <Link
                              to={`/products/${encodeURIComponent(p.slug)}/edit`}
                              className="mt-1 inline-block text-xs font-medium text-[#006437] hover:underline"
                            >
                              Redaktə et
                            </Link>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-slate-600">{p.categoryName}</td>
                      <td className="px-4 py-3 text-right tabular-nums">
                        {formatMoney(p.effectivePrice)}
                        {p.discountedPrice != null && (
                          <span className="ml-2 text-xs text-slate-400 line-through">
                            {formatMoney(p.price)}
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3 text-right tabular-nums">{p.stockQuantity}</td>
                      <td className="px-4 py-3">
                        <span
                          className={
                            p.isAvailableForPurchase
                              ? 'rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800'
                              : !p.isActive
                                ? 'rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-600'
                                : 'rounded-full bg-amber-100 px-2 py-0.5 text-xs font-medium text-amber-900'
                          }
                        >
                          {p.isAvailableForPurchase
                            ? 'Aktiv'
                            : !p.isActive
                              ? 'Passiv'
                              : 'Stoksuz'}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right">
                        {!p.isActive ? (
                          <button
                            type="button"
                            onClick={() => void handleActivate(p)}
                            disabled={productActionId === p.id}
                            className="inline-flex items-center gap-1 rounded-lg border border-emerald-200 px-2.5 py-1.5 text-xs font-medium text-emerald-800 hover:bg-emerald-50 disabled:opacity-50"
                          >
                            <CheckCircle2 className="h-3.5 w-3.5" aria-hidden />
                            {productActionId === p.id ? 'Gözləyin...' : 'Aktiv et'}
                          </button>
                        ) : (
                          <button
                            type="button"
                            onClick={() => void handleDeactivate(p)}
                            disabled={productActionId === p.id}
                            className="inline-flex items-center gap-1 rounded-lg border border-red-200 px-2.5 py-1.5 text-xs font-medium text-red-700 hover:bg-red-50 disabled:opacity-50"
                          >
                            <Trash2 className="h-3.5 w-3.5" aria-hidden />
                            {productActionId === p.id ? 'Gözləyin...' : 'Passiv et'}
                          </button>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          <div className="mt-4 flex items-center justify-between">
            <p className="text-sm text-slate-500">
              Səhifə {page} / {totalPages}
            </p>
            <div className="flex gap-2">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                className="inline-flex items-center gap-1 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm disabled:opacity-40"
              >
                <ChevronLeft className="h-4 w-4" />
                Əvvəlki
              </button>
              <button
                type="button"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="inline-flex items-center gap-1 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm disabled:opacity-40"
              >
                Sonrakı
                <ChevronRight className="h-4 w-4" />
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  )
}
