import { useEffect, useState } from "react";
import { Link, Navigate, useLocation } from "react-router-dom";
import { CheckCircle2, Package, MapPin, Clock, CreditCard, Mail, MessageSquare, Printer } from "lucide-react";
import { formatMoney } from "../lib/format";
import { useOrders, orderStatusLabel, deliveryTypeLabel } from "../context/OrderContext";
import type { Order } from "../context/OrderContext";

function timeSlotLabel(value: string) {
  const map: Record<string, string> = {
    "09-12": "09:00–12:00",
    "12-15": "12:00–15:00",
    "15-18": "15:00–18:00",
    "18-21": "18:00–21:00",
  };
  return map[value] ?? value;
}

function formatDateDisplay(isoDate: string) {
  const [y, m, d] = isoDate.split("-").map(Number);
  if (!y || !m || !d) return isoDate;
  return new Date(y, m - 1, d).toLocaleDateString("az-AZ", { day: "numeric", month: "long", year: "numeric" });
}

function paymentLabel(m: string) {
  const map: Record<string, string> = {
    card: "Bank kartı (3D Secure)",
    cash: "Nağd (çatdırılma zamanı)",
    transfer: "Bank köçürməsi",
  };
  return map[m] ?? m;
}

interface SimNotifProps {
  type: "email" | "sms";
  to: string;
  orderId: string;
}

function SimNotif({ type, to, orderId }: SimNotifProps) {
  const [shown, setShown] = useState(true);
  if (!shown) return null;
  return (
    <div className="flex items-start gap-3 bg-blue-50 border border-blue-200 rounded-xl px-4 py-3 text-sm text-blue-700">
      {type === "email" ? <Mail size={16} className="mt-0.5 shrink-0" /> : <MessageSquare size={16} className="mt-0.5 shrink-0" />}
      <div className="flex-1">
        <span className="font-medium">{type === "email" ? "E-poçt təsdiqi" : "SMS bildirişi"}</span> {to}-a göndərildi
        <span className="block text-xs text-blue-500 mt-0.5">Sifariş №: {orderId} (Demo: real göndərilmir)</span>
      </div>
      <button onClick={() => setShown(false)} className="text-blue-400 hover:text-blue-600 text-lg leading-none">&times;</button>
    </div>
  );
}

export default function OrderConfirmationPage() {
  const location = useLocation();
  const { getOrder } = useOrders();
  const orderId = (location.state as { orderId?: string } | null)?.orderId;
  const order: Order | undefined = orderId ? getOrder(orderId) : undefined;

  useEffect(() => {
    window.scrollTo({ top: 0 });
  }, []);

  if (!orderId || !order) {
    return <Navigate to="/" replace />;
  }

  const handlePrint = () => window.print();

  return (
    <div className="max-w-2xl mx-auto px-4 py-12">
      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-green-100 text-green-500 mb-4 animate-pulse">
          <CheckCircle2 size={44} strokeWidth={1.5} />
        </div>
        <h1 className="text-3xl font-bold text-gray-800 mb-2">Sifariş qəbul olundu!</h1>
        <p className="text-gray-500 text-sm mb-4">
          Sifariş nömrənizi qeyd edin. Tezliklə sizinlə əlaqə saxlayacağıq.
        </p>
        <div className="inline-block bg-lime-50 border border-lime-200 rounded-2xl px-6 py-3">
          <p className="text-xs text-gray-500 mb-1">Sifariş nömrəsi</p>
          <p className="text-2xl font-mono font-bold text-[#65a30d] tracking-wider">{order.id}</p>
        </div>
      </div>

      <div className="space-y-3 mb-6">
        {order.address.recipientPhone && (
          <SimNotif type="sms" to={order.address.recipientPhone} orderId={order.id} />
        )}
        {order.guestEmail && (
          <SimNotif type="email" to={order.guestEmail} orderId={order.id} />
        )}
      </div>

      {/* Status timeline */}
      <div className="bg-white rounded-2xl border border-gray-100 p-5 mb-5">
        <h2 className="font-semibold text-gray-800 text-sm mb-4">Sifariş statusu</h2>
        <div className="flex items-center gap-0">
          {(["confirmed", "preparing", "shipped", "delivered"] as const).map((s, i, arr) => {
            const done = ["confirmed", "preparing", "shipped", "delivered"].indexOf(order.status) >= i;
            return (
              <div key={s} className="flex items-center flex-1">
                <div className="flex flex-col items-center">
                  <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold ${done ? "bg-[#65a30d] text-white" : "bg-gray-100 text-gray-400"}`}>
                    {done ? "✓" : i + 1}
                  </div>
                  <span className="text-[10px] text-center mt-1 text-gray-500 leading-tight whitespace-nowrap">{orderStatusLabel(s)}</span>
                </div>
                {i < arr.length - 1 && (
                  <div className={`flex-1 h-0.5 mx-1 ${done ? "bg-[#65a30d]" : "bg-gray-100"}`} />
                )}
              </div>
            );
          })}
        </div>
      </div>

      {/* Details */}
      <div className="bg-white rounded-2xl border border-gray-100 p-5 space-y-3 text-sm mb-5">
        <h2 className="font-semibold text-gray-800 flex items-center gap-2 mb-2">
          <Package size={18} className="text-[#65a30d]" /> Çatdırılma məlumatı
        </h2>
        <div className="grid grid-cols-2 gap-3 text-gray-600">
          <div className="flex items-start gap-2">
            <MapPin size={14} className="text-gray-400 mt-0.5 shrink-0" />
            <div>
              <p className="text-xs text-gray-400 mb-0.5">Ünvan</p>
              <p className="font-medium text-gray-800">{order.address.city}, {order.address.district}</p>
              <p className="text-xs text-gray-500">{order.address.line}</p>
            </div>
          </div>
          <div className="flex items-start gap-2">
            <Clock size={14} className="text-gray-400 mt-0.5 shrink-0" />
            <div>
              <p className="text-xs text-gray-400 mb-0.5">Çatdırılma</p>
              <p className="font-medium text-gray-800">{formatDateDisplay(order.deliveryDate)}</p>
              <p className="text-xs text-gray-500">{timeSlotLabel(order.deliveryTimeSlot)}</p>
            </div>
          </div>
          <div className="flex items-start gap-2">
            <CreditCard size={14} className="text-gray-400 mt-0.5 shrink-0" />
            <div>
              <p className="text-xs text-gray-400 mb-0.5">Ödəniş</p>
              <p className="font-medium text-gray-800">{paymentLabel(order.paymentMethod)}</p>
              {order.cardLast4 && <p className="text-xs text-gray-500">•••• {order.cardLast4}</p>}
            </div>
          </div>
          <div>
            <p className="text-xs text-gray-400 mb-0.5">Çatdırılma növü</p>
            <p className="font-medium text-gray-800">{deliveryTypeLabel(order.deliveryType)}</p>
          </div>
        </div>
        {order.notes && (
          <div className="bg-gray-50 rounded-xl p-3 text-xs text-gray-600">
            <span className="font-medium text-gray-700">Qeyd: </span>{order.notes}
          </div>
        )}
      </div>

      {/* Items */}
      <div className="bg-white rounded-2xl border border-gray-100 p-5 mb-8">
        <h3 className="font-semibold text-gray-800 mb-3 text-sm">Məhsullar</h3>
        <ul className="space-y-2 text-sm">
          {order.items.map((item, i) => (
            <li key={i} className="flex justify-between text-gray-600">
              <span className="line-clamp-1 pr-2">
                {item.name}
                {item.variantLabel && <span className="text-gray-400"> ({item.variantLabel})</span>}
                {" "}&times; {item.quantity}
              </span>
              <span className="shrink-0">{formatMoney(item.price * item.quantity)}</span>
            </li>
          ))}
        </ul>
        <div className="border-t border-gray-100 mt-3 pt-3 space-y-1.5 text-sm">
          <div className="flex justify-between text-gray-600">
            <span>Məhsul cəmi</span>
            <span>{formatMoney(order.subtotal)}</span>
          </div>
          {order.deliveryFee > 0 && (
            <div className="flex justify-between text-gray-600">
              <span>Çatdırılma</span>
              <span>{formatMoney(order.deliveryFee)}</span>
            </div>
          )}
          {order.discount > 0 && (
            <div className="flex justify-between text-green-600">
              <span>Endirim</span>
              <span>−{formatMoney(order.discount)}</span>
            </div>
          )}
          {order.installment && order.installment > 1 && (
            <div className="text-xs text-gray-500 text-center">{order.installment} × {formatMoney(order.totalPrice / order.installment)}/ay</div>
          )}
          <div className="flex justify-between font-bold text-gray-900 text-base pt-1 border-t border-gray-100">
            <span>Cəm</span>
            <span>{formatMoney(order.totalPrice)}</span>
          </div>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-3 justify-center">
        <Link to="/" className="inline-flex justify-center items-center bg-[#65a30d] text-white px-6 py-3 rounded-xl font-semibold text-sm hover:bg-[#4d7c0f] transition-colors">
          Ana səhifə
        </Link>
        <Link to="/hesabim/sifarisler" className="inline-flex justify-center items-center border border-gray-200 text-gray-700 px-6 py-3 rounded-xl font-semibold text-sm hover:border-lime-300 transition-colors">
          Sifarişlərim
        </Link>
        <Link to="/mehsullar" className="inline-flex justify-center items-center border border-gray-200 text-gray-700 px-6 py-3 rounded-xl font-semibold text-sm hover:border-lime-300 transition-colors">
          Alış-verişə davam et
        </Link>
        <button onClick={handlePrint} className="inline-flex justify-center items-center gap-2 border border-gray-200 text-gray-700 px-6 py-3 rounded-xl font-semibold text-sm hover:border-lime-300 transition-colors">
          <Printer size={16} /> Çap et
        </button>
      </div>
    </div>
  );
}
