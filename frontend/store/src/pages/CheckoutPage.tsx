import { useState, useMemo, useEffect, startTransition, type FormEvent } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import {
  ArrowLeft, Calendar, Clock, CreditCard, MapPin, User, Wallet,
  Building2, FileText, Truck, Tag, X, CheckCircle, Loader2,
  Shield, ChevronDown, ChevronUp, Zap, Package, Store, Gift,
} from "lucide-react";
import { useCart } from "../hooks/useCart";
import { useAuth } from "../hooks/useAuth";
import { useAddresses } from "../hooks/useAddresses";
import { useToast } from "../hooks/useToast";
import { useOrders } from "../hooks/useOrders";
import { cartItemsToOrderItems } from "../models/order";
import type { DeliveryType, PaymentMethod } from "../models/order";
import { formatMoney } from "../lib/format";
import { computeCartTotals, applyCoupon } from "../lib/cartPricing";
import { cities, DELIVERY_TYPES, INSTALLMENT_OPTIONS } from "../data/cities";
import {
  mapFrontendDeliveryToApi,
  mapFrontendPaymentToApi,
} from "../lib/catalogMap";
import { orderApi, userApi, couponApi, ApiError } from "../services/api";

const UUID_RE =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function normAddrPart(s: string) {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

function phoneDigits(s: string) {
  return s.replace(/\D/g, "");
}

/** Əvvəlcədən saxlanmış ünvanla eynidirsə API-yə təkrar INSERT göndərməmək (5 ünvan limiti). */
function findSavedAddressId(
  addresses: { id: string; recipientName: string; recipientPhone: string; city: string; district: string; line: string; postalCode?: string }[],
  recipientName: string,
  recipientPhone: string,
  city: string,
  district: string,
  line: string,
  postalCode: string
): string | null {
  const pn = phoneDigits(recipientPhone);
  const row = addresses.find(
    (a) =>
      normAddrPart(a.recipientName) === normAddrPart(recipientName) &&
      phoneDigits(a.recipientPhone) === pn &&
      normAddrPart(a.city) === normAddrPart(city) &&
      normAddrPart(a.district) === normAddrPart(district) &&
      normAddrPart(a.line) === normAddrPart(line) &&
      normAddrPart(a.postalCode ?? "") === normAddrPart(postalCode)
  );
  return row && UUID_RE.test(row.id) ? row.id : null;
}

const CARD_MESSAGE_TEMPLATES: { value: string; label: string; text: string }[] = [
  { value: "", label: "Hazır mesaj seçin (istəyə bağlı)", text: "" },
  { value: "love", label: "Sevgi / romantika", text: "Səni sonsuz sevirəm. Bu çiçəklər ürəyimdən." },
  { value: "thanks", label: "Təşəkkür", text: "Hər şeyə görə təşəkkür edirəm. Sən xüsusisən." },
  { value: "congrats", label: "Təbrik", text: "Səmimi təbriklərimi qəbul et!" },
  { value: "sorry", label: "Üzr", text: "Bağışla, qəlbimdən üzr istəyirəm." },
  { value: "birthday", label: "Ad günü təbriki", text: "Ad günün mübarək! Sağlamlıq və xoşbəxtlik arzulayıram." },
];

const OCCASION_OPTIONS: { value: string; label: string }[] = [
  { value: "", label: "Göndərmə məqsədi (istəyə bağlı)" },
  { value: "Ad günü", label: "Ad günü" },
  { value: "Yubiley / ildönümü", label: "Yubiley / ildönümü" },
  { value: "Sevgililər günü / romantika", label: "Sevgililər günü / romantika" },
  { value: "Analar günü", label: "Analar günü" },
  { value: "Təbrik", label: "Təbrik" },
  { value: "Təşəkkür", label: "Təşəkkür" },
  { value: "Başsağlığı", label: "Başsağlığı" },
  { value: "Digər", label: "Digər" },
];

const TIME_SLOTS = [
  { value: "09-12", label: "09:00–12:00" },
  { value: "12-15", label: "12:00–15:00" },
  { value: "15-18", label: "15:00–18:00" },
  { value: "18-21", label: "18:00–21:00" },
] as const;

function formatLocalDate(d: Date) {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

function addDays(d: Date, n: number) {
  const x = new Date(d);
  x.setDate(x.getDate() + n);
  return x;
}

interface ThreeDSModalProps {
  amount: string;
  onSuccess: () => void;
  onCancel: () => void;
}

function ThreeDSModal({ amount, onSuccess, onCancel }: ThreeDSModalProps) {
  const [step, setStep] = useState<"form" | "otp" | "success">("form");
  const [code, setCode] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSend = async () => {
    setLoading(true);
    await new Promise((r) => setTimeout(r, 800));
    setLoading(false);
    setStep("otp");
  };

  const handleVerify = async () => {
    if (code.length < 4) { setError("Kodu tam daxil edin."); return; }
    setLoading(true);
    await new Promise((r) => setTimeout(r, 1000));
    setLoading(false);
    setStep("success");
    setTimeout(onSuccess, 1500);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-sm">
        <div className="flex items-center gap-2 mb-4">
          <Shield size={20} className="text-[#65a30d]" />
          <h3 className="font-bold text-gray-800">3D Secure Doğrulama</h3>
        </div>
        {step === "success" ? (
          <div className="text-center py-6">
            <CheckCircle size={52} className="text-green-500 mx-auto mb-3" />
            <p className="font-semibold text-gray-800">Ödəniş təsdiqləndi!</p>
            <p className="text-sm text-gray-500 mt-1">{amount} uğurla ödənildi</p>
          </div>
        ) : step === "form" ? (
          <>
            <div className="bg-gray-50 rounded-xl p-3 mb-4 text-center">
              <p className="text-xs text-gray-500 mb-1">Ödəniş məbləği</p>
              <p className="text-2xl font-bold text-gray-900">{amount}</p>
            </div>
            <p className="text-sm text-gray-500 mb-4">
              Bankınızın 3D Secure sistemi vasitəsilə ödənişi təsdiqləyin.
              Telefon nömrənizə SMS kodu göndəriləcək.
            </p>
            <p className="text-xs text-amber-600 bg-amber-50 rounded-xl p-3 mb-4">
              Demo rejimi: istənilən 4-6 rəqəmli kod qəbul ediləcək.
            </p>
            <button
              onClick={handleSend}
              disabled={loading}
              className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 flex items-center justify-center gap-2"
            >
              {loading && <Loader2 size={16} className="animate-spin" />}
              SMS Kodu Al
            </button>
            <button onClick={onCancel} className="mt-2 w-full text-sm text-gray-400 hover:text-gray-600">Ləğv et</button>
          </>
        ) : (
          <>
            <p className="text-sm text-gray-600 mb-4">Bankınızdan gələn SMS kodunu daxil edin:</p>
            <input
              type="text"
              maxLength={6}
              value={code}
              onChange={(e) => setCode(e.target.value.replace(/\D/g, ""))}
              placeholder="123456"
              className="w-full border border-gray-200 rounded-xl px-3 py-3 text-center text-xl font-bold tracking-widest outline-none focus:border-[#65a30d] mb-3"
            />
            {error && <p className="text-xs text-red-500 mb-2">{error}</p>}
            <button
              onClick={handleVerify}
              disabled={loading}
              className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 flex items-center justify-center gap-2"
            >
              {loading && <Loader2 size={16} className="animate-spin" />}
              Ödənişi Təsdiqlə
            </button>
            <button onClick={onCancel} className="mt-2 w-full text-sm text-gray-400 hover:text-gray-600">Ləğv et</button>
          </>
        )}
      </div>
    </div>
  );
}

const DELIVERY_ICONS: Record<DeliveryType, typeof Zap> = {
  same_day: Zap,
  courier: Truck,
  cargo: Package,
  pickup: Store,
};

export default function CheckoutPage() {
  const { items, totalPrice, clearCart, couponCode: cartCoupon, couponDiscount: cartCouponDiscount } = useCart();
  const { user } = useAuth();
  const { addresses } = useAddresses();
  const { addOrder } = useOrders();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const today = useMemo(() => { const d = new Date(); d.setHours(0, 0, 0, 0); return d; }, []);
  const minDate = formatLocalDate(today);
  const maxDate = formatLocalDate(addDays(today, 21));

  const [recipientName, setRecipientName] = useState(user ? `${user.firstName} ${user.lastName}` : "");
  const [recipientPhone, setRecipientPhone] = useState(user?.phone ?? "");

  const [city, setCity] = useState("Bakı");
  const [district, setDistrict] = useState("");
  const [addressLine, setAddressLine] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [selectedAddressId, setSelectedAddressId] = useState<string>("");
  const [showAddressBook, setShowAddressBook] = useState(false);

  const [deliveryType, setDeliveryType] = useState<DeliveryType>("courier");
  const [deliveryDate, setDeliveryDate] = useState(minDate);
  const [deliveryTimeSlot, setDeliveryTimeSlot] = useState<string>(TIME_SLOTS[0].value);
  const [notes, setNotes] = useState("");

  const [cardTemplate, setCardTemplate] = useState("");
  const [cardMessage, setCardMessage] = useState("");
  const [cardSenderName, setCardSenderName] = useState("");
  const [cardOccasion, setCardOccasion] = useState("");

  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("card");
  const [cardNumber, setCardNumber] = useState("");
  const [cardHolder, setCardHolder] = useState("");
  const [cardExpiry, setCardExpiry] = useState("");
  const [cardCvv, setCardCvv] = useState("");
  const [installmentMonths, setInstallmentMonths] = useState(1);
  const [showInstallment, setShowInstallment] = useState(false);

  const [couponInput, setCouponInput] = useState("");
  const [appliedCoupon, setAppliedCoupon] = useState(cartCoupon);
  const [appliedCouponDiscount, setAppliedCouponDiscount] = useState(cartCouponDiscount);
  const [appliedCouponLabel, setAppliedCouponLabel] = useState("");
  const [couponError, setCouponError] = useState("");

  const [invoiceRequested, setInvoiceRequested] = useState(false);
  const [invoiceCompanyName, setInvoiceCompanyName] = useState("");
  const [invoiceTaxId, setInvoiceTaxId] = useState("");

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [show3DS, setShow3DS] = useState(false);
  const [pendingOrder, setPendingOrder] = useState<Parameters<typeof addOrder>[0] | null>(null);

  const cityObj = useMemo(() => cities.find((c) => c.name === city), [city]);
  const districts = useMemo(() => cityObj?.districts ?? [], [cityObj]);

  useEffect(() => {
    startTransition(() => {
      if (districts.length > 0 && !districts.find((d) => d.name === district)) {
        setDistrict(districts[0].name);
      }
    });
  }, [city, districts, district]);

  const districtInfo = useMemo(() => districts.find((d) => d.name === district), [districts, district]);

  const sameDayAvailable = districtInfo?.sameDayAvailable ?? false;

  useEffect(() => {
    startTransition(() => {
      if (deliveryType === "same_day" && !sameDayAvailable) {
        setDeliveryType("courier");
      }
    });
  }, [sameDayAvailable, deliveryType]);

  useEffect(() => {
    startTransition(() => {
      if (!cardTemplate) return;
      const row = CARD_MESSAGE_TEMPLATES.find((t) => t.value === cardTemplate);
      if (row?.text) setCardMessage(row.text);
    });
  }, [cardTemplate]);

  const totals = useMemo(() => computeCartTotals(totalPrice, {
    city, district, deliveryType,
    couponCode: appliedCoupon || undefined,
    installmentMonths,
  }), [totalPrice, city, district, deliveryType, appliedCoupon, installmentMonths]);

  const handleSelectAddressBook = (id: string) => {
    const addr = addresses.find((a) => a.id === id);
    if (!addr) return;
    setSelectedAddressId(id);
    setRecipientName(addr.recipientName);
    setRecipientPhone(addr.recipientPhone);
    setCity(addr.city);
    setDistrict(addr.district);
    setAddressLine(addr.line);
    setPostalCode(addr.postalCode ?? "");
    setShowAddressBook(false);
  };

  const handleApplyCoupon = async () => {
    setCouponError("");
    const code = couponInput.trim();
    if (!code) {
      setCouponError("Kupon kodu daxil edin.");
      return;
    }
    if (user) {
      try {
        const r = await couponApi.validate(code.toUpperCase(), totalPrice);
        if (r.isValid && r.discountAmount > 0) {
          setAppliedCoupon(r.code);
          setAppliedCouponDiscount(Number(r.discountAmount));
          setAppliedCouponLabel(r.message ?? r.code);
          setCouponInput("");
          return;
        }
        setCouponError(r.message ?? "Kupon etibarlı deyil.");
        return;
      } catch (e) {
        if (e instanceof ApiError && e.status === 401) {
          setCouponError("Kupon təsdiqi üçün hesaba daxil olmalısınız.");
          return;
        }
      }
    }
    const result = applyCoupon(couponInput, totalPrice);
    if (result.ok && result.discountAmount !== undefined && result.label) {
      setAppliedCoupon(couponInput.toUpperCase().trim());
      setAppliedCouponDiscount(result.discountAmount);
      setAppliedCouponLabel(result.label);
      setCouponInput("");
    } else {
      setCouponError(result.error ?? "Xəta");
    }
  };

  const handleRemoveCoupon = () => {
    setAppliedCoupon("");
    setAppliedCouponDiscount(0);
    setAppliedCouponLabel("");
  };

  const formatCard = (v: string) => v.replace(/\D/g, "").slice(0, 16).replace(/(.{4})/g, "$1 ").trim();
  const formatExpiry = (v: string) => {
    const d = v.replace(/\D/g, "").slice(0, 4);
    return d.length > 2 ? d.slice(0, 2) + "/" + d.slice(2) : d;
  };

  const validate = () => {
    const e: Record<string, string> = {};
    if (recipientName.trim().length < 2) e.recipientName = "Ad, soyad tələb olunur.";
    if (recipientPhone.replace(/\D/g, "").length < 9) e.recipientPhone = "Telefon nömrəsi tələb olunur.";
    if (!city) e.city = "Şəhər seçin.";
    if (!district) e.district = "Rayon seçin.";
    if (addressLine.trim().length < 5) e.addressLine = "Tam ünvanı daxil edin.";
    if (!deliveryDate) e.deliveryDate = "Tarix seçin.";
    if (paymentMethod === "card") {
      if (cardNumber.replace(/\D/g, "").length !== 16) e.cardNumber = "Kart nömrəsi 16 rəqəm olmalıdır.";
      if (!cardHolder.trim()) e.cardHolder = "Kart sahibinin adı tələb olunur.";
      if (cardExpiry.length < 5) e.cardExpiry = "Son istifadə tarixi tələb olunur.";
      if (cardCvv.length < 3) e.cardCvv = "CVV kodu tələb olunur.";
    }
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  if (items.length === 0) return <Navigate to="/sepet" replace />;

  const buildOrder = (): Parameters<typeof addOrder>[0] => {
    const orderNumber = `GL-${new Date().getFullYear()}-${String(Math.floor(Math.random() * 9000 + 1000))}`;
    return {
      id: orderNumber,
      userId: user?.id,
      placedAt: new Date().toISOString(),
      status: "confirmed",
      items: cartItemsToOrderItems(items),
      address: { recipientName: recipientName.trim(), recipientPhone: recipientPhone.trim(), city, district, line: addressLine.trim(), postalCode: postalCode.trim() },
      deliveryType,
      deliveryDate,
      deliveryTimeSlot,
      notes: notes.trim(),
      giftCardMessage: cardMessage.trim() || undefined,
      giftCardSenderName: cardSenderName.trim() || undefined,
      giftCardOccasion: cardOccasion.trim() || undefined,
      paymentMethod,
      cardLast4: paymentMethod === "card" ? cardNumber.replace(/\D/g, "").slice(-4) : undefined,
      installment: installmentMonths > 1 ? installmentMonths : undefined,
      couponCode: appliedCoupon || undefined,
      couponDiscount: appliedCouponDiscount,
      subtotal: totalPrice,
      deliveryFee: totals.shipping,
      discount: totals.discount,
      totalPrice: totals.finalPrice,
      invoiceRequested,
      invoiceCompanyName: invoiceRequested ? invoiceCompanyName : undefined,
      invoiceTaxId: invoiceRequested ? invoiceTaxId : undefined,
      tracking: [{ status: "confirmed", timestamp: new Date().toISOString(), note: "Sifarişiniz qəbul edildi." }],
    };
  };

  const handleSubmit = (ev: FormEvent) => {
    ev.preventDefault();
    if (!validate()) return;
    const order = buildOrder();
    if (paymentMethod === "card") {
      setPendingOrder(order);
      setShow3DS(true);
    } else {
      void finalize(order);
    }
  };

  const finalize = async (order: Parameters<typeof addOrder>[0]) => {
    setSubmitting(true);
    try {
      if (user) {
        let addressId = selectedAddressId;
        if (!UUID_RE.test(addressId)) {
          const reused = findSavedAddressId(
            addresses,
            recipientName,
            recipientPhone,
            city,
            district,
            addressLine,
            postalCode
          );
          if (reused) {
            addressId = reused;
          } else {
            addressId = await userApi.addAddress({
              title: "Çatdırılma",
              recipientName: recipientName.trim(),
              phoneNumber: recipientPhone.trim(),
              city,
              district,
              fullLine: addressLine.trim(),
              postalCode: postalCode.trim() || null,
              isDefault: false,
            });
          }
        }
        const coupon =
          appliedCoupon.trim() !== "" ? appliedCoupon.trim() : null;
        const newId = await orderApi.create({
          addressId,
          deliveryType: mapFrontendDeliveryToApi(deliveryType),
          paymentMethod: mapFrontendPaymentToApi(paymentMethod),
          items: items.map((i) => ({
            productId: i.product.id,
            quantity: i.quantity,
          })),
          couponCode: coupon,
          giftCardMessage: cardMessage.trim() || null,
          giftCardSenderName: cardSenderName.trim() || null,
          giftCardOccasion: cardOccasion.trim() || null,
        });
        const full = await orderApi.getById(newId);
        addOrder(full);
        clearCart();
        navigate("/sifaris-tesdiq", {
          replace: true,
          state: { orderId: full.id },
        });
        return;
      }
      await new Promise((r) => setTimeout(r, 500));
      addOrder(order);
      clearCart();
      navigate("/sifaris-tesdiq", {
        replace: true,
        state: { orderId: order.id },
      });
    } catch (e) {
      const msg = e instanceof ApiError ? e.message : "Sifariş göndərilmədi.";
      showToast(msg, "error");
    } finally {
      setSubmitting(false);
    }
  };

  const PAYMENT_OPTIONS: { value: PaymentMethod; title: string; description: string; icon: typeof CreditCard }[] = [
    { value: "card", title: "Bank kartı", description: "Visa, Mastercard, Troy — 3D Secure", icon: CreditCard },
    { value: "cash", title: "Nağd", description: "Çatdırılma zamanı nağd ödəniş", icon: Wallet },
    { value: "transfer", title: "Bank köçürməsi", description: "Hesaba köçürmə ilə ödəniş", icon: Building2 },
  ];

  return (
    <>
      {show3DS && pendingOrder && (
        <ThreeDSModal
          amount={formatMoney(totals.finalPrice)}
          onSuccess={() => {
            setShow3DS(false);
            void finalize(pendingOrder);
          }}
          onCancel={() => { setShow3DS(false); setSubmitting(false); }}
        />
      )}

      <div className="max-w-7xl mx-auto px-4 py-8">
        <Link to="/sepet" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-[#65a30d] mb-6 transition-colors">
          <ArrowLeft size={16} /> Səbətə qayıt
        </Link>
        <h1 className="text-2xl font-bold text-gray-800 mb-2">Ödəniş və çatdırılma</h1>
        <p className="text-sm text-gray-500 mb-8">Alıcı və çatdırılma məlumatlarını doldurun.</p>

        <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 space-y-6">

            {/* Recipient */}
            <section className="bg-white rounded-2xl border border-gray-100 p-5">
              <div className="flex items-center gap-2 mb-4">
                <User size={18} className="text-[#65a30d]" />
                <h2 className="font-semibold text-gray-800">Alıcı məlumatı</h2>
                {user && (
                  <span className="ml-auto text-xs text-green-600 bg-green-50 border border-green-200 px-2 py-0.5 rounded-full">Hesabdan dolduruldu</span>
                )}
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="sm:col-span-2">
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Ad, soyad <span className="text-red-500">*</span></label>
                  <input type="text" value={recipientName} onChange={(e) => setRecipientName(e.target.value)} autoComplete="name"
                    className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.recipientName ? "border-red-300" : "border-gray-200"}`}
                    placeholder="Məs. Aygün Məmmədova" />
                  {errors.recipientName && <p className="text-xs text-red-500 mt-1">{errors.recipientName}</p>}
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Telefon <span className="text-red-500">*</span></label>
                  <input type="tel" value={recipientPhone} onChange={(e) => setRecipientPhone(e.target.value)} autoComplete="tel"
                    className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.recipientPhone ? "border-red-300" : "border-gray-200"}`}
                    placeholder="+994 50 123 45 67" />
                  {errors.recipientPhone && <p className="text-xs text-red-500 mt-1">{errors.recipientPhone}</p>}
                </div>
              </div>
            </section>

            {/* Delivery */}
            <section className="bg-white rounded-2xl border border-gray-100 p-5">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                  <MapPin size={18} className="text-[#65a30d]" />
                  <h2 className="font-semibold text-gray-800">Çatdırılma ünvanı</h2>
                </div>
                {addresses.length > 0 && (
                  <button type="button" onClick={() => setShowAddressBook(!showAddressBook)}
                    className="text-xs text-[#65a30d] font-medium flex items-center gap-1 hover:underline">
                    Ünvan kitabçası {showAddressBook ? <ChevronUp size={12} /> : <ChevronDown size={12} />}
                  </button>
                )}
              </div>

              {showAddressBook && addresses.length > 0 && (
                <div className="mb-4 space-y-2">
                  {addresses.map((addr) => (
                    <button key={addr.id} type="button" onClick={() => handleSelectAddressBook(addr.id)}
                      className={`w-full text-left p-3 rounded-xl border text-sm transition-colors ${selectedAddressId === addr.id ? "border-[#65a30d] bg-lime-50/40" : "border-gray-200 hover:border-lime-200"}`}>
                      <div className="font-medium text-gray-800">{addr.title} — {addr.recipientName}</div>
                      <div className="text-gray-500 text-xs mt-0.5">{addr.city}, {addr.district}, {addr.line}</div>
                    </button>
                  ))}
                  <div className="border-t border-gray-100 pt-2">
                    <button type="button" onClick={() => setShowAddressBook(false)} className="text-xs text-gray-400 hover:text-gray-600">Yeni ünvan daxil et</button>
                  </div>
                </div>
              )}

              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Şəhər <span className="text-red-500">*</span></label>
                    <select value={city} onChange={(e) => setCity(e.target.value)}
                      className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white ${errors.city ? "border-red-300" : "border-gray-200"}`}>
                      {cities.map((c) => <option key={c.name} value={c.name}>{c.name}</option>)}
                    </select>
                    {errors.city && <p className="text-xs text-red-500 mt-1">{errors.city}</p>}
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Rayon <span className="text-red-500">*</span></label>
                    <select value={district} onChange={(e) => setDistrict(e.target.value)}
                      className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white ${errors.district ? "border-red-300" : "border-gray-200"}`}>
                      {districts.map((d) => <option key={d.name} value={d.name}>{d.name}</option>)}
                    </select>
                    {errors.district && <p className="text-xs text-red-500 mt-1">{errors.district}</p>}
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Küçə, bina, mənzil <span className="text-red-500">*</span></label>
                  <textarea value={addressLine} onChange={(e) => setAddressLine(e.target.value)} rows={2}
                    className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-none ${errors.addressLine ? "border-red-300" : "border-gray-200"}`}
                    placeholder="Məs. Nizami küç. 203, m.4, mənzil 12" />
                  {errors.addressLine && <p className="text-xs text-red-500 mt-1">{errors.addressLine}</p>}
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Poçt indeksi</label>
                    <input type="text" value={postalCode} onChange={(e) => setPostalCode(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                      placeholder="AZ 1000" />
                  </div>
                </div>
              </div>
            </section>

            {/* Delivery type */}
            <section className="bg-white rounded-2xl border border-gray-100 p-5">
              <div className="flex items-center gap-2 mb-4">
                <Truck size={18} className="text-[#65a30d]" />
                <h2 className="font-semibold text-gray-800">Çatdırılma növü</h2>
              </div>
              <div className="grid grid-cols-2 gap-3 mb-4">
                {DELIVERY_TYPES.map(({ value, label, description, extraFee, requiresSameDay }) => {
                  const Icon = DELIVERY_ICONS[value];
                  const disabled = requiresSameDay && !sameDayAvailable;
                  return (
                    <label key={value}
                      className={`flex flex-col gap-1.5 p-3 rounded-xl border cursor-pointer transition-colors ${disabled ? "opacity-40 cursor-not-allowed" : ""} ${deliveryType === value ? "border-[#65a30d] bg-lime-50/50" : "border-gray-200 hover:border-lime-200"}`}>
                      <input type="radio" name="deliveryType" value={value} checked={deliveryType === value}
                        onChange={() => !disabled && setDeliveryType(value)} disabled={disabled} className="sr-only" />
                      <div className="flex items-center gap-2">
                        <Icon size={16} className={deliveryType === value ? "text-[#65a30d]" : "text-gray-500"} />
                        <span className="text-sm font-semibold text-gray-800">{label}</span>
                        {extraFee < 0 && <span className="ml-auto text-xs text-green-600 font-medium">+{formatMoney(-extraFee)} qənaət</span>}
                        {extraFee === 0 && value !== "cargo" && <span className="ml-auto text-xs text-green-600 font-medium">Pulsuz</span>}
                      </div>
                      <p className="text-xs text-gray-500">{description}</p>
                      {disabled && <p className="text-xs text-amber-600">Bu rayona mövcud deyil</p>}
                    </label>
                  );
                })}
              </div>

              {deliveryType !== "pickup" && (
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="flex items-center gap-1.5 text-xs font-medium text-gray-600 mb-1.5">
                      <Calendar size={12} /> Tarix <span className="text-red-500">*</span>
                    </label>
                    <input type="date" value={deliveryDate} min={minDate} max={maxDate}
                      onChange={(e) => setDeliveryDate(e.target.value)}
                      className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.deliveryDate ? "border-red-300" : "border-gray-200"}`} />
                  </div>
                  <div>
                    <label className="flex items-center gap-1.5 text-xs font-medium text-gray-600 mb-1.5">
                      <Clock size={12} /> Vaxt aralığı
                    </label>
                    <select value={deliveryTimeSlot} onChange={(e) => setDeliveryTimeSlot(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white">
                      {TIME_SLOTS.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
                    </select>
                  </div>
                </div>
              )}

              <div className="mt-4">
                <label className="flex items-center gap-1.5 text-xs font-medium text-gray-600 mb-1.5">
                  <FileText size={12} /> Sifariş qeydi
                </label>
                <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2}
                  className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-none"
                  placeholder="Kuryer üçün xüsusi təlimat, zəng etmə vaxtı..." />
              </div>
            </section>

            {/* Kart mesajı — ödənişdən əvvəl */}
            <section className="bg-white rounded-2xl border border-gray-100 p-5" aria-labelledby="gift-card-heading">
              <div className="flex items-center gap-2 mb-3">
                <Gift size={18} className="text-[#65a30d]" aria-hidden />
                <h2 id="gift-card-heading" className="font-semibold text-gray-800">Kart mesajı</h2>
              </div>
              <div className="rounded-xl border border-lime-200 bg-lime-50/60 px-3 py-2.5 text-xs text-lime-900 mb-4">
                Sifariş verdikdən sonra <strong>15 dəqiqə</strong> ərzində kart mesajınızı «Sifarişlərim» səhifəsindən yeniləyə bilərsiniz (funksiya tezliklə aktivləşəcək).
              </div>
              <div className="space-y-4">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Hazır mesaj şablonu</label>
                  <select
                    value={cardTemplate}
                    onChange={(e) => setCardTemplate(e.target.value)}
                    className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white"
                  >
                    {CARD_MESSAGE_TEMPLATES.map((t) => (
                      <option key={t.value || "none"} value={t.value}>{t.label}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Kart mesajınız</label>
                  <textarea
                    value={cardMessage}
                    onChange={(e) => setCardMessage(e.target.value)}
                    rows={4}
                    maxLength={2000}
                    className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-y min-h-[100px]"
                    placeholder="Kart mesajınızı buraya yazın…"
                  />
                  <p className="text-xs text-gray-400 mt-1">{cardMessage.length} / 2000</p>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Kartda göstəriləcək göndərən adı</label>
                  <input
                    type="text"
                    value={cardSenderName}
                    onChange={(e) => setCardSenderName(e.target.value)}
                    maxLength={120}
                    className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                    placeholder="Məs. Aygün Məmmədova"
                  />
                  <p className="text-xs text-gray-500 mt-1.5">
                    Boş buraxsanız, hədiyyə anonim göndərilə bilər; ad yazsanız, çiçək kartında çap olunur.
                  </p>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Göndərmə məqsədi</label>
                  <select
                    value={cardOccasion}
                    onChange={(e) => setCardOccasion(e.target.value)}
                    className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] bg-white"
                  >
                    {OCCASION_OPTIONS.map((o) => (
                      <option key={o.value || "none"} value={o.value}>{o.label}</option>
                    ))}
                  </select>
                </div>
              </div>
            </section>

            {/* Payment */}
            <section className="bg-white rounded-2xl border border-gray-100 p-5">
              <div className="flex items-center gap-2 mb-4">
                <CreditCard size={18} className="text-[#65a30d]" />
                <h2 className="font-semibold text-gray-800">Ödəniş üsulu</h2>
              </div>
              <div className="space-y-2 mb-4">
                {PAYMENT_OPTIONS.map(({ value, title, description, icon: Icon }) => (
                  <label key={value}
                    className={`flex items-start gap-3 p-3 rounded-xl border cursor-pointer transition-colors ${paymentMethod === value ? "border-[#65a30d] bg-lime-50/50" : "border-gray-200 hover:border-lime-200"}`}>
                    <input type="radio" name="payment" value={value} checked={paymentMethod === value}
                      onChange={() => setPaymentMethod(value)} className="mt-1 accent-[#65a30d]" />
                    <Icon size={20} className="text-gray-600 shrink-0 mt-0.5" />
                    <span>
                      <span className="block text-sm font-medium text-gray-800">{title}</span>
                      <span className="block text-xs text-gray-500 mt-0.5">{description}</span>
                    </span>
                  </label>
                ))}
              </div>

              {paymentMethod === "card" && (
                <div className="space-y-4 border-t border-gray-100 pt-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Kart nömrəsi *</label>
                    <div className="relative">
                      <CreditCard size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
                      <input type="text" value={cardNumber} onChange={(e) => setCardNumber(formatCard(e.target.value))}
                        className={`w-full border rounded-xl pl-9 pr-3 py-2.5 text-sm outline-none focus:border-[#65a30d] font-mono tracking-widest ${errors.cardNumber ? "border-red-300" : "border-gray-200"}`}
                        placeholder="0000 0000 0000 0000" maxLength={19} />
                    </div>
                    {errors.cardNumber && <p className="text-xs text-red-500 mt-1">{errors.cardNumber}</p>}
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Kart sahibi *</label>
                    <input type="text" value={cardHolder} onChange={(e) => setCardHolder(e.target.value.toUpperCase())}
                      className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] uppercase ${errors.cardHolder ? "border-red-300" : "border-gray-200"}`}
                      placeholder="AD SOYAD" />
                    {errors.cardHolder && <p className="text-xs text-red-500 mt-1">{errors.cardHolder}</p>}
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1.5">Son tarix *</label>
                      <input type="text" value={cardExpiry} onChange={(e) => setCardExpiry(formatExpiry(e.target.value))}
                        className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.cardExpiry ? "border-red-300" : "border-gray-200"}`}
                        placeholder="MM/YY" maxLength={5} />
                      {errors.cardExpiry && <p className="text-xs text-red-500 mt-1">{errors.cardExpiry}</p>}
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-600 mb-1.5">CVV *</label>
                      <input type="password" value={cardCvv} onChange={(e) => setCardCvv(e.target.value.replace(/\D/g, "").slice(0, 4))}
                        className={`w-full border rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] ${errors.cardCvv ? "border-red-300" : "border-gray-200"}`}
                        placeholder="•••" maxLength={4} />
                      {errors.cardCvv && <p className="text-xs text-red-500 mt-1">{errors.cardCvv}</p>}
                    </div>
                  </div>

                  <button type="button" onClick={() => setShowInstallment(!showInstallment)}
                    className="flex items-center gap-1.5 text-sm text-[#65a30d] font-medium hover:underline">
                    Taksit seçimi {showInstallment ? <ChevronUp size={14} /> : <ChevronDown size={14} />}
                  </button>

                  {showInstallment && (
                    <div className="grid grid-cols-3 gap-2">
                      {INSTALLMENT_OPTIONS.map((opt) => (
                        <button key={opt.months} type="button"
                          onClick={() => setInstallmentMonths(opt.months)}
                          className={`p-2.5 rounded-xl border text-xs font-medium transition-colors ${installmentMonths === opt.months ? "border-[#65a30d] bg-lime-50 text-[#65a30d]" : "border-gray-200 text-gray-600 hover:border-lime-200"}`}>
                          <div>{opt.label}</div>
                          {opt.months > 1 && (
                            <div className="text-gray-400 mt-0.5">
                              {formatMoney(totals.finalPrice / opt.months)}/ay
                            </div>
                          )}
                          {opt.interestRate > 0 && <div className="text-amber-600">+%{opt.interestRate}</div>}
                        </button>
                      ))}
                    </div>
                  )}

                  <div className="flex items-center gap-2 bg-green-50 rounded-xl p-3">
                    <Shield size={16} className="text-green-600 shrink-0" />
                    <p className="text-xs text-green-700">3D Secure ilə qorunur — ödəniş zamanı SMS kodu tələb olunacaq.</p>
                  </div>
                </div>
              )}
            </section>

            {/* Invoice */}
            <section className="bg-white rounded-2xl border border-gray-100 p-5">
              <label className="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" checked={invoiceRequested} onChange={(e) => setInvoiceRequested(e.target.checked)} className="accent-[#65a30d]" />
                <div>
                  <p className="text-sm font-medium text-gray-800">Korporativ faktura / e-arşiv</p>
                  <p className="text-xs text-gray-500">Şirkət adına faktura tələb edirəm</p>
                </div>
              </label>
              {invoiceRequested && (
                <div className="mt-4 grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">Şirkət adı</label>
                    <input type="text" value={invoiceCompanyName} onChange={(e) => setInvoiceCompanyName(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                      placeholder="ABC MMC" />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-600 mb-1.5">VÖEN</label>
                    <input type="text" value={invoiceTaxId} onChange={(e) => setInvoiceTaxId(e.target.value)}
                      className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d]"
                      placeholder="1234567890" />
                  </div>
                </div>
              )}
            </section>
          </div>

          {/* Order summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-2xl border border-gray-100 p-5 lg:sticky lg:top-24">
              <h2 className="font-bold text-gray-800 text-lg mb-4">Sifariş xülasəsi</h2>
              <ul className="space-y-2 max-h-48 overflow-y-auto text-sm mb-4 border-b border-gray-100 pb-4">
                {items.map(({ product, quantity, variantLabel, addons }) => {
                  const addonTotal = (addons ?? []).reduce((a, b) => a + b.price, 0);
                  return (
                    <li key={`${product.id}_${variantLabel}`} className="flex justify-between gap-2 text-gray-600">
                      <span className="line-clamp-2">
                        {product.name}
                        {variantLabel && <span className="text-gray-400"> ({variantLabel})</span>}
                        {" "}&times; {quantity}
                      </span>
                      <span className="shrink-0 font-medium text-gray-800">{formatMoney((product.price + addonTotal) * quantity)}</span>
                    </li>
                  );
                })}
              </ul>

              {/* Coupon */}
              <div className="mb-4">
                {appliedCoupon ? (
                  <div className="flex items-center gap-2 bg-green-50 border border-green-200 rounded-xl px-3 py-2">
                    <Tag size={14} className="text-green-600" />
                    <span className="text-xs text-green-700 flex-1 font-medium">{appliedCoupon} — {appliedCouponLabel}</span>
                    <button type="button" onClick={handleRemoveCoupon} className="text-gray-400 hover:text-red-500">
                      <X size={14} />
                    </button>
                  </div>
                ) : (
                  <div>
                    <div className="flex gap-2">
                      <input type="text" value={couponInput} onChange={(e) => setCouponInput(e.target.value.toUpperCase())}
                        placeholder="Kupon kodu" className="flex-1 border border-gray-200 rounded-xl px-3 py-2 text-xs outline-none focus:border-[#65a30d]" />
                      <button type="button" onClick={handleApplyCoupon}
                        className="px-3 py-2 bg-[#65a30d] text-white rounded-xl text-xs font-semibold hover:bg-[#4d7c0f]">
                        Tətbiq et
                      </button>
                    </div>
                    {couponError && <p className="text-xs text-red-500 mt-1">{couponError}</p>}
                  </div>
                )}
              </div>

              <div className="space-y-2 text-sm">
                <div className="flex justify-between text-gray-600">
                  <span>Məhsul cəmi</span>
                  <span>{formatMoney(totalPrice)}</span>
                </div>
                <div className="flex justify-between text-gray-600">
                  <span>Çatdırılma</span>
                  <span className={totals.shipping === 0 ? "text-green-600 font-medium" : ""}>
                    {totals.shipping === 0 ? "Pulsuz" : formatMoney(totals.shipping)}
                  </span>
                </div>
                {totals.loyaltyDiscount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Sadiqlik endirimi (%5)</span>
                    <span>−{formatMoney(totals.loyaltyDiscount)}</span>
                  </div>
                )}
                {totals.couponDiscount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Kupon endirimi</span>
                    <span>−{formatMoney(totals.couponDiscount)}</span>
                  </div>
                )}
                {totals.installmentInterest > 0 && (
                  <div className="flex justify-between text-amber-600">
                    <span>Taksit faizi</span>
                    <span>+{formatMoney(totals.installmentInterest)}</span>
                  </div>
                )}
                <div className="border-t border-gray-100 pt-2 flex justify-between font-bold text-gray-900 text-base">
                  <span>Cəm</span>
                  <span>{formatMoney(totals.finalPrice)}</span>
                </div>
                {installmentMonths > 1 && (
                  <div className="text-xs text-center text-gray-500">{installmentMonths} × {formatMoney(totals.finalPrice / installmentMonths)}/ay</div>
                )}
              </div>

              <button type="submit" disabled={submitting}
                className="w-full mt-5 bg-[#65a30d] text-white py-3.5 rounded-xl font-semibold text-sm hover:bg-[#4d7c0f] transition-colors disabled:opacity-60 disabled:pointer-events-none flex items-center justify-center gap-2">
                {submitting && <Loader2 size={16} className="animate-spin" />}
                {paymentMethod === "card" ? "Ödənişə keç" : "Sifarişi təsdiqlə"}
              </button>

              <p className="text-xs text-gray-400 mt-3 text-center flex items-center justify-center gap-1">
                <Shield size={12} /> SSL ilə qorunur
              </p>
            </div>
          </div>
        </form>
      </div>
    </>
  );
}
