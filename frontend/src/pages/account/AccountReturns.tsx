import { useState } from "react";
import { Link } from "react-router-dom";
import { RotateCcw, Package, ExternalLink, ChevronDown, ChevronUp } from "lucide-react";
import { useOrders, orderStatusLabel, orderStatusTone } from "../../context/OrderContext";
import { formatMoney } from "../../lib/format";

export default function AccountReturns() {
  const { orders, requestReturn } = useOrders();
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [reasonInputs, setReasonInputs] = useState<Record<string, string>>({});
  const [submitted, setSubmitted] = useState<Set<string>>(new Set());

  const returnOrders = orders.filter(
    (o) => o.status === "delivered" || o.status === "return_requested" || o.status === "returned"
  );

  const REASONS = [
    "Məhsul zədəlidir",
    "Yanlış məhsul göndərilib",
    "Məhsul gözləntiləri qarşılamadı",
    "Çatdırılma gecikmişdir",
    "Digər",
  ];

  const handleSubmit = (orderId: string) => {
    const r = reasonInputs[orderId];
    if (!r?.trim()) return;
    requestReturn(orderId, r);
    setSubmitted((prev) => new Set(prev).add(orderId));
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-800">İadə / Şikayət</h1>
        <p className="text-sm text-gray-500 mt-1">Çatdırılmış sifarişlər üçün iadə sorğusu göndərin.</p>
      </div>

      <div className="bg-blue-50 border border-blue-200 rounded-2xl p-4 text-sm text-blue-700">
        <p className="font-semibold mb-1">İadə siyasəti</p>
        <ul className="space-y-1 text-xs list-disc list-inside text-blue-600">
          <li>Çatdırılmadan sonra 24 saat ərzində problem bildirin.</li>
          <li>Zədəli və ya yanlış məhsullar pulsuz dəyişdirilir.</li>
          <li>İadə prosesi 2-3 iş günü ərzində tamamlanır.</li>
          <li>Əlavə suallar üçün *6500 nömrəsinə zəng edin.</li>
        </ul>
      </div>

      {returnOrders.length === 0 ? (
        <div className="bg-white rounded-2xl border border-gray-100 p-12 text-center">
          <RotateCcw className="mx-auto text-gray-300 mb-3" size={40} />
          <p className="text-gray-600 font-medium">İadə üçün uyğun sifariş tapılmadı.</p>
          <p className="text-xs text-gray-400 mt-1">Yalnız çatdırılmış sifarişlər üçün iadə sorğusu göndərə bilərsiniz.</p>
          <Link to="/hesabim/sifarisler" className="mt-4 inline-block text-sm text-[#65a30d] hover:underline">
            Bütün sifarişlərə bax
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {returnOrders.map((order) => {
            const isExpanded = expandedId === order.id;
            const isAlreadyReturned = order.status === "return_requested" || order.status === "returned";
            const isSubmitted = submitted.has(order.id);
            return (
              <article key={order.id} className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
                <div className="flex flex-wrap items-center justify-between gap-2 px-4 py-3 bg-gray-50 border-b border-gray-100">
                  <div>
                    <p className="font-semibold text-gray-800">{order.id}</p>
                    <p className="text-xs text-gray-500">
                      {new Date(order.placedAt).toLocaleDateString("az-AZ", { day: "numeric", month: "long", year: "numeric" })}
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className={`text-xs font-semibold px-2.5 py-1 rounded-full border ${orderStatusTone(order.status)}`}>
                      {orderStatusLabel(order.status)}
                    </span>
                    <button type="button" onClick={() => setExpandedId(isExpanded ? null : order.id)} className="text-gray-400">
                      {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
                    </button>
                  </div>
                </div>

                <ul className="divide-y divide-gray-100">
                  {order.items.map((item, i) => (
                    <li key={i} className="px-4 py-3 flex items-center justify-between gap-2">
                      <div className="flex items-center gap-3 min-w-0">
                        <img src={item.image} alt={item.name} className="w-10 h-10 rounded-lg object-cover shrink-0" />
                        <div className="min-w-0">
                          <p className="text-sm font-medium text-gray-800 line-clamp-1">{item.name}</p>
                          <p className="text-xs text-gray-500">{item.quantity} ədəd × {formatMoney(item.price)}</p>
                        </div>
                      </div>
                      <Link to={`/urun/${item.productSlug}`} className="text-xs text-[#65a30d] hover:underline flex items-center gap-0.5 shrink-0">
                        <ExternalLink size={12} />
                      </Link>
                    </li>
                  ))}
                </ul>

                {isExpanded && (
                  <div className="border-t border-gray-100 px-4 py-4">
                    {isAlreadyReturned ? (
                      <div className="text-sm text-orange-700 bg-orange-50 rounded-xl p-3">
                        {order.status === "return_requested" ? (
                          <>
                            <p className="font-semibold">İadə sorğusu göndərildi.</p>
                            {order.returnReason && <p className="text-xs mt-1">Səbəb: {order.returnReason}</p>}
                            {order.returnRequestedAt && (
                              <p className="text-xs text-gray-500 mt-0.5">
                                {new Date(order.returnRequestedAt).toLocaleString("az-AZ")}
                              </p>
                            )}
                          </>
                        ) : (
                          <p className="font-semibold">İadə tamamlandı.</p>
                        )}
                      </div>
                    ) : isSubmitted ? (
                      <div className="text-sm text-green-700 bg-green-50 rounded-xl p-3">
                        <p className="font-semibold">Sorğunuz qəbul edildi.</p>
                        <p className="text-xs mt-1">Müştəri xidmətimiz tezliklə sizinlə əlaqə saxlayacaq.</p>
                      </div>
                    ) : (
                      <div className="space-y-3">
                        <p className="text-sm font-medium text-gray-700">İadə səbəbi seçin:</p>
                        <div className="space-y-2">
                          {REASONS.map((r) => (
                            <label key={r} className="flex items-center gap-2.5 cursor-pointer">
                              <input type="radio" name={`reason_${order.id}`}
                                checked={reasonInputs[order.id] === r}
                                onChange={() => setReasonInputs((prev) => ({ ...prev, [order.id]: r }))}
                                className="accent-[#65a30d]" />
                              <span className="text-sm text-gray-700">{r}</span>
                            </label>
                          ))}
                        </div>
                        {reasonInputs[order.id] === "Digər" && (
                          <textarea
                            rows={3}
                            placeholder="Səbəbi ətraflı izah edin..."
                            onChange={(e) => setReasonInputs((prev) => ({ ...prev, [order.id]: e.target.value }))}
                            className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-none" />
                        )}
                        <button type="button" onClick={() => handleSubmit(order.id)}
                          disabled={!reasonInputs[order.id]?.trim() || reasonInputs[order.id] === "Digər"}
                          className="flex items-center gap-2 bg-[#65a30d] text-white px-4 py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f] disabled:opacity-50">
                          <RotateCcw size={14} /> Sorğu göndər
                        </button>
                        <p className="text-xs text-gray-400 flex items-center gap-1">
                          <Package size={11} /> Çatdırılmadan sonra 24 saat ərzində sorğu göndərin
                        </p>
                      </div>
                    )}
                  </div>
                )}

                <div className="px-4 py-3 flex justify-between items-center bg-gray-50/80 text-sm">
                  <span className="text-gray-600">Cəm</span>
                  <span className="font-bold text-gray-900">{formatMoney(order.totalPrice)}</span>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </div>
  );
}
