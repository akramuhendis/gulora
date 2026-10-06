import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Loader2, MessageSquare, Send } from 'lucide-react'
import { ApiError, sellerPortalApi, type OrderSummaryResponse } from '../services/api'
import { usePageMeta } from '../hooks/usePageMeta'

type Thread = {
  id: string
  subject: string
  preview: string
  unread: boolean
  updatedAt: string
  orderRef?: string | null
  messages: { id: string; from: 'buyer' | 'seller'; body: string; at: string }[]
}

const MOCK_THREADS: Thread[] = [
  {
    id: 't-demo-1',
    subject: 'Çatdırılma vaxtı sorğusu',
    preview: 'Sabah çatdırılma mümkündürmü?',
    unread: true,
    updatedAt: new Date().toISOString(),
    orderRef: null,
    messages: [
      {
        id: 'm1',
        from: 'buyer',
        body: 'Sabah çatdırılma mümkündürmü?',
        at: new Date(Date.now() - 3600_000).toISOString(),
      },
    ],
  },
  {
    id: 't-demo-2',
    subject: 'Məhsul ölçüsü',
    preview: 'Qablaşdırma ölçüləri haqqında…',
    unread: false,
    updatedAt: new Date(Date.now() - 86400_000).toISOString(),
    messages: [
      {
        id: 'm2',
        from: 'buyer',
        body: 'Qablaşdırma ölçüləri haqqında təsdiq ala bilərəmmi?',
        at: new Date(Date.now() - 86400_000).toISOString(),
      },
      {
        id: 'm3',
        from: 'seller',
        body: 'Salam! Qutu 30×20×10 sm olaraq göndərilir.',
        at: new Date(Date.now() - 86000_000).toISOString(),
      },
    ],
  },
]

function formatShort(iso: string): string {
  return new Date(iso).toLocaleString('az-AZ', { dateStyle: 'short', timeStyle: 'short' })
}

export default function MessagesPage() {
  usePageMeta('Mesajlar')

  const [threads, setThreads] = useState<Thread[]>(MOCK_THREADS)
  const [selectedId, setSelectedId] = useState<string | null>(MOCK_THREADS[0]?.id ?? null)
  const [reply, setReply] = useState('')
  const [recentOrders, setRecentOrders] = useState<OrderSummaryResponse[]>([])
  const [ordersLoading, setOrdersLoading] = useState(true)

  const loadOrders = useCallback(async () => {
    setOrdersLoading(true)
    try {
      const res = await sellerPortalApi.listOrders({ page: 1, pageSize: 5 })
      setRecentOrders(res.items)
    } catch (e) {
      if (e instanceof ApiError && e.status !== 401) {
        /* opsional kontekst */
      }
      setRecentOrders([])
    } finally {
      setOrdersLoading(false)
    }
  }, [])

  useEffect(() => {
    queueMicrotask(() => void loadOrders())
  }, [loadOrders])

  const selected = useMemo(
    () => threads.find((t) => t.id === selectedId) ?? null,
    [threads, selectedId]
  )

  const unreadCount = useMemo(() => threads.filter((t) => t.unread).length, [threads])

  function sendReply() {
    const text = reply.trim()
    if (!text || !selected) return
    const msg = {
      id: `m-${Date.now()}`,
      from: 'seller' as const,
      body: text,
      at: new Date().toISOString(),
    }
    setThreads((prev) =>
      prev.map((t) =>
        t.id === selected.id
          ? {
              ...t,
              unread: false,
              updatedAt: msg.at,
              preview: text.slice(0, 120),
              messages: [...t.messages, msg],
            }
          : t
      )
    )
    setReply('')
  }

  return (
    <div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
            Mesajlar və sorğular
          </h1>
          <p className="mt-2 max-w-3xl text-slate-600">
            Trendyol Partner tipli mövzu siyahısı, oxunmamış sayı və sifariş konteksti. Canlı API
            qoşulana qədər nümunə mövzular və yerli cavablar saxlanılır (sessiya daxilində).
          </p>
        </div>
        <div className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700">
          Oxunmamış: {unreadCount}
        </div>
      </div>

      <div className="mt-8 grid gap-6 lg:grid-cols-5">
        <section
          className="rounded-xl border border-slate-200 bg-white shadow-sm lg:col-span-2"
          aria-labelledby="thread-list"
        >
          <h2 id="thread-list" className="sr-only">
            Mövzular
          </h2>
          <ul className="divide-y divide-slate-100">
            {threads.map((t) => {
              const active = t.id === selectedId
              return (
                <li key={t.id}>
                  <button
                    type="button"
                    onClick={() => {
                      setSelectedId(t.id)
                      setThreads((prev) =>
                        prev.map((x) => (x.id === t.id ? { ...x, unread: false } : x))
                      )
                    }}
                    className={`flex w-full flex-col gap-1 px-4 py-3 text-left text-sm transition hover:bg-slate-50 ${
                      active ? 'bg-green-50/80 ring-1 ring-inset ring-[#006437]/25' : ''
                    }`}
                  >
                    <span className="flex items-center justify-between gap-2">
                      <span className="font-semibold text-slate-900">{t.subject}</span>
                      {t.unread && (
                        <span className="h-2 w-2 shrink-0 rounded-full bg-[#006437]" aria-label="Oxunmayıb" />
                      )}
                    </span>
                    <span className="line-clamp-2 text-xs text-slate-600">{t.preview}</span>
                    <span className="text-[11px] text-slate-400">{formatShort(t.updatedAt)}</span>
                  </button>
                </li>
              )
            })}
          </ul>
        </section>

        <section
          className="flex min-h-[420px] flex-col rounded-xl border border-slate-200 bg-white shadow-sm lg:col-span-3"
          aria-labelledby="thread-detail"
        >
          <h2 id="thread-detail" className="sr-only">
            Söhbət
          </h2>
          {!selected ? (
            <p className="p-6 text-sm text-slate-600">Mövzu seçin.</p>
          ) : (
            <>
              <div className="border-b border-slate-100 px-5 py-4">
                <p className="font-semibold text-slate-900">{selected.subject}</p>
                {selected.orderRef && (
                  <p className="mt-1 text-xs text-slate-500">Sifariş: {selected.orderRef}</p>
                )}
              </div>
              <div className="flex-1 space-y-3 overflow-y-auto px-5 py-4">
                {selected.messages.map((m) => (
                  <div
                    key={m.id}
                    className={`max-w-[92%] rounded-2xl px-4 py-2 text-sm ${
                      m.from === 'seller'
                        ? 'ml-auto bg-[#006437] text-white'
                        : 'mr-auto bg-slate-100 text-slate-900'
                    }`}
                  >
                    <p>{m.body}</p>
                    <p
                      className={`mt-1 text-[10px] ${
                        m.from === 'seller' ? 'text-white/80' : 'text-slate-500'
                      }`}
                    >
                      {formatShort(m.at)}
                    </p>
                  </div>
                ))}
              </div>
              <div className="border-t border-slate-100 p-4">
                <label htmlFor="reply-box" className="sr-only">
                  Cavab yazın
                </label>
                <textarea
                  id="reply-box"
                  value={reply}
                  onChange={(e) => setReply(e.target.value)}
                  rows={3}
                  placeholder="Cavab yazın (yerli nümunə — serverə göndərilmir)"
                  className="w-full rounded-xl border border-slate-300 px-3 py-2 text-sm"
                />
                <div className="mt-3 flex justify-end">
                  <button
                    type="button"
                    onClick={() => sendReply()}
                    disabled={!reply.trim()}
                    className="inline-flex items-center gap-2 rounded-lg bg-[#006437] px-4 py-2 text-sm font-semibold text-white hover:bg-[#005029] disabled:opacity-40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
                  >
                    <Send className="h-4 w-4" aria-hidden />
                    Göndər
                  </button>
                </div>
              </div>
            </>
          )}
        </section>
      </div>

      <section className="mt-10 rounded-xl border border-slate-200 bg-slate-50 p-5" aria-labelledby="order-context">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <h2 id="order-context" className="text-sm font-semibold uppercase tracking-wide text-slate-600">
            Son sifarişlər — kontekst keçidi
          </h2>
          <Link
            to="/orders"
            className="text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
          >
            Hamısı
          </Link>
        </div>
        {ordersLoading ? (
          <div className="mt-6 flex justify-center text-slate-500">
            <Loader2 className="h-8 w-8 animate-spin" aria-label="Yüklənir" />
          </div>
        ) : recentOrders.length === 0 ? (
          <p className="mt-4 text-sm text-slate-600">Sifariş yoxdur.</p>
        ) : (
          <ul className="mt-4 divide-y divide-slate-200 rounded-lg border border-slate-200 bg-white">
            {recentOrders.map((o) => (
              <li key={o.id} className="flex flex-wrap items-center justify-between gap-2 px-4 py-3 text-sm">
                <span className="font-medium text-slate-900">{o.orderNumber}</span>
                <Link
                  to={`/orders/${o.id}`}
                  className="font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
                >
                  Sifarişə keç
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      <div className="mt-10 flex flex-col items-center rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-10 text-center">
        <span className="flex h-14 w-14 items-center justify-center rounded-2xl bg-[#006437]/10 text-[#006437]">
          <MessageSquare className="h-7 w-7" aria-hidden />
        </span>
        <p className="mt-4 text-sm font-semibold text-slate-900">Canlı dəstək inteqrasiyası</p>
        <p className="mt-2 max-w-lg text-sm text-slate-600">
          Webhook və ya dəstək platforması qoşulanda mövzular serverdə saxlanılacaq, oxunmamış sayı
          naviqasiyada göstəriləcək.
        </p>
      </div>
    </div>
  )
}
