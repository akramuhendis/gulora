import { useState } from "react";
import { Link } from "react-router-dom";
import {
  Store, TrendingUp, Shield, Headphones, CheckCircle,
  ChevronRight, Phone, Mail, MapPin, Package, Star,
} from "lucide-react";
import { usePageMeta } from "../hooks/usePageMeta";
import { sellerApi } from "../services/api";
import { categories } from "../data/products";

const STEPS = [
  {
    num: "01",
    title: "Formu doldurun",
    desc: "Aşağıdakı qeydiyyat formasını doldurun. Satış komandasımız 48 saat ərzində sizinlə əlaqə quracaq.",
  },
  {
    num: "02",
    title: "Yoxlama prosesi",
    desc: "Məhsul keyfiyyəti, çatdırılma imkanı və sənədlər yoxlanılır. Bu proses 3–5 iş günü çəkir.",
  },
  {
    num: "03",
    title: "Mağazanızı açın",
    desc: "Təsdiq aldıqdan sonra məhsullarınızı yükləyin, qiymətləri təyin edin və satışa başlayın.",
  },
];

const BENEFITS = [
  { icon: TrendingUp, title: "Geniş müştəri bazası", desc: "Hər ay 200.000+ aktiv alıcıya çatın. Azərbaycanın ən böyük çiçək platforması." },
  { icon: Shield, title: "Təhlükəsiz ödəniş", desc: "Satışlarınız avtomatik olaraq 7 gündə bir bank hesabınıza köçürülür." },
  { icon: Package, title: "Lojistik dəstəyi", desc: "Öz kuryerimiz vasitəsilə çatdırılma imkanı. Ayrı lojistik lazım deyil." },
  { icon: Headphones, title: "Satıcı dəstəyi", desc: "Həsr olunmuş satıcı xidmət xəttimiz sizə 7/24 kömək edir." },
  { icon: Star, title: "Güclü analitika", desc: "Satış, ziyarətçi, geri dönüş statistikalarınızı real zamanlı izləyin." },
  { icon: Store, title: "Fərdi mağaza səhifəsi", desc: "Şirkət loqonuz, hekayəniz və bütün məhsullarınızla xüsusi mağaza." },
];

const FAQS = [
  { q: "Gülora-da satıcı olmaq pulludurmu?", a: "Xeyr. Qeydiyyat tamamilə pulsuzdur. Komissiya yalnız satış baş verdikdə tutulur." },
  { q: "Komissiya nə qədərdir?", a: "Kateqoriyaya görə 8–15% arasında dəyişir. Həcmli satıcılar üçün endirimli tariflər mövcuddur." },
  { q: "Hansı məhsulları satmaq olar?", a: "Çiçəklər, bitkilər, şokolad, hədiyyələr, oyuncaqlar, kekler, qourmet məhsullar. Yeni kateqoriyalar üçün komanda ilə danışa bilərsiniz." },
  { q: "Çatdırılmanı özüm edə bilərəmmi?", a: "Bəli. Öz çatdırılma komandanızı qurabilir, ya da Gülora lojistik şəbəkəsindən yararlanabilirsiniz." },
  { q: "Minimum neçə məhsul yükləməliyəm?", a: "Minimum məhdudiyyət yoxdur. Bir məhsulla başlamaq olar." },
  { q: "Ödənişlər nə vaxt edilir?", a: "Uğurlu çatdırılmadan 7 gün sonra göstərdiyiniz bank hesabına köçürülür." },
];

export default function SellerPage() {
  usePageMeta(
    "Satıcı Ol — Gülora Platformasında Sat",
    "Gülora platformasında çiçəkçi, konfetçi, tort ustası kimi məhsullarınızı 200.000+ müştəriyə satın. Pulsuz qeydiyyat."
  );

  const [form, setForm] = useState({
    fullName: "",
    company: "",
    taxNumber: "",
    email: "",
    phone: "",
    city: "",
    category: "",
    message: "",
  });
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [errors, setErrors] = useState<Partial<typeof form>>({});
  const [openFaq, setOpenFaq] = useState<number | null>(null);

  const validate = () => {
    const e: Partial<typeof form> = {};
    if (!form.fullName.trim()) e.fullName = "Ad Soyad tələb olunur";
    if (!form.company.trim()) e.company = "Şirkət / Mağaza adı tələb olunur";
    if (!form.taxNumber.trim()) e.taxNumber = "Vergi nömrəsi tələb olunur";
    if (!form.email.trim() || !form.email.includes("@")) e.email = "Etibarlı e-poçt daxil edin";
    if (form.phone.replace(/\D/g, "").length < 9) e.phone = "Telefon nömrəsi tələb olunur";
    if (!form.city.trim()) e.city = "Şəhər tələb olunur";
    if (!form.category) e.category = "Kateqoriya seçin";
    if (!form.message.trim()) e.message = "Qısa məlumat tələb olunur";
    return e;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const errs = validate();
    setErrors(errs);
    if (Object.keys(errs).length > 0) return;
    setLoading(true);
    await sellerApi.apply(form);
    setLoading(false);
    setSubmitted(true);
  };

  const set = (key: keyof typeof form) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    setForm((prev) => ({ ...prev, [key]: e.target.value }));
    if (errors[key]) setErrors((prev) => ({ ...prev, [key]: undefined }));
  };

  const inputCls = (err?: string) =>
    `w-full border rounded-xl px-3 py-2.5 text-sm outline-none transition-colors ${
      err ? "border-red-400 focus:border-red-500" : "border-gray-200 focus:border-[#65a30d]"
    }`;

  return (
    <div className="min-h-screen">
      {/* Hero */}
      <section className="bg-gradient-to-br from-[#65a30d] to-[#3f6212] text-white py-20 px-4">
        <div className="max-w-4xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 bg-white/15 border border-white/25 rounded-full px-4 py-1.5 text-sm mb-6">
            <Store size={14} /> Satıcı Platforması
          </div>
          <h1 className="text-4xl md:text-5xl font-bold mb-5 leading-tight">
            Məhsullarınızı <span className="text-yellow-300">200.000+</span> müştəriyə satın
          </h1>
          <p className="text-lime-100 text-lg mb-8 max-w-2xl mx-auto">
            Çiçəkçi, şokolatçı, tort ustası, hədiyyəçi — Gülora platformasında mağazanızı açın,
            pulsuz başlayın, uğur qazanın.
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <a href="#form"
              className="bg-white text-[#65a30d] px-7 py-3 rounded-xl font-semibold hover:bg-lime-50 transition-colors flex items-center gap-2">
              İndi qeydiyyatdan keç <ChevronRight size={16} />
            </a>
            <a href="tel:*6500"
              className="border border-white/30 text-white px-7 py-3 rounded-xl font-semibold hover:bg-white/10 transition-colors flex items-center gap-2">
              <Phone size={16} /> Əlavə məlumat al
            </a>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="bg-white border-b border-gray-100 py-8">
        <div className="max-w-7xl mx-auto px-4">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
            {[
              { value: "200K+", label: "Aylıq aktiv alıcı" },
              { value: "1.200+", label: "Aktiv satıcı" },
              { value: "0 ₼", label: "Qeydiyyat haqqı" },
              { value: "7 gün", label: "Ödəniş müddəti" },
            ].map((s) => (
              <div key={s.label}>
                <div className="text-2xl font-bold text-[#65a30d]">{s.value}</div>
                <div className="text-sm text-gray-500 mt-1">{s.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* How it works */}
      <section className="max-w-7xl mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-gray-800 mb-3">Necə işləyir?</h2>
          <p className="text-gray-500">3 addımda satışa başlayın</p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 relative">
          <div className="hidden md:block absolute top-8 left-1/3 right-1/3 h-0.5 bg-lime-100" />
          {STEPS.map((step) => (
            <div key={step.num} className="text-center relative">
              <div className="w-16 h-16 rounded-full bg-lime-100 text-[#65a30d] text-xl font-bold flex items-center justify-center mx-auto mb-4 border-2 border-lime-200">
                {step.num}
              </div>
              <h3 className="font-semibold text-gray-800 mb-2">{step.title}</h3>
              <p className="text-sm text-gray-500 leading-relaxed">{step.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Benefits */}
      <section className="bg-gray-50 py-16 px-4">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-800 mb-3">Satıcı üstünlükləri</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {BENEFITS.map(({ icon: Icon, title, desc }) => (
              <div key={title} className="flex gap-4 p-5 bg-white rounded-2xl border border-gray-100">
                <div className="w-10 h-10 bg-lime-100 rounded-xl flex items-center justify-center shrink-0">
                  <Icon size={18} className="text-[#65a30d]" />
                </div>
                <div>
                  <h3 className="font-semibold text-gray-800 mb-1">{title}</h3>
                  <p className="text-sm text-gray-500 leading-relaxed">{desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Registration form */}
      <section id="form" className="max-w-2xl mx-auto px-4 py-16">
        <div className="text-center mb-10">
          <h2 className="text-3xl font-bold text-gray-800 mb-3">Satıcı qeydiyyatı</h2>
          <p className="text-gray-500">Formu doldurun, satış komandamız sizinlə əlaqə qursun.</p>
        </div>

        {submitted ? (
          <div className="bg-white rounded-2xl border border-green-200 p-10 text-center">
            <CheckCircle size={52} className="text-green-500 mx-auto mb-4" />
            <h3 className="text-xl font-bold text-gray-800 mb-2">Müraciətiniz qəbul edildi!</h3>
            <p className="text-gray-500 mb-6">
              Satıcı komandasımız 48 saat ərzində{" "}
              <strong>{form.email}</strong> ünvanına əlaqə saxlayacaq.
            </p>
            <Link to="/" className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f] transition-colors">
              Ana səhifəyə qayıt
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-8 space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Ad Soyad *</label>
                <input value={form.fullName} onChange={set("fullName")} placeholder="Ad Soyad"
                  className={inputCls(errors.fullName)} />
                {errors.fullName && <p className="text-xs text-red-500 mt-1">{errors.fullName}</p>}
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Şirkət / Mağaza adı *</label>
                <input value={form.company} onChange={set("company")} placeholder="Mağaza və ya şirkət adı"
                  className={inputCls(errors.company)} />
                {errors.company && <p className="text-xs text-red-500 mt-1">{errors.company}</p>}
              </div>
              <div className="sm:col-span-2">
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Vergi nömrəsi *</label>
                <input value={form.taxNumber} onChange={set("taxNumber")} placeholder="VÖEN / vergi identifikasiya nömrəsi"
                  className={inputCls(errors.taxNumber)} inputMode="text" autoComplete="off" />
                {errors.taxNumber && <p className="text-xs text-red-500 mt-1">{errors.taxNumber}</p>}
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">E-poçt *</label>
                <input type="email" value={form.email} onChange={set("email")} placeholder="siz@email.com"
                  className={inputCls(errors.email)} />
                {errors.email && <p className="text-xs text-red-500 mt-1">{errors.email}</p>}
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Telefon *</label>
                <input value={form.phone} onChange={set("phone")} placeholder="+994 50 xxx xx xx"
                  className={inputCls(errors.phone)} />
                {errors.phone && <p className="text-xs text-red-500 mt-1">{errors.phone}</p>}
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Şəhər *</label>
                <input value={form.city} onChange={set("city")} placeholder="Bakı, Sumqayıt..."
                  className={inputCls(errors.city)} />
                {errors.city && <p className="text-xs text-red-500 mt-1">{errors.city}</p>}
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Əsas kateqoriya *</label>
                <select value={form.category} onChange={set("category")} className={inputCls(errors.category)}>
                  <option value="">Seçin...</option>
                  {categories.map((c) => (
                    <option key={c.slug} value={c.slug}>{c.name}</option>
                  ))}
                </select>
                {errors.category && <p className="text-xs text-red-500 mt-1">{errors.category}</p>}
              </div>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1.5">Özünüz haqqında qısa məlumat *</label>
              <textarea value={form.message} onChange={set("message")} rows={4}
                placeholder="Neçə ildir bu sahədəsiniz, hansı məhsulları satırsınız, gündəlik istehsal həcminiz..."
                className={`${inputCls(errors.message)} resize-none`} />
              {errors.message && <p className="text-xs text-red-500 mt-1">{errors.message}</p>}
            </div>
            <button type="submit" disabled={loading}
              className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 transition-colors flex items-center justify-center gap-2">
              {loading ? (
                <><span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" /> Göndərilir...</>
              ) : (
                <>Müraciəti göndər <ChevronRight size={16} /></>
              )}
            </button>
            <p className="text-xs text-gray-400 text-center">
              Göndərməklə <Link to="/mexfilik" className="underline hover:text-gray-600">Məxfilik Siyasəti</Link>ni qəbul etmiş olursunuz.
            </p>
          </form>
        )}
      </section>

      {/* FAQ */}
      <section className="bg-gray-50 py-16 px-4">
        <div className="max-w-2xl mx-auto">
          <div className="text-center mb-10">
            <h2 className="text-3xl font-bold text-gray-800 mb-3">Tez-tez verilən suallar</h2>
          </div>
          <div className="space-y-3">
            {FAQS.map((faq, i) => (
              <div key={i} className="bg-white rounded-xl border border-gray-100 overflow-hidden">
                <button
                  type="button"
                  onClick={() => setOpenFaq(openFaq === i ? null : i)}
                  className="w-full flex items-center justify-between px-5 py-4 text-left text-sm font-medium text-gray-800 hover:bg-gray-50"
                >
                  {faq.q}
                  <ChevronRight
                    size={16}
                    className={`text-gray-400 transition-transform shrink-0 ml-2 ${openFaq === i ? "rotate-90" : ""}`}
                  />
                </button>
                {openFaq === i && (
                  <div className="px-5 pb-4 text-sm text-gray-600 border-t border-gray-100 pt-3">
                    {faq.a}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Contact */}
      <section className="max-w-7xl mx-auto px-4 py-12">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          {[
            { icon: Phone, label: "Satıcı dəstəy xətti", value: "*6501", href: "tel:*6501" },
            { icon: Mail, label: "Satıcı e-poçt", value: "satici@gulora.az", href: "mailto:satici@gulora.az" },
            { icon: MapPin, label: "Ünvan", value: "Nizami küç. 203, Bakı", href: "#" },
          ].map(({ icon: Icon, label, value, href }) => (
            <a key={label} href={href}
              className="flex items-center gap-4 p-5 bg-gray-50 rounded-2xl hover:bg-lime-50 transition-colors group">
              <div className="w-10 h-10 bg-lime-100 rounded-xl flex items-center justify-center shrink-0 group-hover:bg-lime-200 transition-colors">
                <Icon size={18} className="text-[#65a30d]" />
              </div>
              <div>
                <p className="text-xs text-gray-400">{label}</p>
                <p className="text-sm font-semibold text-gray-800">{value}</p>
              </div>
            </a>
          ))}
        </div>
      </section>
    </div>
  );
}
