import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import {
  Package,
  ExternalLink,
  RotateCcw,
  X,
  ChevronDown,
  MapPin,
  Clock,
  CreditCard,
  CheckCircle,
  Search,
  Truck,
  Ban,
  Undo2,
  Clock3,
  Star,
  Gift,
} from "lucide-react";
import { useOrders } from "../../hooks/useOrders";
import { orderStatusLabel, orderStatusTone, deliveryTypeLabel } from "../../models/order";
import type { OrderStatus, Order } from "../../models/order";
import ProductCoverImage from "../../components/ProductCoverImage";
import { formatMoney } from "../../lib/format";

/** Brend yaşılı (digər hesab səhifələri ilə uyğun) */
const ACCENT = "#65a30d";
const BORDER = "#e6e6e6";
const PAGE_BG = "#f5f5f5";

const PROGRESS_STEPS: { status: OrderStatus; label: string }[] = [
  { status: "pending", label: "Gözləmədə" },
  { status: "confirmed", label: "Təsdiqləndi" },
  { status: "preparing", label: "Hazırlanır" },
  { status: "shipped", label: "Yoldadır" },
  { status: "delivered", label: "Çatdırıldı" },
];

const STEP_INDEX: Partial<Record<OrderStatus, number>> = {
  pending: 0,
  confirmed: 1,
  preparing: 2,
  shipped: 3,
  delivered: 4,
};

function OrderProgressBar({ status }: { status: OrderStatus }) {
  if (status === "cancelled" || status === "return_requested" || status === "returned") return null;
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
                <div
                  className={`absolute top-3 left-1/2 right-0 h-0.5 ${
                    done || active ? "bg-[#65a30d]" : "bg-gray-200"
                  }`}
                />
              )}
              <div
                className={`w-6 h-6 rounded-full flex items-center justify-center z-10 border-2 transition-colors ${
                  done
                    ? "bg-[#65a30d] border-[#65a30d] text-white"
                    : active
                      ? "bg-white border-[#65a30d] text-[#65a30d]"
                      : "bg-white border-gray-200 text-gray-300"
                }`}
              >
                {done ? <CheckCircle size={12} /> : <span className="text-[9px] font-bold">{i + 1}</span>}
              </div>
              <span
                className={`text-[9px] mt-1 font-medium ${
                  active ? "text-[#65a30d]" : done ? "text-gray-600" : "text-gray-300"
                }`}
              >
                {step.label}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

type CategoryTab = "all" | "active" | "cancelled" | "returns";

const CATEGORY_TABS: { id: CategoryTab; label: string }[] = [
  { id: "all", label: "Hamısı" },
  { id: "active", label: "Davam edənlər" },
  { id: "cancelled", label: "Ləğvlər" },
  { id: "returns", label: "İadələr" },
];

type DateFilter = "all" | "30d" | "90d" | "year";

const DATE_FILTERS: { id: DateFilter; label: string }[] = [
  { id: "all", label: "Bütün tarixlər" },
  { id: "30d", label: "Son 30 gün" },
  { id: "90d", label: "Son 90 gün" },
  { id: "year", label: "Bu il" },
];

function orderInCategory(order: Order, tab: CategoryTab): boolean {
  const s = order.status;
  switch (tab) {
    case "all":
      return true;
    case "active":
      return s === "pending" || s === "confirmed" || s === "preparing" || s === "shipped";
    case "cancelled":
      return s === "cancelled";
    case "returns":
      return s === "return_requested" || s === "returned";
    default:
      return true;
  }
}

function orderMatchesDateFilter(order: Order, df: DateFilter): boolean {
  if (df === "all") return true;
  const t = new Date(order.placedAt).getTime();
  const now = Date.now();
  if (df === "30d") return now - t <= 30 * 86400000;
  if (df === "90d") return now - t <= 90 * 86400000;
  if (df === "year") return new Date(order.placedAt).getFullYear() === new Date().getFullYear();
  return true;
}

function totalItemUnits(order: Order): number {
  return order.items.reduce((acc, it) => acc + it.quantity, 0);
}

function orderStatusHeadline(status: OrderStatus): { icon: typeof CheckCircle; tone: "ok" | "warn" | "bad"; title: string; sub: string } {
  switch (status) {
    case "delivered":
      return {
        icon: CheckCircle,
        tone: "ok",
        title: "Çatdırılıb",
        sub: "Məhsullar ünvana çatdırılıb.",
      };
    case "shipped":
      return {
        icon: Truck,
        tone: "ok",
        title: "Yoldadır",
        sub: "Sifarişiniz kuryerə təhvil verilib.",
      };
    case "preparing":
      return {
        icon: Clock3,
        tone: "warn",
        title: "Hazırlanır",
        sub: "Anbarımızda toplanır.",
      };
    case "confirmed":
      return {
        icon: Clock3,
        tone: "warn",
        title: "Təsdiqləndi",
        sub: "Tezliklə hazırlığa keçəcək.",
      };
    case "pending":
      return {
        icon: Clock3,
        tone: "warn",
        title: "Gözləmədə",
        sub: "Satıcı təsdiqləyənə qədər sifarişi ləğv edə bilərsiniz.",
      };
    case "cancelled":
      return {
        icon: Ban,
        tone: "bad",
        title: "Ləğv edilib",
        sub: "Bu sifariş ləğv olunub.",
      };
    case "return_requested":
      return {
        icon: Undo2,
        tone: "warn",
        title: "İadə sorğusu",
        sub: "Müştəri xidməti sorğunuzu emal edir.",
      };
    case "returned":
      return {
        icon: Undo2,
        tone: "ok",
        title: "İadə tamamlandı",
        sub: "İadə prosesi bitib.",
      };
    default:
      return {
        icon: Package,
        tone: "ok",
        title: orderStatusLabel(status),
        sub: "",
      };
  }
}

function ReturnModal({ orderId, onClose, onSubmit }: { orderId: string; onClose: () => void; onSubmit: (reason: string) => void }) {
  const [reason, setReason] = useState("");
  const REASONS = [
    "Məhsul zədəlidir",
    "Yanlış məhsul göndərilib",
    "Məhsul gözləntiləri qarşılamadı",
    "Sifarişi ləğv etmək istəyirəm",
    "Digər",
  ];
  const [custom, setCustom] = useState("");

  const handleSubmit = () => {
    const r = reason === "Digər" ? custom : reason;
    if (!r.trim()) return;
    onSubmit(r);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl p-6 w-full max-w-sm border" style={{ borderColor: BORDER }}>
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-bold text-gray-800">İadə / Şikayət sorğusu</h3>
          <button type="button" onClick={onClose} aria-label="Bağla">
            <X size={18} className="text-gray-400" />
          </button>
        </div>
        <p className="text-xs text-gray-500 mb-3">Sifariş №: …{orderId.slice(-8)}</p>
        <div className="space-y-2 mb-4">
          {REASONS.map((r) => (
            <label key={r} className="flex items-center gap-2.5 cursor-pointer p-2.5 rounded-xl hover:bg-gray-50">
              <input
                type="radio"
                name="reason"
                value={r}
                checked={reason === r}
                onChange={() => setReason(r)}
                style={{ accentColor: ACCENT }}
              />
              <span className="text-sm text-gray-700">{r}</span>
            </label>
          ))}
        </div>
        {reason === "Digər" && (
          <textarea
            value={custom}
            onChange={(e) => setCustom(e.target.value)}
            rows={3}
            placeholder="Səbəbi ətraflı izah edin..."
            className="w-full border rounded-xl px-3 py-2.5 text-sm outline-none resize-none mb-4"
            style={{ borderColor: BORDER }}
          />
        )}
        <div className="flex gap-2">
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!reason || (reason === "Digər" && !custom.trim())}
            className="flex-1 text-white py-2.5 rounded-xl text-sm font-semibold disabled:opacity-50"
            style={{ backgroundColor: ACCENT }}
          >
            Sorğu göndər
          </button>
          <button
            type="button"
            onClick={onClose}
            className="flex-1 border text-gray-600 py-2.5 rounded-xl text-sm font-semibold hover:bg-gray-50"
            style={{ borderColor: BORDER }}
          >
            Ləğv et
          </button>
        </div>
      </div>
    </div>
  );
}

export default function AccountOrders() {
  const { orders, ordersLoadError, refreshOrders, requestReturn, cancelOrder } = useOrders();
  const [category, setCategory] = useState<CategoryTab>("all");
  const [dateFilter, setDateFilter] = useState<DateFilter>("all");
  const [search, setSearch] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [returnOrderId, setReturnOrderId] = useState<string | null>(null);

  const list = useMemo(() => {
    const q = search.trim().toLowerCase();
    return orders.filter((o) => {
      if (!orderInCategory(o, category)) return false;
      if (!orderMatchesDateFilter(o, dateFilter)) return false;
      if (!q) return true;
      return o.items.some((it) => it.name.toLowerCase().includes(q));
    });
  }, [orders, category, dateFilter, search]);

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
      <div
        className="rounded-2xl border p-4 sm:p-6 space-y-5 -mx-1 sm:mx-0"
        style={{ backgroundColor: PAGE_BG, borderColor: BORDER }}
      >
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <h1 className="text-xl sm:text-2xl font-bold text-gray-900 tracking-tight">Sifarişlərim</h1>
            <p className="text-sm text-gray-500 mt-0.5">Bütün sifariş tarixçəniz və statuslar.</p>
          </div>
          <div className="relative w-full lg:max-w-md shrink-0">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} aria-hidden />
            <input
              type="search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Məhsul və ya brend adına görə axtar"
              className="w-full rounded-lg border bg-white py-2.5 pl-10 pr-3 text-sm outline-none focus:ring-2 focus:ring-lime-200 focus:border-[#65a30d]/40"
              style={{ borderColor: BORDER }}
            />
          </div>
        </div>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex gap-2 overflow-x-auto pb-1 -mx-1 px-1 scrollbar-hide">
            {CATEGORY_TABS.map((t) => {
              const active = category === t.id;
              return (
                <button
                  key={t.id}
                  type="button"
                  onClick={() => setCategory(t.id)}
                  className={`shrink-0 rounded-full border px-4 py-2 text-xs font-semibold transition-colors ${
                    active
                      ? "text-white shadow-sm"
                      : "bg-white text-gray-700 hover:bg-gray-50"
                  }`}
                  style={
                    active
                      ? { backgroundColor: ACCENT, borderColor: ACCENT }
                      : { borderColor: BORDER }
                  }
                >
                  {t.label}
                </button>
              );
            })}
          </div>
          <div className="relative shrink-0 sm:min-w-[160px]">
            <select
              value={dateFilter}
              onChange={(e) => setDateFilter(e.target.value as DateFilter)}
              className="w-full sm:w-auto appearance-none rounded-lg border bg-white py-2 pl-3 pr-9 text-sm font-medium text-gray-800 outline-none focus:ring-2 focus:ring-lime-200 focus:border-[#65a30d]/40"
              style={{ borderColor: BORDER }}
            >
              {DATE_FILTERS.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.label}
                </option>
              ))}
            </select>
            <ChevronDown
              className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 text-gray-400"
              size={16}
              aria-hidden
            />
          </div>
        </div>

        {ordersLoadError && (
          <div className="rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-900">
            <p className="font-medium">Sifariş siyahısı alınmadı</p>
            <p className="mt-1 text-amber-800/90">{ordersLoadError}</p>
            <button
              type="button"
              onClick={() => void refreshOrders()}
              className="mt-3 rounded-lg border border-amber-300 bg-white px-3 py-1.5 text-xs font-semibold text-amber-900 hover:bg-amber-100/80"
            >
              Yenidən yüklə
            </button>
          </div>
        )}

        {list.length === 0 ? (
          <div className="bg-white rounded-xl border p-12 text-center" style={{ borderColor: BORDER }}>
            <Package className="mx-auto text-gray-300 mb-3" size={40} />
            <p className="text-gray-600 font-medium">
              {ordersLoadError
                ? "Serverdən sifariş gəlmədi; yuxarıdakı xəbərdarlığa baxın."
                : "Bu filtrə uyğun sifariş yoxdur."}
            </p>
            <Link
              to="/mehsullar"
              className="mt-4 inline-block text-sm font-semibold hover:underline"
              style={{ color: ACCENT }}
            >
              Alış-verişə başla
            </Link>
          </div>
        ) : (
          <div className="space-y-4">
            {list.map((order) => {
              const isExpanded = expandedId === order.id;
              const canCancel = order.canBeCancelled ?? order.status === "pending";
              const canReturn = order.status === "delivered";
              const units = totalItemUnits(order);
              const headline = orderStatusHeadline(order.status);
              const HeadIcon = headline.icon;
              const firstSlug = order.items.find((i) => i.productSlug)?.productSlug;
              const showRate = order.status === "delivered" && !order.reviewLeft && Boolean(firstSlug);

              return (
                <article
                  key={order.id}
                  className="bg-white rounded-xl border overflow-hidden shadow-sm"
                  style={{ borderColor: BORDER }}
                >
                  <div
                    className="flex flex-wrap items-stretch gap-3 px-3 py-3 sm:px-4 border-b sm:items-center sm:justify-between"
                    style={{ borderColor: BORDER, backgroundColor: "#fafafa" }}
                  >
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-x-4 gap-y-2 text-sm flex-1 min-w-0">
                      <div>
                        <p className="text-[11px] uppercase tracking-wide text-gray-500">Sifariş tarixi</p>
                        <p className="font-semibold text-gray-900 mt-0.5">
                          {new Date(order.placedAt).toLocaleDateString("az-AZ", {
                            day: "numeric",
                            month: "long",
                            year: "numeric",
                          })}
                        </p>
                      </div>
                      <div>
                        <p className="text-[11px] uppercase tracking-wide text-gray-500">Sifariş xülasəsi</p>
                        <p className="font-semibold text-[#65a30d] mt-0.5">
                          1 çatdırılma, {units} məhsul
                        </p>
                      </div>
                      <div className="min-w-0 col-span-2 sm:col-span-1">
                        <p className="text-[11px] uppercase tracking-wide text-gray-500">Alıcı</p>
                        <p className="font-semibold text-gray-900 mt-0.5 truncate">{order.address.recipientName}</p>
                      </div>
                      <div>
                        <p className="text-[11px] uppercase tracking-wide text-gray-500">Cəm</p>
                        <p className="font-bold mt-0.5 text-base" style={{ color: ACCENT }}>
                          {formatMoney(order.totalPrice)}
                        </p>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => setExpandedId(isExpanded ? null : order.id)}
                      className="shrink-0 rounded-lg px-4 py-2 text-xs font-bold text-white self-center w-full sm:w-auto bg-[#65a30d] hover:bg-[#4d7c0f] transition-colors"
                    >
                      {isExpanded ? "Gizlət" : "Ətraflı"}
                    </button>
                  </div>

                  <div className="p-3 sm:p-4">
                    <div
                      className="flex flex-col gap-4 rounded-lg border p-3 sm:p-4 sm:flex-row sm:items-center sm:justify-between"
                      style={{ borderColor: BORDER }}
                    >
                      <div className="flex gap-3 min-w-0 flex-1">
                        <div
                          className={`mt-0.5 shrink-0 ${
                            headline.tone === "ok"
                              ? "text-[#65a30d]"
                              : headline.tone === "bad"
                                ? "text-red-600"
                                : "text-[#65a30d]"
                          }`}
                        >
                          <HeadIcon size={22} strokeWidth={2} />
                        </div>
                        <div className="min-w-0">
                          <p
                            className={`text-sm font-bold ${
                              headline.tone === "ok"
                                ? "text-[#65a30d]"
                                : headline.tone === "bad"
                                  ? "text-red-600"
                                  : "text-[#4d7c0f]"
                            }`}
                          >
                            {headline.title}
                          </p>
                          <p className="text-xs text-gray-500 mt-0.5">{headline.sub}</p>
                          <span
                            className={`inline-flex mt-2 text-[10px] font-semibold px-2 py-0.5 rounded-full border sm:hidden ${orderStatusTone(order.status)}`}
                          >
                            {orderStatusLabel(order.status)}
                          </span>
                        </div>
                      </div>

                      <div className="flex flex-wrap items-center gap-2">
                        <div className="flex -space-x-2 overflow-hidden py-0.5">
                          {order.items.slice(0, 5).map((item, i) => (
                            <ProductCoverImage
                              key={i}
                              src={item.image}
                              alt={item.name}
                              className="h-11 w-11 rounded-md border-2 border-white bg-gray-100 object-cover ring-1 ring-gray-200 sm:h-12 sm:w-12"
                            />
                          ))}
                        </div>
                        {order.items.length > 5 && (
                          <span className="text-xs font-semibold text-gray-500">+{order.items.length - 5}</span>
                        )}
                      </div>

                      <div className="flex flex-col gap-2 sm:items-end shrink-0">
                        <span
                          className={`hidden sm:inline-flex text-[10px] font-semibold px-2 py-0.5 rounded-full border ${orderStatusTone(order.status)}`}
                        >
                          {orderStatusLabel(order.status)}
                        </span>
                        {showRate && firstSlug && (
                          <Link
                            to={`/urun/${encodeURIComponent(firstSlug)}`}
                            className="inline-flex justify-center items-center rounded-lg border-2 px-3 py-2 text-xs font-bold transition-colors hover:bg-lime-50"
                            style={{ borderColor: ACCENT, color: ACCENT }}
                          >
                            <Star size={14} className="mr-1.5 shrink-0" aria-hidden />
                            Qiymətləndir
                          </Link>
                        )}
                      </div>
                    </div>

                    <OrderProgressBar status={order.status} />

                    <ul className="mt-3 divide-y rounded-lg border overflow-hidden bg-white" style={{ borderColor: BORDER }}>
                      {order.items.map((item, i) => (
                        <li key={i} className="px-3 py-2.5 flex flex-wrap items-center justify-between gap-2 sm:px-4">
                          <div className="flex items-center gap-3 min-w-0">
                            <ProductCoverImage
                              src={item.image}
                              alt={item.name}
                              className="h-10 w-10 shrink-0 rounded-md border bg-gray-50 object-cover"
                            />
                            <div className="min-w-0">
                              <p className="text-sm font-medium text-gray-900 line-clamp-1">{item.name}</p>
                              {item.variantLabel && <p className="text-xs text-gray-400">{item.variantLabel}</p>}
                              <p className="text-xs text-gray-500">
                                {item.quantity} ədəd × {formatMoney(item.price)}
                              </p>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            {item.productSlug ? (
                              <Link
                                to={`/urun/${encodeURIComponent(item.productSlug)}`}
                                className="text-xs font-semibold flex items-center gap-0.5 hover:underline shrink-0"
                                style={{ color: ACCENT }}
                              >
                                <ExternalLink size={12} />
                              </Link>
                            ) : (
                              <span className="text-xs text-gray-300 shrink-0">—</span>
                            )}
                          </div>
                        </li>
                      ))}
                    </ul>
                  </div>

                  {isExpanded && (
                    <div className="border-t px-3 py-4 space-y-3 text-sm text-gray-600 sm:px-4" style={{ borderColor: BORDER }}>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                        <div className="flex items-start gap-2">
                          <MapPin size={14} className="text-gray-400 mt-0.5 shrink-0" />
                          <div>
                            <p className="text-xs text-gray-400">Çatdırılma ünvanı</p>
                            <p className="font-medium text-gray-700">
                              {order.address.city}, {order.address.district}
                            </p>
                            <p className="text-xs">{order.address.line}</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-2">
                          <Clock size={14} className="text-gray-400 mt-0.5 shrink-0" />
                          <div>
                            <p className="text-xs text-gray-400">Çatdırılma</p>
                            <p className="font-medium text-gray-700">
                              {new Date(order.deliveryDate + "T00:00:00").toLocaleDateString("az-AZ", {
                                day: "numeric",
                                month: "long",
                              })}
                            </p>
                            <p className="text-xs">{deliveryTypeLabel(order.deliveryType)}</p>
                          </div>
                        </div>
                        <div className="flex items-start gap-2">
                          <CreditCard size={14} className="text-gray-400 mt-0.5 shrink-0" />
                          <div>
                            <p className="text-xs text-gray-400">Ödəniş</p>
                            <p className="font-medium text-gray-700">
                              {order.paymentMethod === "card"
                                ? "Kart"
                                : order.paymentMethod === "cash"
                                  ? "Nağd"
                                  : "Köçürmə"}
                            </p>
                            {order.cardLast4 && <p className="text-xs">•••• {order.cardLast4}</p>}
                          </div>
                        </div>
                      </div>

                      {(order.giftCardMessage || order.giftCardSenderName || order.giftCardOccasion) && (
                        <div className="rounded-lg border border-lime-100 bg-lime-50/50 px-3 py-3 space-y-2">
                          <p className="text-xs font-semibold text-[#3f6212] flex items-center gap-1.5">
                            <Gift size={14} aria-hidden />
                            Kart mesajı
                          </p>
                          {order.giftCardOccasion && (
                            <p className="text-xs text-gray-600">
                              <span className="text-gray-400">Məqsəd:</span> {order.giftCardOccasion}
                            </p>
                          )}
                          {order.giftCardSenderName && (
                            <p className="text-xs text-gray-600">
                              <span className="text-gray-400">Göndərən:</span> {order.giftCardSenderName}
                            </p>
                          )}
                          {order.giftCardMessage && (
                            <p className="text-sm text-gray-800 whitespace-pre-wrap">{order.giftCardMessage}</p>
                          )}
                        </div>
                      )}

                      {order.tracking.length > 0 && (
                        <div>
                          <p className="text-xs font-medium text-gray-600 mb-2">İzləmə tarixçəsi</p>
                          <div className="space-y-1.5">
                            {[...order.tracking].reverse().map((t, i) => (
                              <div key={i} className="flex items-start gap-2 text-xs">
                                <span
                                  className="w-1.5 h-1.5 rounded-full mt-1.5 shrink-0"
                                  style={{ backgroundColor: ACCENT }}
                                />
                                <div>
                                  <span className="font-medium text-gray-700">{orderStatusLabel(t.status)}</span>
                                  <span className="text-gray-400 ml-2">
                                    {new Date(t.timestamp).toLocaleString("az-AZ")}
                                  </span>
                                  {t.note && <p className="text-gray-500">{t.note}</p>}
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}

                      <div className="flex flex-wrap gap-2 pt-2 border-t" style={{ borderColor: BORDER }}>
                        {canCancel && (
                          <button
                            type="button"
                            onClick={() => cancelOrder(order.id)}
                            className="px-3 py-1.5 text-xs font-semibold text-red-600 border border-red-200 rounded-lg hover:bg-red-50"
                          >
                            Sifarişi ləğv et
                          </button>
                        )}
                        {canReturn && (
                          <button
                            type="button"
                            onClick={() => setReturnOrderId(order.id)}
                            className="flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold rounded-lg border border-lime-200 text-[#3f6212] hover:bg-lime-50"
                          >
                            <RotateCcw size={12} /> İadə / Şikayət
                          </button>
                        )}
                        {order.returnReason && (
                          <span className="px-3 py-1.5 text-xs text-[#3f6212] bg-lime-50 rounded-lg border border-lime-100">
                            İadə səbəbi: {order.returnReason}
                          </span>
                        )}
                      </div>
                    </div>
                  )}
                </article>
              );
            })}
          </div>
        )}
      </div>
    </>
  );
}
