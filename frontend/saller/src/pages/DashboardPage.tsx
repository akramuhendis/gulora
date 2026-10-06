import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  ArrowRight,
  BarChart3,
  Boxes,
  Gauge,
  Landmark,
  Loader2,
  Megaphone,
  MessageSquare,
  Package,
  RefreshCw,
  ShoppingCart,
  Star,
  Store,
  User,
} from 'lucide-react'
import {
  ApiError,
  sellerPortalApi,
  type OrderSummaryResponse,
  type SellerDashboardResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'
import { formatMoney } from '../lib/formatMoney'
import { orderDisplayTotalForSeller } from '../lib/orderDisplayAmount'
import { PARTNER_ANNOUNCEMENTS } from '../data/partnerHub'

function StatCard({
  label,
  value,
  hint,
  loading,
}: {
  label: string
  value: string | number
  hint?: string
  loading: boolean
}) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white px-4 py-4 shadow-sm">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
      {loading ? (
        <div className="mt-2 flex h-8 items-center text-slate-400">
          <Loader2 className="h-5 w-5 animate-spin" aria-label="Yüklənir" />
        </div>
      ) : (
        <>
          <p className="mt-1 text-2xl font-bold tabular-nums text-slate-900">{value}</p>
          {hint ? <p className="mt-1 text-xs text-slate-500">{hint}</p> : null}
        </>
      )}
    </div>
  )
}

function SparkBars({
  days,
}: {
  days: { label: string; count: number; dateKey: string }[]
}) {
  const max = Math.max(...days.map((d) => d.count), 1)
  const last7 = days.slice(-7)
  const summary = last7.map((d) => `${d.label}: ${d.count}`).join('; ')
  return (
    <div
      className="flex h-24 items-end gap-1"
      role="img"
      aria-label={`Son 7 gün sifariş sayı. ${summary}`}
    >
      {last7.map((d) => (
        <div key={d.dateKey} className="flex flex-1 flex-col items-center justify-end gap-1">
          <div
            className="w-full max-w-[20px] rounded-sm bg-[#006437]/75"
            style={{
              height: `${Math.max(8, Math.round((d.count / max) * 100))}%`,
            }}
          />
          <span className="max-w-full truncate text-[9px] font-medium text-slate-400">{d.label}</span>
        </div>
      ))}
    </div>
  )
}

const nf = (n: number) => new Intl.NumberFormat('az-AZ').format(n)

export default function DashboardPage() {
  usePageMeta('İcmal')

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [dash, setDash] = useState<SellerDashboardResponse | null>(null)

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
      setError(e instanceof ApiError ? e.message : 'Məlumat yüklənmədi.')
      setDash(null)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  const trendDaysUi = useMemo(() => {
    if (!dash) return []
    return dash.ordersLastDays.map((d) => ({
      dateKey: d.date,
      label: new Date(d.date + 'T12:00:00Z').toLocaleDateString('az-AZ', {
        month: 'short',
        day: 'numeric',
      }),
      count: d.orderCount,
    }))
  }, [dash])

  const statusMax = useMemo(() => {
    if (!dash?.statusBreakdown.length) return 1
    return Math.max(...dash.statusBreakdown.map((s) => s.count), 1)
  }, [dash])

  const quickLinks = [
    {
      to: '/products',
      title: 'Məhsullar',
      desc: 'Kataloq və qiymət',
      icon: Package,
      accent: 'bg-green-100 text-[#006437]',
    },
    {
      to: '/inventory',
      title: 'İnventar',
      desc: 'Stok və təhlükəli SKU',
      icon: Boxes,
      accent: 'bg-sky-100 text-sky-800',
    },
    {
      to: '/orders',
      title: 'Sifarişlər',
      desc: 'Emal və göndərmə',
      icon: ShoppingCart,
      accent: 'bg-amber-100 text-amber-900',
    },
    {
      to: '/analytics',
      title: 'Analitika',
      desc: 'Trend və status',
      icon: BarChart3,
      accent: 'bg-violet-100 text-violet-900',
    },
    {
      to: '/finance',
      title: 'Maliyyə',
      desc: 'Ödənişlər və GMV',
      icon: Landmark,
      accent: 'bg-emerald-100 text-emerald-900',
    },
    {
      to: '/reviews',
      title: 'Rəylər',
      desc: 'Reytinq üzrə icmal',
      icon: Star,
      accent: 'bg-yellow-100 text-yellow-900',
    },
    {
      to: '/campaigns',
      title: 'Kampaniyalar',
      desc: 'Endirim planları',
      icon: Megaphone,
      accent: 'bg-rose-100 text-rose-900',
    },
    {
      to: '/messages',
      title: 'Mesajlar',
      desc: 'Alıcı sorğuları',
      icon: MessageSquare,
      accent: 'bg-indigo-100 text-indigo-900',
    },
    {
      to: '/shop',
      title: 'Mağaza vitrinim',
      desc: 'Logo, banner, slug',
      icon: Store,
      accent: 'bg-teal-100 text-teal-900',
    },
    {
      to: '/performance',
      title: 'Performans',
      desc: 'SLA və skor icması',
      icon: Gauge,
      accent: 'bg-orange-100 text-orange-900',
    },
    {
      to: '/account',
      title: 'Hesabım',
      desc: 'Profil və təhlükəsizlik',
      icon: User,
      accent: 'bg-slate-200 text-slate-800',
    },
  ]

  function displayMoneyForRow(o: OrderSummaryResponse): string {
    return formatMoney(orderDisplayTotalForSeller(o))
  }

  return (
    <div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 flex-1">
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">İcmal</h1>
          <p className="mt-2 max-w-3xl text-slate-600">
            Satış göstəriciləri, inventar və sifariş axını — serverdə tam kəmiyyət üzrə hesablanır.
          </p>
          <div className="mt-4 space-y-2">
            {PARTNER_ANNOUNCEMENTS.slice(0, 2).map((a) => (
              <div
                key={a.id}
                className={`rounded-lg border px-4 py-3 text-sm ${
                  a.severity === 'warning'
                    ? 'border-amber-200 bg-amber-50 text-amber-950'
                    : a.severity === 'success'
                      ? 'border-emerald-200 bg-emerald-50 text-emerald-950'
                      : 'border-slate-200 bg-white text-slate-800'
                }`}
              >
                <p className="font-semibold">{a.title}</p>
                <p className="mt-1 text-xs opacity-90">{a.body}</p>
              </div>
            ))}
            <p className="text-xs text-slate-500">
              Tam siyahı üçün yan menyuda <strong>Duyuru</strong> düyməsinə və ya{' '}
              <Link to="/help" className="font-semibold text-[#006437] hover:underline">
                Yardım mərkəzi
              </Link>{' '}
              səhifəsinə keçin.
            </p>
          </div>
        </div>
        <button
          type="button"
          onClick={() => void load()}
          disabled={loading}
          className="inline-flex shrink-0 items-center justify-center gap-2 self-start rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 shadow-sm hover:bg-slate-50 disabled:opacity-60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} aria-hidden />
          Yenilə
        </button>
      </div>

      <p className="mt-4 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-xs text-slate-600">
        GMV və reallaşmış məbləğ <strong>sizə aid məhsul sətirləri</strong> üzrədir (çox satıcılı
        səbətdə ümumi sifariş tutarından fərqli ola bilər). Qrafik günü UTC ilə qruplaşdırır.
      </p>

      {error && (
        <p className="mt-6 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {error}
        </p>
      )}

      <section className="mt-8" aria-labelledby="stats-heading">
        <h2 id="stats-heading" className="sr-only">
          Göstəricilər
        </h2>
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <StatCard
            label="Məhsul sayı"
            value={dash ? nf(dash.totalProductCount) : '—'}
            loading={loading}
          />
          <StatCard
            label="Sifariş sayı (ümumi)"
            value={dash ? nf(dash.totalOrderCount) : '—'}
            loading={loading}
          />
          <StatCard
            label="Emal gözləyən"
            value={dash ? nf(dash.pendingFulfillmentCount) : '—'}
            hint="Gözləmə / təsdiq / hazırlanır"
            loading={loading}
          />
          <StatCard
            label={`Aşağı stok (≤${dash?.lowStockThreshold ?? 10})`}
            value={dash ? nf(dash.lowStockCount) : '—'}
            hint={dash ? `Stoksuz: ${nf(dash.outOfStockCount)}` : undefined}
            loading={loading}
          />
        </div>
      </section>

      <section className="mt-6 grid gap-4 lg:grid-cols-3" aria-labelledby="revenue-heading">
        <h2 id="revenue-heading" className="sr-only">
          Satış məbləği
        </h2>
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm lg:col-span-2">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
            GMV — satıcıya aid sətirlər
          </p>
          <p className="mt-2 text-3xl font-bold tabular-nums text-slate-900">
            {loading ? '…' : dash ? formatMoney(dash.gmvSellerAttributed) : '—'}
          </p>
          <p className="mt-2 text-xs text-slate-500">
            Ləğv, iadə və iadə tələbi statusları çıxarılıb. Ətraflı:{' '}
            <Link to="/finance" className="font-semibold text-[#006437] hover:underline">
              Maliyyə
            </Link>{' '}
            və{' '}
            <Link to="/analytics" className="font-semibold text-[#006437] hover:underline">
              Analitika
            </Link>
            .
          </p>
          <div className="mt-6 border-t border-slate-100 pt-5">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Son 7 gün — sifariş sayı (UTC)
            </p>
            {loading ? (
              <div className="mt-4 flex h-24 items-center justify-center text-slate-400">
                <Loader2 className="h-6 w-6 animate-spin" aria-label="Yüklənir" />
              </div>
            ) : (
              <SparkBars days={trendDaysUi} />
            )}
          </div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-linear-to-b from-white to-emerald-50/40 p-5 shadow-sm">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
            Kargoda / çatdırılıb (sizin sətirlər)
          </p>
          <p className="mt-2 text-3xl font-bold tabular-nums text-[#006437]">
            {loading ? '…' : dash ? formatMoney(dash.realizedSellerAttributed) : '—'}
          </p>
          <p className="mt-3 text-sm text-slate-600">
            Komissiya və köçürmə qaydaları əməliyyat müqaviləsi ilə təsdiqlənəndən sonra burada
            əks olunacaq.
          </p>
        </div>
      </section>

      {!loading && dash && dash.statusBreakdown.length > 0 && (
        <section className="mt-8 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">
            Status üzrə paylanma (bütün sifarişlər)
          </h2>
          <ul className="mt-4 grid gap-3 sm:grid-cols-2">
            {dash.statusBreakdown.map((r) => (
              <li key={r.status}>
                <div className="flex justify-between text-xs font-medium text-slate-600">
                  <span>
                    {r.statusLabel} <span className="text-slate-400">({r.status})</span>
                  </span>
                  <span className="tabular-nums text-slate-900">{r.count}</span>
                </div>
                <div className="mt-1 h-2 overflow-hidden rounded-full bg-slate-100">
                  <div
                    className="h-full rounded-full bg-[#006437]"
                    style={{ width: `${Math.round((r.count / statusMax) * 100)}%` }}
                  />
                </div>
              </li>
            ))}
          </ul>
        </section>
      )}

      <section className="mt-10">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">
          Panellər və keçidlər
        </h2>
        <div className="mt-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {quickLinks.map((q) => (
            <Link
              key={q.to}
              to={q.to}
              className="group flex items-center justify-between rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:border-[#006437] hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
            >
              <div className="flex items-center gap-4">
                <span
                  className={`flex h-12 w-12 items-center justify-center rounded-xl ${q.accent}`}
                >
                  <q.icon className="h-6 w-6" aria-hidden />
                </span>
                <div>
                  <p className="font-semibold text-slate-900">{q.title}</p>
                  <p className="text-sm text-slate-500">{q.desc}</p>
                </div>
              </div>
              <ArrowRight
                className="h-5 w-5 shrink-0 text-slate-400 transition motion-safe:group-hover:translate-x-0.5 motion-safe:group-hover:text-[#006437]"
                aria-hidden
              />
            </Link>
          ))}
        </div>
      </section>

      <section className="mt-10" aria-labelledby="recent-orders-heading">
        <div className="flex flex-wrap items-end justify-between gap-2">
          <h2 id="recent-orders-heading" className="text-lg font-semibold text-slate-900">
            Son sifarişlər
          </h2>
          <Link
            to="/orders"
            className="rounded text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
          >
            Hamısına bax
          </Link>
        </div>

        {loading ? (
          <div className="mt-6 flex justify-center py-12 text-slate-500">
            <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
          </div>
        ) : !dash || dash.recentOrders.length === 0 ? (
          <p className="mt-6 rounded-xl border border-dashed border-slate-200 bg-slate-50/80 px-4 py-10 text-center text-sm text-slate-600">
            Hələ sifariş yoxdur. Məhsullarınız satılanda burada görünəcək.
          </p>
        ) : (
          <div className="mt-4 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-4 py-3">Nömrə</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="hidden px-4 py-3 text-right sm:table-cell">Sizin məbləğ</th>
                  <th className="hidden px-4 py-3 md:table-cell">Tarix</th>
                  <th className="w-[1%] px-4 py-3 text-right" />
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {dash.recentOrders.map((o) => (
                  <tr key={o.id} className="hover:bg-slate-50/80">
                    <td className="px-4 py-3 font-medium text-slate-900">{o.orderNumber}</td>
                    <td className="px-4 py-3">
                      <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-800">
                        {o.statusLabel}
                      </span>
                    </td>
                    <td className="hidden px-4 py-3 text-right font-medium tabular-nums sm:table-cell">
                      {displayMoneyForRow(o)}
                    </td>
                    <td className="hidden px-4 py-3 text-slate-600 md:table-cell">
                      {new Date(o.createdAt).toLocaleString('az-AZ', {
                        dateStyle: 'short',
                        timeStyle: 'short',
                      })}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <Link
                        to={`/orders/${o.id}`}
                        className="rounded text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437]"
                      >
                        Ətraflı
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
