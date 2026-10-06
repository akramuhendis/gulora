import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Loader2, Star } from 'lucide-react'
import {
  ApiError,
  sellerPortalApi,
  type ProductReviewResponse,
  type ProductSummaryResponse,
} from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

function Stars({ value }: { value: number }) {
  const full = Math.round(value)
  return (
    <span className="flex items-center gap-0.5 text-amber-500" aria-label={`${value.toFixed(1)} ulduz`}>
      {Array.from({ length: 5 }, (_, i) => (
        <Star
          key={i}
          className={`h-4 w-4 ${i < full ? 'fill-current' : 'fill-none opacity-30'}`}
          aria-hidden
        />
      ))}
    </span>
  )
}

type ReviewSummary = { totalCount: number; items: ProductReviewResponse[] }

function rowCount(p: ProductSummaryResponse, s: ReviewSummary | undefined) {
  return Math.max(s?.totalCount ?? 0, p.reviewCount)
}

function rowAvg(p: ProductSummaryResponse, s: ReviewSummary | undefined) {
  const cnt = rowCount(p, s)
  if (cnt <= 0) return p.averageRating
  if (p.averageRating > 0) return p.averageRating
  if (s && s.items.length === s.totalCount && s.totalCount > 0) {
    return s.items.reduce((a, r) => a + r.rating, 0) / s.totalCount
  }
  if (s && s.items.length > 0) return s.items.reduce((a, r) => a + r.rating, 0) / s.items.length
  return p.averageRating
}

function reviewBodyText(r: ProductReviewResponse) {
  const t = (r.title ?? '').trim()
  const b = (r.body ?? '').trim()
  if (!t) return b
  if (!b) return t
  if (t === b) return t
  if (b.startsWith(t)) return b
  return `${t} — ${b}`
}

function statusBadge(status: string) {
  const s = String(status).toLowerCase()
  if (s === 'pending')
    return { label: 'Gözləmədə', cls: 'bg-amber-100 text-amber-900 ring-1 ring-amber-200/60' }
  if (s === 'approved')
    return { label: 'Təsdiqli', cls: 'bg-emerald-100 text-emerald-900 ring-1 ring-emerald-200/60' }
  if (s === 'rejected')
    return { label: 'Rədd', cls: 'bg-red-100 text-red-800 ring-1 ring-red-200/60' }
  return { label: status, cls: 'bg-slate-100 text-slate-700' }
}

export default function ReviewsRatingsPage() {
  usePageMeta('Rəylər')

  const [items, setItems] = useState<ProductSummaryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reviewByProductId, setReviewByProductId] = useState<Record<string, ReviewSummary>>({})
  const [reviewsLoading, setReviewsLoading] = useState(false)

  const loadReviewSummaries = useCallback(async (list: ProductSummaryResponse[]) => {
    if (list.length === 0) {
      setReviewByProductId({})
      return
    }
    setReviewsLoading(true)
    const entries = await Promise.all(
      list.map(async (p) => {
        try {
          const res = await sellerPortalApi.listMyProductReviews(p.id, 1, 50)
          return [p.id, { totalCount: res.totalCount, items: res.items }] as const
        } catch {
          return [p.id, { totalCount: 0, items: [] as ProductReviewResponse[] }] as const
        }
      })
    )
    setReviewByProductId(Object.fromEntries(entries))
    setReviewsLoading(false)
  }, [])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await sellerPortalApi.listProducts({ page: 1, pageSize: 100, sortBy: 'newest' })
      setItems(res.items)
      void loadReviewSummaries(res.items)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Məhsullar yüklənmədi.')
      setItems([])
      setReviewByProductId({})
    } finally {
      setLoading(false)
    }
  }, [loadReviewSummaries])

  useEffect(() => {
    queueMicrotask(() => void load())
  }, [load])

  const withReviews = useMemo(() => {
    return [...items]
      .filter((p) => rowCount(p, reviewByProductId[p.id]) > 0)
      .sort((a, b) => rowAvg(b, reviewByProductId[b.id]) - rowAvg(a, reviewByProductId[a.id]))
  }, [items, reviewByProductId])

  const weightedAvg = useMemo(() => {
    let sum = 0
    let n = 0
    for (const p of items) {
      const s = reviewByProductId[p.id]
      const cnt = rowCount(p, s)
      if (cnt <= 0) continue
      sum += rowAvg(p, s) * cnt
      n += cnt
    }
    return n > 0 ? sum / n : null
  }, [items, reviewByProductId])

  const allReviewRows = useMemo(() => {
    const rows: { product: ProductSummaryResponse; review: ProductReviewResponse }[] = []
    for (const p of items) {
      const s = reviewByProductId[p.id]
      if (!s?.items.length) continue
      for (const r of s.items) {
        rows.push({ product: p, review: r })
      }
    }
    rows.sort((a, b) => (a.review.createdAt < b.review.createdAt ? 1 : -1))
    return rows
  }, [items, reviewByProductId])

  return (
    <div>
      <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
        Rəylər və reytinq
      </h1>
      <p className="mt-2 max-w-3xl text-slate-600">
        Aşağıda öz məhsullarınız üçün API-dən oxunan rəy mətnləri göstərilir (təsdiqli, gözləmədə və rədd
        statusları). Üst cədvəldə orta və say kataloq + ilk səhifə məlumatı ilə uyğunlaşdırılır.
      </p>

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
          <section className="mt-10 grid gap-4 sm:grid-cols-2" aria-labelledby="review-summary">
            <h2 id="review-summary" className="sr-only">
              Xülasə
            </h2>
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Çəkilmiş orta (təsdiqli rəylər)
              </p>
              {reviewsLoading ? (
                <div className="mt-4 flex items-center gap-2 text-slate-500 text-sm">
                  <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
                  Rəylər hesablanır…
                </div>
              ) : weightedAvg === null ? (
                <p className="mt-3 text-slate-600">Hələ rəy qeydə alınmayıb.</p>
              ) : (
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <p className="text-4xl font-bold tabular-nums text-slate-900">
                    {weightedAvg.toFixed(2)}
                  </p>
                  <Stars value={weightedAvg} />
                </div>
              )}
            </div>
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Rəyli məhsul sayı
              </p>
              {reviewsLoading ? (
                <div className="mt-4 flex items-center gap-2 text-slate-500 text-sm">
                  <Loader2 className="h-5 w-5 animate-spin" aria-hidden />
                </div>
              ) : (
                <>
                  <p className="mt-3 text-4xl font-bold tabular-nums text-[#006437]">
                    {withReviews.length}
                  </p>
                  <p className="mt-2 text-sm text-slate-600">
                    İzləmə siyahısındakı {items.length} məhsuldan (ən azı bir rəy qeydi olan).
                  </p>
                </>
              )}
            </div>
          </section>

          <section className="mt-10" aria-labelledby="review-products">
            <h2 id="review-products" className="text-lg font-semibold text-slate-900">
              Məhsullar üzrə reytinq
            </h2>
            <div className="mt-4 overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-4 py-3">Məhsul</th>
                    <th className="px-4 py-3 text-right tabular-nums">Orta</th>
                    <th className="px-4 py-3 text-right tabular-nums">Rəy sayı</th>
                    <th className="px-4 py-3 text-right w-[1%]" />
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {items.map((p) => {
                    const s = reviewByProductId[p.id]
                    const rc = rowCount(p, s)
                    const ra = rowAvg(p, s)
                    return (
                      <tr key={p.id} className="hover:bg-slate-50/80">
                        <td className="px-4 py-3 font-medium text-slate-900">{p.name}</td>
                        <td className="px-4 py-3 text-right">
                          <span className="inline-flex items-center justify-end gap-2 tabular-nums">
                            {reviewsLoading && !s ? (
                              <span className="text-slate-400">…</span>
                            ) : (
                              <>
                                {ra.toFixed(1)}
                                <Stars value={ra} />
                              </>
                            )}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-right tabular-nums text-slate-700">
                          {reviewsLoading && !s ? '…' : rc}
                          {s && s.totalCount > s.items.length ? (
                            <span className="block text-[10px] font-normal normal-case text-slate-400">
                              (ilk {s.items.length} sətirdə)
                            </span>
                          ) : null}
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
                    )
                  })}
                </tbody>
              </table>
              {items.length === 0 && !error && (
                <p className="px-4 py-12 text-center text-sm text-slate-600">Məhsul yoxdur.</p>
              )}
            </div>
          </section>

          <section className="mt-10" aria-labelledby="review-texts">
            <h2 id="review-texts" className="text-lg font-semibold text-slate-900">
              Rəy mətnləri
            </h2>
            <p className="mt-1 text-sm text-slate-600">
              Satıcı JWT ilə yalnız öz məhsullarınız üçün bütün statuslarda rəylər göstərilir (səhifə başına
              ən çox 50).
            </p>
            <div className="mt-4 space-y-3">
              {reviewsLoading && (
                <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-6 text-slate-500 text-sm">
                  <Loader2 className="h-5 w-5 animate-spin shrink-0" aria-hidden />
                  Rəy mətnləri yüklənir…
                </div>
              )}
              {!reviewsLoading && allReviewRows.length === 0 && (
                <p className="rounded-xl border border-slate-200 bg-white px-4 py-8 text-center text-sm text-slate-600">
                  Bu məhsullar üçün hələ rəy yoxdur. Mağaza vitrinində rəy yazıldıqdan sonra burada görünəcək
                  (gözləmədə olsa belə).
                </p>
              )}
              {!reviewsLoading &&
                allReviewRows.map(({ product, review: r }) => {
                  const sb = statusBadge(r.status)
                  return (
                  <article
                    key={r.id}
                    className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
                  >
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                          {product.name}
                        </p>
                        <p className="mt-1 flex flex-wrap items-center gap-2 font-medium text-slate-900">
                          <span
                            className={`inline-flex rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide ${sb.cls}`}
                          >
                            {sb.label}
                          </span>
                          <span>
                            {r.reviewerName?.trim() || 'Alıcı'}{' '}
                            <span className="text-amber-500 tabular-nums">{'★'.repeat(r.rating)}</span>
                          </span>
                        </p>
                      </div>
                      <time
                        className="text-xs text-slate-400 tabular-nums shrink-0"
                        dateTime={r.createdAt}
                      >
                        {new Date(r.createdAt).toLocaleDateString('az-AZ', {
                          day: 'numeric',
                          month: 'short',
                          year: 'numeric',
                        })}
                      </time>
                    </div>
                    <p className="mt-2 text-sm text-slate-700 leading-relaxed">{reviewBodyText(r) || '—'}</p>
                  </article>
                  )
                })}
            </div>
          </section>
        </>
      )}
    </div>
  )
}
