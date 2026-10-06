import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Gauge, Loader2, PackageCheck, Timer, Truck } from 'lucide-react'
import { ApiError, sellerPortalApi, type SellerDashboardResponse } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

function pct(n: number): string {
  return `${Math.round(n * 100)}%`
}

export default function PerformancePage() {
  usePageMeta('Performans')

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
      setError(e instanceof ApiError ? e.message : 'Performans yüklənmədi.')
      setDash(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  const shipScore = useMemo(() => {
    if (!dash?.statusBreakdown?.length) return null
    const total = dash.statusBreakdown.reduce((a, r) => a + r.count, 0)
    if (!total) return null
    const shipped =
      dash.statusBreakdown.find((r) => r.status === 'Shipped')?.count ?? 0
    const delivered =
      dash.statusBreakdown.find((r) => r.status === 'Delivered')?.count ?? 0
    return (shipped + delivered) / total
  }, [dash])

  const cancelRate = useMemo(() => {
    if (!dash?.statusBreakdown?.length) return null
    const total = dash.statusBreakdown.reduce((a, r) => a + r.count, 0)
    if (!total) return null
    const cancelled = dash.statusBreakdown.find((r) => r.status === 'Cancelled')?.count ?? 0
    return cancelled / total
  }, [dash])

  const compositeScore = useMemo(() => {
    if (shipScore == null || cancelRate == null) return null
    return Math.min(10, Math.max(1, Math.round(5 + 5 * shipScore - 5 * cancelRate)))
  }, [shipScore, cancelRate])

  return (
    <div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
            Performans icması
          </h1>
          <p className="mt-2 max-w-3xl text-slate-600">
            Trendyol Partner tipli «satıcı skoru» üçün əsas göstəricilər: çatdırılma payı, ləğv nisbəti,
            emal gözləməsi. Real SLA və çatdırılma müddəti ölçüləri backend hesabat API-si ilə
            sinxron olacaq.
          </p>
        </div>
        <Link
          to="/analytics"
          className="inline-flex shrink-0 items-center justify-center rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          Analitikaya keç
        </Link>
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
          <section className="mt-10 grid gap-4 lg:grid-cols-4" aria-labelledby="perf-kpi">
            <h2 id="perf-kpi" className="sr-only">
              KPI kartları
            </h2>
            <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div className="flex items-center gap-2 text-slate-500">
                <Truck className="h-5 w-5 text-[#006437]" aria-hidden />
                <p className="text-xs font-semibold uppercase tracking-wide">Kargo payı (təxmini)</p>
              </div>
              <p className="mt-3 text-3xl font-bold tabular-nums text-slate-900">
                {shipScore != null ? pct(shipScore) : '—'}
              </p>
              <p className="mt-2 text-xs text-slate-500">
                (Shipped + Delivered) / bütün sifarişlər — ümumi göstərici.
              </p>
            </div>
            <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div className="flex items-center gap-2 text-slate-500">
                <Timer className="h-5 w-5 text-amber-700" aria-hidden />
                <p className="text-xs font-semibold uppercase tracking-wide">Emal gözləyən</p>
              </div>
              <p className="mt-3 text-3xl font-bold tabular-nums text-slate-900">
                {dash ? new Intl.NumberFormat('az-AZ').format(dash.pendingFulfillmentCount) : '—'}
              </p>
              <p className="mt-2 text-xs text-slate-500">Gözləmə / təsdiq / hazırlanır.</p>
            </div>
            <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div className="flex items-center gap-2 text-slate-500">
                <PackageCheck className="h-5 w-5 text-emerald-700" aria-hidden />
                <p className="text-xs font-semibold uppercase tracking-wide">Ləğv nisbəti</p>
              </div>
              <p className="mt-3 text-3xl font-bold tabular-nums text-slate-900">
                {cancelRate != null ? pct(cancelRate) : '—'}
              </p>
              <p className="mt-2 text-xs text-slate-500">Cancelled / ümumi (əsas göstərici).</p>
            </div>
            <div className="rounded-xl border border-slate-200 bg-linear-to-b from-white to-emerald-50/40 p-5 shadow-sm">
              <div className="flex items-center gap-2 text-slate-500">
                <Gauge className="h-5 w-5 text-emerald-800" aria-hidden />
                <p className="text-xs font-semibold uppercase tracking-wide">Ümumi reytinq (UI)</p>
              </div>
              <p className="mt-3 text-4xl font-black tabular-nums text-[#006437]">
                {compositeScore ?? '—'}
              </p>
              <p className="mt-2 text-xs text-slate-600">
                Sadələşdirilmiş nümunə bal — əvəzinə backend skoru istifadə olunacaq.
              </p>
            </div>
          </section>

          <section className="mt-10 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6">
            <h2 className="text-base font-semibold text-slate-900">Tezliklə (Trendyol tipli)</h2>
            <ul className="mt-3 list-inside list-disc space-y-1 text-sm text-slate-700">
              <li>Sifarişə görə orta hazırlıq müddəti (saat)</li>
              <li>Kargo təslimdə gecikmə sayı və səbəbi kodları</li>
              <li>Müştəri şikayəti / iadə səbəbi dağılımı</li>
              <li>Hədəf SLA və yaşıl/sarı/qırmızı zona</li>
            </ul>
          </section>
        </>
      )}
    </div>
  )
}
