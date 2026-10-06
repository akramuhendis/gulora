import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft, Gift, Loader2, Truck } from 'lucide-react'
import { ApiError, sellerPortalApi, type OrderResponse } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'
import { CARGO_COMPANY_PRESETS } from '../data/partnerHub'

function formatMoney(n: number): string {
  return (
    new Intl.NumberFormat('az-AZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
      n
    ) + ' ₼'
  )
}

/** Satıcı PATCH /status — backend yalnız Confirmed, Processing, Delivered, Returned keçidlərini icazə verir. */
function sellerStatusesAllowedForPatch(current: string): string[] {
  switch (current) {
    case 'Pending':
      return ['Pending', 'Confirmed']
    case 'Confirmed':
      return ['Confirmed', 'Processing']
    case 'Processing':
      return ['Processing']
    case 'Shipped':
      return ['Shipped', 'Delivered']
    case 'Delivered':
      return ['Delivered']
    case 'ReturnRequested':
      return ['ReturnRequested', 'Returned']
    case 'Cancelled':
    case 'Returned':
      return [current]
    default:
      return current ? [current] : []
  }
}

const STATUS_LABEL_AZ: Record<string, string> = {
  Pending: 'Gözləmədə',
  Confirmed: 'Təsdiqlənib',
  Processing: 'Hazırlıqda',
  Shipped: 'Yoldadır',
  Delivered: 'Çatdırılıb',
  Cancelled: 'Ləğv olunub',
  ReturnRequested: 'İadə tələbi',
  Returned: 'İadə tamamlanıb',
}

function statusOptionLabel(apiValue: string): string {
  return STATUS_LABEL_AZ[apiValue] ?? apiValue
}

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [order, setOrder] = useState<OrderResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const [shipOpen, setShipOpen] = useState(false)
  const [cargoCompany, setCargoCompany] = useState('')
  const [trackingNumber, setTrackingNumber] = useState('')

  const [newStatus, setNewStatus] = useState('')
  const [statusNote, setStatusNote] = useState('')

  const detailTitle = order ? `Sifariş ${order.orderNumber}` : 'Sifariş təfsilatı'
  usePageMeta(detailTitle)

  useEffect(() => {
    if (!shipOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setShipOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [shipOpen])

  const load = useCallback(async () => {
    if (!id) return
    setLoading(true)
    setError(null)
    try {
      const o = await sellerPortalApi.getOrder(id)
      setOrder(o)
      setNewStatus(o.status)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Sifariş tapılmadı.')
      setOrder(null)
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    queueMicrotask(() => {
      void load()
    })
  }, [load])

  const statusPatchOptions = useMemo(() => {
    if (!order) return []
    return sellerStatusesAllowedForPatch(order.status)
  }, [order])

  useEffect(() => {
    if (!order) return
    if (!statusPatchOptions.includes(newStatus)) {
      queueMicrotask(() => {
        setNewStatus(order.status)
      })
    }
  }, [order, newStatus, statusPatchOptions])

  async function submitStatus() {
    if (!id || !newStatus) return
    setActionError(null)
    setPending(true)
    try {
      await sellerPortalApi.updateOrderStatus(id, newStatus, statusNote.trim() || undefined)
      setStatusNote('')
      await load()
    } catch (e) {
      setActionError(e instanceof ApiError ? e.message : 'Status yenilənmədi.')
    } finally {
      setPending(false)
    }
  }

  async function submitShip() {
    if (!id || !cargoCompany.trim() || !trackingNumber.trim()) return
    setActionError(null)
    setPending(true)
    try {
      await sellerPortalApi.shipOrder(id, cargoCompany.trim(), trackingNumber.trim())
      setShipOpen(false)
      setCargoCompany('')
      setTrackingNumber('')
      await load()
    } catch (e) {
      setActionError(e instanceof ApiError ? e.message : 'Kargo qeydi alınmadı.')
    } finally {
      setPending(false)
    }
  }

  if (!id) {
    return (
      <p className="text-slate-600">
        Yanlış ünvan. <Link to="/orders">Siyahıya qayıt</Link>
      </p>
    )
  }

  if (loading) {
    return (
      <div className="flex justify-center py-20 text-slate-500">
        <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
      </div>
    )
  }

  if (error || !order) {
    return (
      <div>
        <p className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800">{error}</p>
        <button
          type="button"
          onClick={() => navigate('/orders')}
          className="mt-4 text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
        >
          Sifarişlərə qayıt
        </button>
      </div>
    )
  }

  const canShip = order.status === 'Processing'

  return (
    <div>
      <button
        type="button"
        onClick={() => navigate('/orders')}
        className="mb-6 inline-flex items-center gap-1 text-sm font-medium text-slate-600 hover:text-slate-900"
      >
        <ArrowLeft className="h-4 w-4" aria-hidden />
        Sifarişlərə qayıt
      </button>

      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">{order.orderNumber}</h1>
          <p className="mt-1 text-slate-600">{order.statusLabel}</p>
          <p className="text-xs text-slate-500">
            Yaradılıb: {new Date(order.createdAt).toLocaleString('az-AZ')}
          </p>
        </div>
        <div className="text-right">
          <p className="text-sm text-slate-500">Ümumi</p>
          <p className="text-xl font-bold tabular-nums text-slate-900">
            {formatMoney(order.totalAmount)}
          </p>
        </div>
      </div>

      {actionError && (
        <p className="mt-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-800" role="alert">
          {actionError}
        </p>
      )}

      <div className="mt-8 grid gap-6 lg:grid-cols-2">
        <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">
            Çatdırılma ünvanı
          </h2>
          <div className="mt-3 text-sm text-slate-800">
            <p className="font-medium">{order.address.recipientName}</p>
            <p>{order.address.phoneNumber}</p>
            <p className="mt-2">
              {order.address.district}, {order.address.city}
            </p>
            <p className="mt-1">{order.address.fullLine}</p>
            {order.address.postalCode && (
              <p className="text-slate-500">{order.address.postalCode}</p>
            )}
          </div>
          {(order.cargoCompany || order.trackingNumber) && (
            <div className="mt-4 rounded-lg bg-slate-50 p-3 text-sm">
              <p>
                <span className="text-slate-500">Daşıyıcı:</span>{' '}
                {order.cargoCompany ?? '—'}
              </p>
              <p>
                <span className="text-slate-500">İzləmə:</span>{' '}
                {order.trackingNumber ?? '—'}
              </p>
            </div>
          )}
        </section>

        <section className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-500">
            Əməliyyatlar
          </h2>
          <p className="mt-2 text-xs text-slate-500">
            Açılan siyahı yalnız bu sifariş üçün icazə verilən status keçidlərini göstərir (server
            qaydaları ilə uyğun). Axın: gözləmədə → təsdiqlənib → hazırlıqda →{' '}
            <strong>Kargoya ver</strong> (yoldadır) → çatdırılıb.
          </p>
          {order.status === 'Processing' && (
            <p className="mt-1 text-xs text-slate-600">
              &quot;Yoldadır&quot; statusu buradan seçilmir — &quot;Kargoya ver&quot; düyməsi ilə
              əlavə olunur.
            </p>
          )}

          <div className="mt-4 space-y-3">
            <div>
              <label htmlFor="new-status" className="block text-xs font-medium text-slate-600">
                Yeni status
              </label>
              <select
                id="new-status"
                value={newStatus}
                onChange={(e) => setNewStatus(e.target.value)}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
              >
                {statusPatchOptions.map((s) => (
                  <option key={s} value={s}>
                    {statusOptionLabel(s)}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="status-note" className="block text-xs font-medium text-slate-600">
                Qeyd (istəyə bağlı)
              </label>
              <input
                id="status-note"
                value={statusNote}
                onChange={(e) => setStatusNote(e.target.value)}
                className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
              />
            </div>
            <button
              type="button"
              disabled={pending || newStatus === order.status}
              onClick={() => void submitStatus()}
              className="w-full rounded-lg bg-slate-900 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-40"
            >
              Statusu yenilə
            </button>
          </div>

          {!canShip && (
            <p className="mt-3 text-xs text-amber-800">
              Kargoya vermək üçün əvvəl &quot;Təsdiqlənib&quot;, sonra &quot;Hazırlıqda&quot; statusunu
              seçib yadda saxlayın.
            </p>
          )}
          <button
            type="button"
            disabled={!canShip || pending}
            title={
              canShip
                ? undefined
                : 'Kargoya vermək üçün əvvəl statusu təsdiqləyib hazırlıqda göstərin.'
            }
            onClick={() => setShipOpen(true)}
            className="mt-4 flex w-full items-center justify-center gap-2 rounded-lg border border-[#006437] py-2 text-sm font-semibold text-[#006437] hover:bg-green-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:border-slate-300 disabled:text-slate-400 disabled:hover:bg-transparent"
          >
            <Truck className="h-4 w-4" aria-hidden />
            Kargoya ver
          </button>
        </section>
      </div>

      {(order.giftCardMessage || order.giftCardSenderName || order.giftCardOccasion) && (
        <section
          className="mt-6 rounded-xl border border-amber-200/80 bg-amber-50/60 p-5 shadow-sm"
          aria-labelledby="seller-gift-card-heading"
        >
          <h2
            id="seller-gift-card-heading"
            className="text-sm font-semibold text-[#713f12] flex items-center gap-2"
          >
            <Gift className="h-4 w-4 shrink-0" aria-hidden />
            Kart mesajı
          </h2>
          <div className="mt-3 space-y-2 text-sm">
            {order.giftCardOccasion && (
              <p className="text-slate-700">
                <span className="text-slate-500">Məqsəd:</span> {order.giftCardOccasion}
              </p>
            )}
            {order.giftCardSenderName && (
              <p className="text-slate-700">
                <span className="text-slate-500">Göndərən:</span> {order.giftCardSenderName}
              </p>
            )}
            {order.giftCardMessage && (
              <p className="whitespace-pre-wrap text-slate-900">{order.giftCardMessage}</p>
            )}
          </div>
        </section>
      )}

      <section className="mt-8 rounded-xl border border-slate-200 bg-white shadow-sm">
        <h2 className="border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-700">
          Məhsul sətirləri
        </h2>
        <table className="w-full text-left text-sm">
          <thead className="bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th className="px-5 py-2">Məhsul</th>
              <th className="px-5 py-2 text-right">Vahid</th>
              <th className="px-5 py-2 text-right">Say</th>
              <th className="px-5 py-2 text-right">Cəm</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {order.items.map((row) => (
              <tr key={row.id}>
                <td className="px-5 py-3">
                  <p className="font-medium text-slate-900">{row.productName}</p>
                  {row.productSku && (
                    <p className="text-xs text-slate-500">SKU: {row.productSku}</p>
                  )}
                </td>
                <td className="px-5 py-3 text-right tabular-nums">{formatMoney(row.unitPrice)}</td>
                <td className="px-5 py-3 text-right tabular-nums">{row.quantity}</td>
                <td className="px-5 py-3 text-right tabular-nums font-medium">
                  {formatMoney(row.lineTotal)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {order.trackings.length > 0 && (
        <section className="mt-8 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="text-sm font-semibold text-slate-700">Status tarixçəsi</h2>
          <ul className="mt-3 space-y-2 text-sm">
            {order.trackings.map((t) => (
              <li key={t.id} className="flex flex-wrap gap-2 border-l-2 border-[#006437] pl-3">
                <span className="font-medium text-slate-900">{t.statusLabel}</span>
                <span className="text-slate-500">
                  {new Date(t.createdAt).toLocaleString('az-AZ')}
                </span>
                {t.note && <span className="w-full text-slate-600">{t.note}</span>}
              </li>
            ))}
          </ul>
        </section>
      )}

      {shipOpen && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
          role="dialog"
          aria-modal="true"
          aria-labelledby="ship-title"
        >
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <h2 id="ship-title" className="text-lg font-bold text-slate-900">
              Kargo məlumatı
            </h2>
            <div className="mt-4 space-y-3">
              <div>
                <label htmlFor="cargo-co" className="block text-xs font-medium text-slate-600">
                  Daşıyıcı şirkət
                </label>
                <input
                  id="cargo-co"
                  list="cargo-presets"
                  value={cargoCompany}
                  onChange={(e) => setCargoCompany(e.target.value)}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                />
                <datalist id="cargo-presets">
                  {CARGO_COMPANY_PRESETS.map((c) => (
                    <option key={c} value={c} />
                  ))}
                </datalist>
              </div>
              <div>
                <label htmlFor="track-no" className="block text-xs font-medium text-slate-600">
                  İzləmə nömrəsi
                </label>
                <input
                  id="track-no"
                  value={trackingNumber}
                  onChange={(e) => setTrackingNumber(e.target.value)}
                  className="mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm"
                />
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setShipOpen(false)}
                className="rounded-lg px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
              >
                Ləğv et
              </button>
              <button
                type="button"
                disabled={pending || !cargoCompany.trim() || !trackingNumber.trim()}
                onClick={() => void submitShip()}
                className="rounded-lg bg-[#006437] px-4 py-2 text-sm font-semibold text-white hover:bg-[#005029] disabled:opacity-40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
              >
                Təsdiq et
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
