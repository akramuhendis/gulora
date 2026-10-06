import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ArrowRight, Landmark, Loader2, PiggyBank } from 'lucide-react'
import {
  ApiError,
  sellerPortalApi,
  type OrderSummaryResponse,
  type SellerDashboardResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'
import { formatMoney } from '../lib/formatMoney'
import { orderDisplayTotalForSeller } from '../lib/orderDisplayAmount'

function formatShortDate(iso: string): string {
  return new Date(iso).toLocaleString('az-AZ', { dateStyle: 'short', timeStyle: 'short' })
}

function sumPendingSellerAttributed(orders: OrderSummaryResponse[]): number {
  let s = 0
  for (const o of orders) {
    const paid =
      o.paymentStatus?.toLowerCase() === 'completed' || o.paymentStatus?.toLowerCase() === 'paid'
    if (paid) continue
    if (o.status === 'Cancelled' || o.status === 'Returned' || o.status === 'ReturnRequested') {
      continue
    }
    s += orderDisplayTotalForSeller(o)
  }
  return s
}

export default function FinancePage() {
  usePageMeta('Maliyyə')

  const [dash, setDash] = useState<SellerDashboardResponse | null>(null)
  const [orders, setOrders] = useState<OrderSummaryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [partialNotice, setPartialNotice] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    setPartialNotice(null)

    const outcomes = await Promise.allSettled([
      sellerPortalApi.getDashboard({ trendDays: 14, recentOrders: 8 }),
      sellerPortalApi.listOrders({ page: 1, pageSize: 80 }),
    ])

    let dashOk: SellerDashboardResponse | null = null
    let ordersOk: OrderSummaryResponse[] = []
    const problems: string[] = []

    const [d0, d1] = outcomes
    if (d0.status === 'fulfilled') dashOk = d0.value
    else problems.push(d0.reason instanceof ApiError ? d0.reason.message : 'İcmal yüklənmədi.')

    if (d1.status === 'fulfilled') ordersOk = d1.value.items
    else problems.push(d1.reason instanceof ApiError ? d1.reason.message : 'Sifariş siyahısı gəlmədi.')

    setDash(dashOk)
    setOrders(ordersOk)

    if (problems.length === 2) {
      setError(problems.join(' '))
    } else if (problems.length === 1) {
      setPartialNotice(problems[0]!)
      setError(null)
    }

    setLoading(false)
  }, [])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  const pendingPayment = useMemo(() => sumPendingSellerAttributed(orders), [orders])

  const gmvDisplay = dash?.gmvSellerAttributed
  const realizedDisplay = dash?.realizedSellerAttributed

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
        Ödənişlər və hesabat
      </h1>
      <p className="mt-2 max-w-3xl text-slate-600">
        Üst göstəricilər server aqreqatındandır; cədvəl son 80 sifariş nümunəsidir. Məbləğlər, mümkün
        olduqda, sizə aid məhsul sətirləri ilə göstərilir.
      </p>

      {partialNotice && (
        <p className="mt-4 rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-950">
          {partialNotice}
        </p>
      )}

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
          <section className="mt-10 grid gap-4 lg:grid-cols-3" aria-labelledby="finance-kpi">
            <h2 id="finance-kpi" className="sr-only">
              Maliyyə göstəriciləri
            </h2>
            <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div className="flex items-center gap-2 text-slate-500">
                <PiggyBank className="h-5 w-5 text-[#006437]" aria-hidden />
                <p className="text-xs font-semibold uppercase tracking-wide">GMV (satıcı sətirləri)</p>
              </div>
              <p className="mt-2 text-2xl font-bold tabular-nums text-slate-900">
                {gmvDisplay != null ? formatMoney(gmvDisplay) : '—'}
              </p>
              <p className="mt-2 text-xs text-slate-500">
                Ləğv, iadə və iadə tələbi çıxarılıb — tam küy.
              </p>
            </div>
            <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <div className="flex items-center gap-2 text-slate-500">
                <Landmark className="h-5 w-5 text-emerald-700" aria-hidden />
                <p className="text-xs font-semibold uppercase tracking-wide">Kargoda / çatdırılıb</p>
              </div>
              <p className="mt-2 text-2xl font-bold tabular-nums text-[#006437]">
                {realizedDisplay != null ? formatMoney(realizedDisplay) : '—'}
              </p>
              <p className="mt-2 text-xs text-slate-500">Komissiya sonrakı mərhələdə.</p>
            </div>
            <div className="rounded-xl border border-amber-100 bg-amber-50/80 p-5 shadow-sm">
              <p className="text-xs font-semibold uppercase tracking-wide text-amber-900">
                Gözləyən ödəniş (nümunə 80 sifariş)
              </p>
              <p className="mt-2 text-2xl font-bold tabular-nums text-amber-950">
                {formatMoney(pendingPayment)}
              </p>
              <p className="mt-2 text-xs text-amber-900/80">
                Ödəniş statusu «Completed/Paid» deyil — təxmini, sətir cəmi ilə.
              </p>
            </div>
          </section>

          <section className="mt-10 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="text-base font-semibold text-slate-900">Ödəniş təqvimi (nümunə)</h2>
            <p className="mt-2 text-sm text-slate-600">
              Trendyol Partner tipli həftəlik/hər iki həftəlik köçürmə pəncərələri üçün təqvim görünüşü.
              Real tarixlər əməliyyat müqaviləsi və backend ödəniş cədvəli ilə sinxron olacaq.
            </p>
            <ul className="mt-4 grid gap-3 sm:grid-cols-3">
              {[
                {
                  label: 'Növbəti ödəniş pəncərəsi',
                  value: '12–14 May 2026',
                  tone: 'bg-emerald-50 border-emerald-200',
                },
                { label: 'Son köçürmə', value: '28 Apr 2026', tone: 'bg-white border-slate-200' },
                {
                  label: 'Gözləyən əməliyyatlar',
                  value: 'Əməliyyat siyahısı ilə',
                  tone: 'bg-amber-50 border-amber-200',
                },
              ].map((row) => (
                <li
                  key={row.label}
                  className={`rounded-xl border px-4 py-4 text-sm shadow-sm ${row.tone}`}
                >
                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                    {row.label}
                  </p>
                  <p className="mt-2 font-semibold text-slate-900">{row.value}</p>
                </li>
              ))}
            </ul>
          </section>

          <section className="mt-10 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6">
            <h2 className="text-base font-semibold text-slate-900">Tezliklə</h2>
            <ul className="mt-3 list-inside list-disc space-y-1 text-sm text-slate-600">
              <li>Həftəlik/Hər iki həftəlik ödəniş qəbzləri (PDF)</li>
              <li>Komissiya və əDV xülasəsi</li>
              <li>Bank köçürmə tarixçəsi və reconciliasiya</li>
            </ul>
          </section>

          <section className="mt-10" aria-labelledby="recent-financial-orders">
            <div className="flex flex-wrap items-end justify-between gap-2">
              <h2 id="recent-financial-orders" className="text-lg font-semibold text-slate-900">
                Son əməliyyatlar (nümunə)
              </h2>
              <Link
                to="/orders"
                className="inline-flex items-center gap-1 text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
              >
                Bütün sifarişlər
                <ArrowRight className="h-4 w-4" aria-hidden />
              </Link>
            </div>

            <div className="mt-4 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-4 py-3">Nömrə</th>
                    <th className="px-4 py-3">Ödəniş</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3 text-right tabular-nums">Sizin məbləğ</th>
                    <th className="hidden px-4 py-3 md:table-cell">Tarix</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {orders.map((o) => (
                    <tr key={o.id} className="hover:bg-slate-50/80">
                      <td className="px-4 py-3 font-medium text-slate-900">
                        <Link
                          to={`/orders/${o.id}`}
                          className="text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
                        >
                          {o.orderNumber}
                        </Link>
                      </td>
                      <td className="px-4 py-3 text-slate-700">{o.paymentStatus}</td>
                      <td className="px-4 py-3">
                        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-800">
                          {o.statusLabel}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right font-semibold tabular-nums text-slate-900">
                        {formatMoney(orderDisplayTotalForSeller(o))}
                      </td>
                      <td className="hidden px-4 py-3 text-slate-600 md:table-cell">
                        {formatShortDate(o.createdAt)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {orders.length === 0 && !error && (
                <p className="px-4 py-12 text-center text-sm text-slate-600">Əməliyyat tapılmadı.</p>
              )}
            </div>
          </section>
        </>
      )}
    </div>
  )
}
