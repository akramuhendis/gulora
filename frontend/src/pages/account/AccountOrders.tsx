import { useState } from "react";
import { Link } from "react-router-dom";
import { Package, ExternalLink, RotateCcw, X, ChevronDown, ChevronUp, MapPin, Clock, CreditCard, CheckCircle } from "lucide-react";
import { useOrders, orderStatusLabel, orderStatusTone, deliveryTypeLabel } from "../../context/OrderContext";
import type { OrderStatus } from "../../context/OrderContext";
import { formatMoney } from "../../lib/format";

const PROGRESS_STEPS: { status: OrderStatus; label: string }[] = [
  { status: "pending",    label: "Gözləmədə" },
  { status: "confirmed",  label: "Təsdiqləndi" },
  { status: "preparing",  label: "Hazırlanır" },
  { status: "shipped",    label: "Yoldadır" },
  { status: "delivered",  label: "Çatdırıldı" },
];

const STEP_INDEX: Partial<Record<OrderStatus, number>> = {
  pending: 0, confirmed: 1, preparing: 2, shipped: 3, delivered: 4,
};

function OrderProgressBar({ status }: { status: OrderStatus }) {
  if (status === "cancelled" || status === "return_requested") return null;
  const current = STEP_INDEX[status] ?? -1;
  return (
    <div className="px-4 pt-3 pb-1">
      <div className="flex items-center justify-between">
        {PROGRESS_STEPS.map((step, i) => {
          const done = i < current;
          const active = i === current;
          return (
            <div key={step.status} className="flex-1 flex flex-col items-center relative">
              {i < PROGRESS_STEPS.length - 1 && (
                <div className={`absolute top-3 left-1/2 right-0 h-0.5 ${done || active ? "bg-[#65a30d]" : "bg-gray-200"}`} />
              )}
              <div className={`w-6 h-6 rounded-full flex items-center justify-center z-10 border-2 transition-colors ${
                done
                  ? "bg-[#65a30d] border-[#65a30d] text-white"
                  : active
                  ? "bg-white border-[#65a30d] text-[#65a30d]"
                  : "bg-white border-gray-200 text-gray-300"
              }`}>
                {done ? <CheckCircle size={12} /> : <span className="text-[9px] font-bold">{i + 1}</span>}
              </div>
              <span className={`text-[9px] mt-1 font-medium ${active ? "text-[#65a30d]" : done ? "text-gray-600" : "text-gray-300"}`}>
                {step.label}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

const filters: { value: "all" | OrderStatus; label: string }[] = [
  { value: "all", label: "Hamısı" },
  { value: "delivered", label: "Çatdırılıb" },
  { value: "shipped", label: "Yoldadır" },
  { value: "preparing", label: "Hazırlanır" },
  { value: "confirmed", label: "Təsdiqləndi" },
  { value: "pending", label: "Gözləmədə" },
  { value: "cancelled", label: "Ləğv edilib" },
  { value: "return_requested", label: "İadə sorğusu" },
];

function ReturnModal({ orderId, onClose, onSubmit }: { orderId: string; onClose: () => void; onSubmit: (reason: string) => void }) {
  const [reason, setReason] = useState("");
  const REASONS = ["Məhsul zədəlidir", "Yanlış məhsul göndərilib", "Məhsul gözləntiləri qarşılamadı", "Sifarişi ləğv etmək istəyirəm", "Digər"];
  const [custom, setCustom] = useState("");

  const handleSubmit = () => {
    const r = reason === "Digər" ? custom : reason;
    if (!r.trim()) return;
    onSubmit(r);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl p-6 w-full max-w-sm">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-bold text-gray-800">İadə / Şikayət Sorğusu</h3>
          <button onClick={onClose}><X size={18} className="text-gray-400" /></button>
        </div>
        <p className="text-xs text-gray-500 mb-3">Sifariş: {orderId}</p>
        <div className="space-y-2 mb-4">
          {REASONS.map((r) => (
            <label key={r} className="flex items-center gap-2.5 cursor-pointer p-2.5 rounded-xl hover:bg-gray-50">
              <input type="radio" name="reason" value={r} checked={reason === r} onChange={() => setReason(r)} className="accent-[#65a30d]" />
              <span className="text-sm text-gray-700">{r}</span>
            </label>
          ))}
        </div>
        {reason === "Digər" && (
          <textarea value={custom} onChange={(e) => setCustom(e.target.value)} rows={3}
            placeholder="Səbəbi ətraflı izah edin..."
            className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-none mb-4" />
        )}
        <div className="flex gap-2">
          <button onClick={handleSubmit} disabled={!reason || (reason === "Digər" && !custom.trim())}
            className="flex-1 bg-[#65a30d] text-white py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f] disabled:opacity-50">
            Sorğu göndər
          </button>
          <button onClick={onClose} className="flex-1 border border-gray-200 text-gray-600 py-2.5 rounded-xl text-sm font-semibold hover:bg-gray-50">
            Ləğv et
          </button>
        </div>
      </div>
    </div>
  );
}

export default function AccountOrders() {
  const { orders, requestReturn, cancelOrder } = useOrders();
  const [filter, setFilter] = useState<"all" | OrderStatus>("all");
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [returnOrderId, setReturnOrderId] = useState<string | null>(null);

  const list = filter === "all" ? orders : orders.filter((o) => o.status === filter);

  const handleReturn = (reason: string) => {
    if (returnOrderId) {
      requestReturn(returnOrderId, reason);
      setReturnOrderId(null);
    }
  };

  return (
    <>
      {returnOrderId && (
        <ReturnModal orderId={returnOrderId} onClose={() => setReturnOrderId(null)} onSubmit={handleReturn} />
      )}
      <div className="space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Sifarişlərim</h1>
          <p className="text-sm text-gray-500 mt-1">Bütün sifariş tarixçəniz və statuslar.</p>
        </div>

        <div className="flex flex-wrap gap-2">
          {filters.map((f) => (
            <button key={f.value} type="button" onClick={() => setFilter(f.value)}
              className={`px-3 py-1.5 rounded-full text-xs font-semibold border transition-colors ${filter === f.value ? "bg-[#65a30d] text-white border-[#65a30d]" : "bg-white text-gray-600 border-gray-200 hover:border-lime-300"}`}>
              {f.label}
            </button>
          ))}
        </div>

        {list.length === 0 ? (
          <div className="bg-white rounded-2xl border border-gray-100 p-12 text-center">
            <Package className="mx-auto text-gray-300 mb-3" size={40} />
            <p className="text-gray-600 font-medium">Bu filtrəyə uyğun sifariş yoxdur.</p>
            <Link to="/mehsullar" className="mt-4 inline-block text-sm text-[#65a30d] hover:underline">Alış-verişə başla</Link>
          </div>
        ) : (
          <div className="space-y-4">
            {list.map((order) => {
              const isExpanded = expandedId === order.id;
              const canCancel = order.status === "pending" || order.status === "confirmed";
              const canReturn = order.status === "delivered";
              return (
                <article key={order.id} className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
                  <div className="flex flex-wrap items-center justify-between gap-2 px-4 py-3 bg-gray-50 border-b border-gray-100">
                    <div>
                      <p className="font-semibold text-gray-800">{order.id}</p>
                      <p className="text-xs text-gray-500">
                        {new Date(order.placedAt).toLocaleDateString("az-AZ", { weekday: "short", day: "numeric", month: "long", year: "numeric" })}
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className={`text-xs font-semibold px-2.5 py-1 rounded-full border ${orderStatusTone(order.status)}`}>
                        {orderStatusLabel(order.status)}
                      </span>
                      <button type="button" onClick={() => setExpandedId(isExpanded ? null : order.id)}
                        className="text-gray-400 hover:text-gray-600">
                        {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
                      </button>
                    </div>
                  </div>

                  <OrderProgressBar status={order.status} />

                  <ul className="divide-y divide-gray-100">
                    {order.items.map((item, i) => (
                      <li key={i} className="px-4 py-3 flex flex-wrap items-center justify-between gap-2">
                        <div className="flex items-center gap-3 min-w-0">
                          <img src={item.image} alt={item.name} className="w-10 h-10 rounded-lg object-cover shrink-0" />
                          <div className="min-w-0">
                            <p className="text-sm font-medium text-gray-800 line-clamp-1">{item.name}</p>
                            {item.variantLabel && <p className="text-xs text-gray-400">{item.variantLabel}</p>}
                            <p className="text-xs text-gray-500">{item.quantity} ədəd × {formatMoney(item.price)}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Link to={`/urun/${item.productSlug}`}
                            className="text-xs text-[#65a30d] font-medium flex items-center gap-0.5 hover:underline shrink-0">
                            <ExternalLink size={12} />
                          </Link>
                        </div>
                      </li>
                    ))}
                  </ul>

                  {isExpanded && (
                    <div className="border-t border-gray-100 px-4 py-4 space-y-3 text-sm text-gray-600">
                      <div className="grid grid-cols-2 gap-3">
                        <div className="flex items-start gap-2">
                          <MapPin size={14} className="text-gray-400 mt-0.5 shrink-0" />
                          <div>
                            <p className="text-xs text-gray-400">Çatdırılma ünvanı</p>
                            <p className="font-medium text-gray-700">{order.address.city}, {order.address.district}</p>
                            <p className="text-xs">{order.address.line}</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-2">
                          <Clock size={14} className="text-gray-400 mt-0.5 shrink-0" />
                          <div>
                            <p className="text-xs text-gray-400">Çatdırılma</p>
                            <p className="font-medium text-gray-700">{new Date(order.deliveryDate + "T00:00:00").toLocaleDateString("az-AZ", { day: "numeric", month: "long" })}</p>
                            <p className="text-xs">{deliveryTypeLabel(order.deliveryType)}</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-2">
                          <CreditCard size={14} className="text-gray-400 mt-0.5 shrink-0" />
                          <div>
                            <p className="text-xs text-gray-400">Ödəniş</p>
                            <p className="font-medium text-gray-700">{order.paymentMethod === "card" ? "Kart" : order.paymentMethod === "cash" ? "Nağd" : "Köçürmə"}</p>
                            {order.cardLast4 && <p className="text-xs">•••• {order.cardLast4}</p>}
                          </div>
                        </div>
                      </div>

                      {order.tracking.length > 0 && (
                        <div>
                          <p className="text-xs font-medium text-gray-600 mb-2">İzləmə tarixçəsi</p>
                          <div className="space-y-1.5">
                            {[...order.tracking].reverse().map((t, i) => (
                              <div key={i} className="flex items-start gap-2 text-xs">
                                <span className="w-1.5 h-1.5 rounded-full bg-[#65a30d] mt-1.5 shrink-0" />
                                <div>
                                  <span className="font-medium text-gray-700">{orderStatusLabel(t.status)}</span>
                                  <span className="text-gray-400 ml-2">{new Date(t.timestamp).toLocaleString("az-AZ")}</span>
                                  {t.note && <p className="text-gray-500">{t.note}</p>}
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="flex flex-wrap gap-2 pt-2 border-t border-gray-100">
                        {canCancel && (
                          <button type="button" onClick={() => cancelOrder(order.id)}
                            className="px-3 py-1.5 text-xs font-semibold text-red-600 border border-red-200 rounded-xl hover:bg-red-50">
                            Sifarişi ləğv et
                          </button>
                        )}
                        {canReturn && (
                          <button type="button" onClick={() => setReturnOrderId(order.id)}
                            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-orange-600 border border-orange-200 rounded-xl hover:bg-orange-50">
                            <RotateCcw size={12} /> İadə / Şikayət
                          </button>
                        )}
                        {order.returnReason && (
                          <span className="px-3 py-1.5 text-xs text-orange-700 bg-orange-50 rounded-xl border border-orange-100">
                            İadə səbəbi: {order.returnReason}
                          </span>
                        )}
                      </div>
                    </div>
                  )}

                  <div className="px-4 py-3 flex justify-between items-center bg-gray-50/80">
                    <span className="text-sm text-gray-600">Cəm</span>
                    <span className="text-base font-bold text-gray-900">{formatMoney(order.totalPrice)}</span>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </div>
    </>
  );
}
