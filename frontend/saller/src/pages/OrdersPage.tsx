import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronLeft, ChevronRight, Loader2 } from 'lucide-react'
import {
  ApiError,
  ORDER_STATUS_VALUES,
  sellerPortalApi,
  type OrderSummaryResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'
import { prefetchOnIntent } from '../lib/prefetchSellerRoutes'
import { orderDisplayTotalForSeller } from '../lib/orderDisplayAmount'

const QUICK_TABS: { id: string; label: string; status: string }[] = [
  { id: 'all', label: 'Hamısı', status: '' },
  { id: 'Pending', label: 'Yeni', status: 'Pending' },
  { id: 'Processing', label: 'Hazırlıq', status: 'Processing' },
  { id: 'Shipped', label: 'Yolda', status: 'Shipped' },
  { id: 'Delivered', label: 'Çatdırılıb', status: 'Delivered' },
  { id: 'ReturnRequested', label: 'İadə', status: 'ReturnRequested' },
  { id: 'Cancelled', label: 'Ləğv', status: 'Cancelled' },
]

function formatMoney(n: number): string {
  return (
    new Intl.NumberFormat('az-AZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
      n
    ) + ' ₼'
  )
}

export default function OrdersPage() {
  usePageMeta('Sifarişlər')
  const [page, setPage] = useState(1)
  const pageSize = 15
  const [totalPages, setTotalPages] = useState(1)
  const [items, setItems] = useState<OrderSummaryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [draftStatus, setDraftStatus] = useState('')
  const [draftOrderNumber, setDraftOrderNumber] = useState('')
  const [draftDateFrom, setDraftDateFrom] = useState('')
  const [draftDateTo, setDraftDateTo] = useState('')
  const [filters, setFilters] = useState({
    status: '',
    orderNumber: '',
    dateFrom: '' as string,
    dateTo: '' as string,
  })

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await sellerPortalApi.listOrders({
        page,
        pageSize,
        status: filters.status || undefined,
        orderNumber: filters.orderNumber.trim() || undefined,
        dateFrom: filters.dateFrom || undefined,
        dateTo: filters.dateTo || undefined,
      })
      setItems(res.items)
      const tp =
        res.totalPages ??
        (res.pageSize > 0 ? Math.ceil(res.totalCount / res.pageSize) : 0)
      setTotalPages(Math.max(1, tp || 1))
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Sifarişlər yüklənmədi.')
      setItems([])
    } finally {
      setLoading(false)
    }
  }, [page, pageSize, filters])

  useEffect(() => {
    queueMicrotask(() => {
      void load()
    })
  }, [load])

  function applyFilters() {
    setFilters({
      status: draftStatus,
      orderNumber: draftOrderNumber.trim(),
      dateFrom: draftDateFrom.trim(),
      dateTo: draftDateTo.trim(),
    })
    setPage(1)
  }

  function pickQuickTab(status: string) {
    setDraftStatus(status)
    setFilters((f) => ({
      ...f,
      status,
    }))
    setPage(1)
  }

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Sifarişlər</h1>
      <p className="mt-2 text-slate-600">
        Öz məhsullarınıza düşən sifarişləri status və nömrəyə görə süzün. Məbləğ sütunu{' '}
        <strong>sizə aid məhsul sətirləri</strong>nin cəmini göstərir.
      </p>

      <div
        className="mt-6 flex gap-2 overflow-x-auto pb-1 [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
        role="tablist"
        aria-label="Sürətli status süzgəci"
      >
        {QUICK_TABS.map((t) => {
          const active = filters.status === t.status
          return (
            <button
              key={t.id}
              type="button"
              role="tab"
              aria-selected={active}
              onClick={() => pickQuickTab(t.status)}
              className={`shrink-0 rounded-full px-3 py-1.5 text-sm font-semibold transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2 ${
                active
                  ? 'bg-[#006437] text-white shadow-sm'
                  : 'border border-slate-200 bg-white text-slate-800 hover:bg-slate-50'
              }`}
            >
              {t.label}
            </button>
          )
        })}
      </div>

      <div className="mt-6 flex flex-wrap items-end gap-3 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div>
          <label htmlFor="filter-status" className="block text-xs font-medium text-slate-600">
            Status
          </label>
          <select
            id="filter-status"
            value={draftStatus}
            onChange={(e) => setDraftStatus(e.target.value)}
            className="mt-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="">Hamısı</option>
            {ORDER_STATUS_VALUES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </div>
        <div className="min-w-[180px] flex-1">
          <label htmlFor="filter-order" className="block text-xs font-medium text-slate-600">
            Sifariş nömrəsi
          </label>
          <input
            id="filter-order"
            type="search"
            value={draftOrderNumber}
            onChange={(e) => setDraftOrderNumber(e.target.value)}
            placeholder="Məs: ORD-"
            className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label htmlFor="filter-from" className="block text-xs font-medium text-slate-600">
            Başlanğıc tarixi
          </label>
          <input
            id="filter-from"
            type="date"
            value={draftDateFrom}
            onChange={(e) => setDraftDateFrom(e.target.value)}
            className="mt-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </div>
        <div>
          <label htmlFor="filter-to" className="block text-xs font-medium text-slate-600">
            Son tarix
          </label>
          <input
            id="filter-to"
            type="date"
            value={draftDateTo}
            onChange={(e) => setDraftDateTo(e.target.value)}
            className="mt-1 rounded-lg border border-slate-300 px-3 py-2 text-sm"
          />
        </div>
        <button
          type="button"
          onClick={() => applyFilters()}
          className="rounded-lg bg-[#006437] px-4 py-2 text-sm font-semibold text-white hover:bg-[#005029] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          Tətbiq et
        </button>
      </div>

      {error && (
        <p className="mt-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </p>
      )}

      {loading ? (
        <div className="mt-12 flex justify-center text-slate-500">
          <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : (
        <>
          <div className="mt-6 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Nömrə</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Ödəniş</th>
                  <th className="px-4 py-3 text-right">Sizin məbləğ</th>
                  <th className="px-4 py-3">Tarix</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-4 py-10 text-center text-slate-500">
                      Sifariş tapılmadı.
                    </td>
                  </tr>
                ) : (
                  items.map((o) => (
                    <tr key={o.id} className="hover:bg-slate-50/80">
                      <td className="px-4 py-3 font-medium text-slate-900">{o.orderNumber}</td>
                      <td className="px-4 py-3">
                        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-800">
                          {o.statusLabel}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-slate-600">{o.paymentStatus}</td>
                      <td className="px-4 py-3 text-right tabular-nums font-medium">
                        {formatMoney(orderDisplayTotalForSeller(o))}
                      </td>
                      <td className="px-4 py-3 text-slate-600">
                        {new Date(o.createdAt).toLocaleString('az-AZ')}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <Link
                          to={`/orders/${o.id}`}
                          {...prefetchOnIntent(`/orders/${o.id}`)}
                          className="text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
                        >
                          Ətraflı
                        </Link>
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
