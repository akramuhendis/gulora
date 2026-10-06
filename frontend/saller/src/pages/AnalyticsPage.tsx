import { useCallback, useEffect, useMemo, useState } from 'react'
import { Loader2, RefreshCw } from 'lucide-react'
import { ApiError, sellerPortalApi, type SellerDashboardResponse } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'
import { formatMoney } from '../lib/formatMoney'

const nf = new Intl.NumberFormat('az-AZ')

function formatCount(value: number): string {
  return nf.format(value)
}

function formatTrendDate(dateKey: string): string {
  const date = new Date(dateKey + 'T12:00:00Z')
  return `${String(date.getUTCDate()).padStart(2, '0')}.${String(date.getUTCMonth() + 1).padStart(2, '0')}`
}

function KpiCard({
  label,
  value,
  hint,
  tone = 'text-slate-900',
}: {
  label: string
  value: string
  hint: string
  tone?: string
}) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">{label}</p>
      <p className={`mt-2 text-2xl font-bold tabular-nums ${tone}`}>{value}</p>
      <p className="mt-2 text-xs text-slate-500">{hint}</p>
    </div>
  )
}

function BarChart({
  data,
  max,
  barClass,
  ariaLabel,
  emptyLabel,
  valueFormatter,
}: {
  data: { label: string; value: number }[]
  max: number
  barClass: string
  ariaLabel: string
  emptyLabel: string
  valueFormatter?: (value: number) => string
}) {
  const m = Math.max(max, 1)
  const formatValue = valueFormatter ?? formatCount
  const summary = data.map((d) => `${d.label}: ${formatValue(d.value)}`).join('; ')
  const hasPositiveValue = data.some((d) => d.value > 0)

  if (data.length === 0 || !hasPositiveValue) {
    return (
      <div className="flex h-48 items-center justify-center rounded-lg border border-dashed border-slate-200 bg-slate-50 px-4 text-center">
        <p className="text-sm text-slate-500">{emptyLabel}</p>
      </div>
    )
  }

  return (
    <div
      className="flex h-48 items-end gap-1 sm:gap-2"
      role="img"
      aria-label={`${ariaLabel}. ${summary}`}
    >
      {data.map((d, index) => {
        const h = Math.round((d.value / m) * 100)
        const showLabel = data.length <= 8 || index % 2 === 0 || index === data.length - 1
        return (
          <div key={d.label} className="flex flex-1 flex-col items-center gap-1">
            <div className="flex w-full flex-1 items-end justify-center">
              <div
                className={`w-full max-w-[28px] rounded-t-md ${barClass}`}
                style={{ height: `${Math.max(h, d.value > 0 ? 8 : 3)}%` }}
                title={`${d.label}: ${formatValue(d.value)}`}
              />
            </div>
            <span className="max-w-full truncate text-[10px] font-medium text-slate-500 sm:text-xs">
              {showLabel ? d.label : ''}
            </span>
          </div>
        )
      })}
    </div>
  )
}

function StatusBars({
  rows,
}: {
  rows: { status: string; statusLabel: string; count: number }[]
}) {
  const max = rows.reduce((a, r) => Math.max(a, r.count), 0) || 1
  const total = rows.reduce((a, r) => a + r.count, 0)
  return (
    <ul className="space-y-3" aria-label="Status üzrə paylanma">
      {rows.map((r) => (
        <li key={r.status}>
          <div className="flex justify-between text-xs font-medium text-slate-600">
            <span>
              {r.statusLabel} <span className="text-slate-400">({r.status})</span>
            </span>
            <span className="tabular-nums text-slate-900">
              {r.count}
              {total > 0 ? (
                <span className="ml-1 text-slate-400">({Math.round((r.count / total) * 100)}%)</span>
              ) : null}
            </span>
          </div>
          <div className="mt-1 h-2 overflow-hidden rounded-full bg-slate-100">
            <div
              className="h-full rounded-full bg-[#006437] motion-safe:transition-[width] motion-safe:duration-300"
              style={{ width: `${Math.round((r.count / max) * 100)}%` }}
            />
          </div>
        </li>
      ))}
      {rows.length === 0 && <li className="text-sm text-slate-500">Sifariş yoxdur.</li>}
    </ul>
  )
}

export default function AnalyticsPage() {
  usePageMeta('Analitika')

  const [dash, setDash] = useState<SellerDashboardResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await sellerPortalApi.getDashboard({
        recentOrders: 8,
        trendDays: 14,
        lowStockThreshold: 10,
      })
      setDash(data)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Analitika yüklənmədi.')
      setDash(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  const chartOrders = useMemo(() => {
    if (!dash) return []
    return dash.ordersLastDays.map((d) => ({
      label: formatTrendDate(d.date),
      value: d.orderCount,
    }))
  }, [dash])

  const chartRev = useMemo(() => {
    if (!dash) return []
    return dash.ordersLastDays.map((d) => ({
      label: formatTrendDate(d.date),
      value: d.sellerAttributedTotal ?? 0,
    }))
  }, [dash])

  const maxCount = useMemo(
    () => chartOrders.reduce((a, d) => Math.max(a, d.value), 0),
    [chartOrders]
  )
  const maxRev = useMemo(() => chartRev.reduce((a, d) => Math.max(a, d.value), 0), [chartRev])
  const averageOrderGmv =
    dash && dash.totalOrderCount > 0 ? dash.gmvSellerAttributed / dash.totalOrderCount : null

  return (
    <div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 flex-1">
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">Analitika</h1>
          <p className="mt-2 max-w-3xl text-slate-600">
            Sifariş, GMV və stok göstəriciləri serverdə tam satıcı küyü üzrə hesablanır; trendlər UTC
            günlərinə görə qruplaşdırılır.
          </p>
        </div>
        <button
          type="button"
          onClick={() => void load()}
          disabled={loading}
          className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] disabled:cursor-not-allowed disabled:opacity-60"
        >
          <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} aria-hidden />
          Yenilə
        </button>
      </div>

      {error && (
        <p className="mt-6 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </p>
      )}

      {loading ? (
        <div className="mt-16 flex justify-center text-slate-500">
          <Loader2 className="h-10 w-10 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : (
        <>
          {!dash ? (
            <section className="mt-10 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-8 text-center">
              <h2 className="text-base font-semibold text-slate-900">Analitika məlumatı göstərilmir</h2>
              <p className="mt-2 text-sm text-slate-600">
                API cavabı gəlmədiyi üçün qrafiklər boş saxlanıldı. Yenidən cəhd edə bilərsiniz.
              </p>
              <button
                type="button"
                onClick={() => void load()}
                className="mt-4 inline-flex items-center justify-center gap-2 rounded-lg bg-[#006437] px-4 py-2 text-sm font-semibold text-white hover:bg-[#00532e] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
              >
                <RefreshCw className="h-4 w-4" aria-hidden />
                Təkrar yoxla
              </button>
            </section>
          ) : null}

          {dash ? (
            <section
              className="mt-10 grid gap-4 sm:grid-cols-2 xl:grid-cols-3"
              aria-labelledby="kpi-analytics"
            >
              <h2 id="kpi-analytics" className="sr-only">
                Əsas göstəricilər
              </h2>
              <KpiCard
                label="Sifariş sayı"
                value={nf.format(dash.totalOrderCount)}
                hint={`${dash.pendingFulfillmentCount} sifariş icra gözləyir`}
              />
              <KpiCard
                label="GMV (satıcı sətirləri)"
                value={formatMoney(dash.gmvSellerAttributed)}
                hint="Ləğv və iadələr çıxarılıb"
              />
              <KpiCard
                label="Kargoda / çatdırılıb"
                value={formatMoney(dash.realizedSellerAttributed)}
                hint="Reallaşmış satıcı sətirləri"
                tone="text-[#006437]"
              />
              <KpiCard
                label="Orta sifariş GMV"
                value={averageOrderGmv == null ? '—' : formatMoney(averageOrderGmv)}
                hint="GMV / sifariş sayı"
              />
              <KpiCard
                label="Məhsul sayı"
                value={nf.format(dash.totalProductCount)}
                hint={`${dash.lowStockCount} az stok, ${dash.outOfStockCount} stokda yoxdur`}
              />
              <KpiCard
                label="Stok limiti"
                value={`${dash.lowStockThreshold} ədəd`}
                hint="Az stok hesablamasında istifadə olunur"
              />
            </section>
          ) : null}

          {dash ? (
            <>
              <div className="mt-10 grid gap-8 lg:grid-cols-2">
                <section
                  className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
                  aria-labelledby="chart-orders"
                >
                  <h2 id="chart-orders" className="text-lg font-semibold text-slate-900">
                    Son günlər — sifariş sayı
                  </h2>
                  <div className="mt-6">
                    <BarChart
                      data={chartOrders}
                      max={maxCount}
                      barClass="bg-[#006437]/85"
                      ariaLabel="Günlük sifariş sayı"
                      emptyLabel="Seçilən dövrdə sifariş yoxdur."
                    />
                  </div>
                </section>
                <section
                  className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
                  aria-labelledby="chart-rev"
                >
                  <h2 id="chart-rev" className="text-lg font-semibold text-slate-900">
                    Son günlər — GMV
                  </h2>
                  <p className="mt-1 text-sm text-slate-500">
                    Satıcıya aid məhsul sətirlərinin günlük toplamı.
                  </p>
                  <div className="mt-6">
                    <BarChart
                      data={chartRev}
                      max={maxRev}
                      barClass="bg-emerald-400/90"
                      ariaLabel="Günlük GMV"
                      emptyLabel="Seçilən dövrdə GMV yoxdur."
                      valueFormatter={formatMoney}
                    />
                  </div>
                </section>
              </div>

              <section
                className="mt-10 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
                aria-labelledby="status-split"
              >
                <h2 id="status-split" className="text-lg font-semibold text-slate-900">
                  Status üzrə paylanma
                </h2>
                <div className="mt-6 max-w-xl">
                  <StatusBars rows={dash.statusBreakdown} />
                </div>
              </section>
            </>
          ) : null}
        </>
      )}
    </div>
  )
}
