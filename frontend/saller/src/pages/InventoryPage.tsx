import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { AlertTriangle, Loader2, Package } from 'lucide-react'
import {
  ApiError,
  sellerPortalApi,
  type ProductSummaryResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'
import { formatMoney } from '../lib/formatMoney'

const LOW = 10

export default function InventoryPage() {
  usePageMeta('İnventar')

  const [items, setItems] = useState<ProductSummaryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await sellerPortalApi.listProducts({
        page: 1,
        pageSize: 100,
        sortBy: 'stock_asc',
      })
      setItems(res.items)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'İnventar yüklənmədi.')
      setItems([])
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  const critical = items.filter((p) => p.stockQuantity > 0 && p.stockQuantity <= LOW)
  const out = items.filter((p) => p.stockQuantity <= 0)

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">İnventar</h1>
      <p className="mt-2 max-w-3xl text-slate-600">
        Stok səviyyələri, təhlükəli məhsullar və sıfır stok üzrə sürətli nəzarət. Siyahı ilk 100
        məhsula qədər, ən az stokdan sıralanır.
      </p>

      {error && (
        <p className="mt-6 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </p>
      )}

      <div className="mt-8 grid gap-4 sm:grid-cols-3">
        <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-4 shadow-sm">
          <div className="flex items-center gap-2 text-amber-900">
            <AlertTriangle className="h-5 w-5 shrink-0" aria-hidden />
            <p className="text-xs font-semibold uppercase tracking-wide">Aşağı stok (≤ {LOW})</p>
          </div>
          <p className="mt-2 text-2xl font-bold tabular-nums text-amber-950">
            {loading ? '…' : critical.length}
          </p>
        </div>
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-4 shadow-sm">
          <div className="flex items-center gap-2 text-red-900">
            <Package className="h-5 w-5 shrink-0" aria-hidden />
            <p className="text-xs font-semibold uppercase tracking-wide">Stoksuz</p>
          </div>
          <p className="mt-2 text-2xl font-bold tabular-nums text-red-950">
            {loading ? '…' : out.length}
          </p>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white px-4 py-4 shadow-sm">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
            Növbəti addım
          </p>
          <p className="mt-2 text-sm text-slate-700">
            Stoku yeniləmək üçün məhsul səhifəsində redaktə edin və ya qiymət/stok PATCH əməliyyatından
            istifadə edin.
          </p>
        </div>
      </div>

      {loading ? (
        <div className="mt-10 flex justify-center py-16 text-slate-500">
          <Loader2 className="h-9 w-9 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : (
        <div className="mt-10 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Məhsul</th>
                <th className="hidden px-4 py-3 sm:table-cell">Kateqoriya</th>
                <th className="px-4 py-3 text-right tabular-nums">Stok</th>
                <th className="hidden px-4 py-3 text-right tabular-nums md:table-cell">Qiymət</th>
                <th className="px-4 py-3 text-right w-[1%]" />
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {items.map((p) => (
                <tr key={p.id} className="hover:bg-slate-50/80">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      {p.primaryImageUrl ? (
                        <img
                          src={p.primaryImageUrl}
                          alt=""
                          className="h-10 w-10 rounded-lg object-cover ring-1 ring-slate-200"
                        />
                      ) : (
                        <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-slate-100 text-slate-400">
                          <Package className="h-5 w-5" aria-hidden />
                        </span>
                      )}
                      <div className="min-w-0">
                        <p className="truncate font-medium text-slate-900">{p.name}</p>
                        <p className="truncate text-xs text-slate-500">{p.slug}</p>
                      </div>
                    </div>
                  </td>
                  <td className="hidden px-4 py-3 text-slate-600 sm:table-cell">{p.categoryName}</td>
                  <td className="px-4 py-3 text-right">
                    <span
                      className={`inline-flex min-w-[2.5rem] justify-end rounded-md px-2 py-0.5 text-sm font-semibold tabular-nums ${
                        p.stockQuantity <= 0
                          ? 'bg-red-100 text-red-900'
                          : p.stockQuantity <= LOW
                            ? 'bg-amber-100 text-amber-950'
                            : 'bg-slate-100 text-slate-800'
                      }`}
                    >
                      {p.stockQuantity}
                    </span>
                  </td>
                  <td className="hidden px-4 py-3 text-right tabular-nums text-slate-800 md:table-cell">
                    {formatMoney(p.effectivePrice)}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Link
                      to={`/products/${p.slug}/edit`}
                      className="text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
                    >
                      Redaktə
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {items.length === 0 && !error && (
            <p className="px-4 py-12 text-center text-sm text-slate-600">Məhsul tapılmadı.</p>
          )}
        </div>
      )}
    </div>
  )
}
