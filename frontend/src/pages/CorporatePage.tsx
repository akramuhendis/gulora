import { useState } from "react";
import { Link } from "react-router-dom";
import {
  Building2, Package, Award, Users, CheckCircle, ChevronRight,
  Phone, Mail, Briefcase, Gift, Star, Clock,
} from "lucide-react";
import { usePageMeta } from "../hooks/usePageMeta";
import { corporateApi } from "../services/api";

const OCCASIONS = [
  "Şirkət ili dönümü", "Müştəri hədiyyəsi", "İşçi mükafatı",
  "Bayram təbrikləri", "Konfrans / Tədbir", "VIP hədiyyə", "Digər",
];

const PACKAGES = [
  {
    name: "Başlanğıc",
    minQty: "10–49",
    discount: "10%",
    features: ["Fərdi kartlar", "Şirkət loqosu qutu üzərində", "Standart çatdırılma", "Müştəri xidməti dəstəyi"],
    highlight: false,
  },
  {
    name: "Standart",
    minQty: "50–199",
    discount: "18%",
    features: ["Fərdi kartlar", "Şirkət loqosu + rəng uyarlama", "Prioritet çatdırılma", "Həsr olunmuş hesab meneceri", "Toplu fatura"],
    highlight: true,
  },
  {
    name: "Premium",
    minQty: "200+",
    discount: "25%",
    features: ["Tam fərdiləşdirmə", "Lüks markalı qablaşdırma", "Eyni gün çatdırılma", "Həsr olunmuş hesab meneceri", "Fatura + müqavilə dəstəyi", "İl boyu anlaşma"],
    highlight: false,
  },
];

const BENEFITS = [
  { icon: Package, title: "Toplu sifariş endirimləri", desc: "10 ədəddən başlayan sifarişlərə xüsusi korporativ qiymətlər." },
  { icon: Award, title: "Fərdi brend qablaşdırma", desc: "Şirkət loqonuz, rəngləriniz və ismarıcınızla xüsusi qablaşdırma." },
  { icon: Clock, title: "Prioritet çatdırılma", desc: "Toplu sifarişlər üçün planlaşdırılmış çatdırılma cədvəli." },
  { icon: Users, title: "Həsr olunmuş menecerlər", desc: "Sifarişdən çatdırılmaya qədər şəxsi korporativ menecerlər." },
  { icon: Briefcase, title: "Fatura və müqavilə", desc: "Rəsmi fatura, müqavilə və KDV hesabı dəstəyi." },
  { icon: Gift, title: "Kataloq seçim azadlığı", desc: "Bütün məhsul kataloqunda seçim etmə imkanı." },
];

const CLIENTS = [
  "SOCAR", "Kapital Bank", "ABB Bank", "Silk Way", "Pasha Holding",
  "Azercell", "Bakcell", "Nar Mobile", "PAŞA Bank", "AccessBank",
];

export default function CorporatePage() {
  usePageMeta(
    "Korporativ Sifarişlər",
    "Şirkətiniz üçün toplu gül və hədiyyə sifarişləri. Korporativ müştəri xidmətləri, fərdi brend qablaşdırma, endirimli toplu qiymətlər."
  );

  const [form, setForm] = useState({
    company: "", name: "", email: "", phone: "",
    occasion: "", quantity: "", budget: "", message: "",
  });
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [errors, setErrors] = useState<Partial<typeof form>>({});

  const validate = () => {
    const e: Partial<typeof form> = {};
    if (!form.company.trim()) e.company = "Şirkət adı tələb olunur";
    if (!form.name.trim()) e.name = "Ad Soyad tələb olunur";
    if (!form.email.includes("@")) e.email = "Etibarlı e-poçt daxil edin";
    if (form.phone.replace(/\D/g, "").length < 9) e.phone = "Telefon nömrəsi tələb olunur";
    if (!form.occasion) e.occasion = "Məqsəd seçin";
    if (!form.quantity.trim()) e.quantity = "Miqdar tələb olunur";
    return e;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const errs = validate();
    setErrors(errs);
    if (Object.keys(errs).length > 0) return;
    setLoading(true);
    await corporateApi.submitInquiry({
      company: form.company,
      contactName: form.name,
      email: form.email,
      phone: form.phone,
      occasion: form.occasion,
      quantity: form.quantity,
      budget: form.budget,
      message: form.message,
    });
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
      <section className="bg-gradient-to-br from-[#1a1a2e] via-[#16213e] to-[#0f3460] text-white py-20 px-4">
        <div className="max-w-4xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 bg-white/10 border border-white/20 rounded-full px-4 py-1.5 text-sm mb-6">
            <Building2 size={14} /> Korporativ Həllər
          </div>
          <h1 className="text-4xl md:text-5xl font-bold mb-5 leading-tight">
            Şirkətiniz üçün <span className="text-[#65a30d]">xüsusi</span> hədiyyə həlləri
          </h1>
          <p className="text-gray-300 text-lg mb-8 max-w-2xl mx-auto">
            Müştərilərinizə, tərəfdaşlarınıza və işçilərinizə xüsusi anlarda gül, şokolad
            və hədiyyə göndərin. Toplu sifarişlər üçün xüsusi endirim və servis.
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <a href="#form"
              className="bg-[#65a30d] text-white px-7 py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors flex items-center gap-2">
              Sorğu göndər <ChevronRight size={16} />
            </a>
            <a href="tel:*6500"
              className="border border-white/30 text-white px-7 py-3 rounded-xl font-semibold hover:bg-white/10 transition-colors flex items-center gap-2">
              <Phone size={16} /> *6500 Zəng et
            </a>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="bg-white border-b border-gray-100 py-8">
        <div className="max-w-7xl mx-auto px-4">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 text-center">
            {[
              { value: "500+", label: "Korporativ müştəri" },
              { value: "25%", label: "Maksimum endirim" },
              { value: "4 saat", label: "Prioritet çatdırılma" },
              { value: "99%", label: "Müştəri məmnuniyyəti" },
            ].map((s) => (
              <div key={s.label}>
                <div className="text-2xl font-bold text-[#65a30d]">{s.value}</div>
                <div className="text-sm text-gray-500 mt-1">{s.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Benefits */}
      <section className="max-w-7xl mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-gray-800 mb-3">Niyə Gülora Korporativ?</h2>
          <p className="text-gray-500">Biznes tərəfdaşlarınıza və işçilərinizə xüsusi diqqət göstərin</p>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {BENEFITS.map(({ icon: Icon, title, desc }) => (
            <div key={title} className="flex gap-4 p-5 rounded-2xl bg-gray-50 hover:bg-lime-50 transition-colors">
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
      </section>

      {/* Packages */}
      <section className="bg-gray-50 py-16 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-800 mb-3">Korporativ Paketlər</h2>
            <p className="text-gray-500">Şirkətinizin ehtiyacına uyğun paket seçin</p>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {PACKAGES.map((pkg) => (
              <div
                key={pkg.name}
                className={`rounded-2xl p-6 border-2 relative ${
                  pkg.highlight
                    ? "border-[#65a30d] bg-white"
                    : "border-gray-200 bg-white"
                }`}
              >
                {pkg.highlight && (
                  <div className="absolute -top-3 left-1/2 -translate-x-1/2 bg-[#65a30d] text-white text-xs font-bold px-4 py-1 rounded-full">
                    Ən populyar
                  </div>
                )}
                <h3 className="text-xl font-bold text-gray-800 mb-1">{pkg.name}</h3>
                <p className="text-sm text-gray-500 mb-3">{pkg.minQty} ədəd / sifariş</p>
                <div className="text-3xl font-bold text-[#65a30d] mb-5">{pkg.discount} <span className="text-base font-medium text-gray-500">endirim</span></div>
                <ul className="space-y-2.5 mb-6">
                  {pkg.features.map((f) => (
                    <li key={f} className="flex items-center gap-2 text-sm text-gray-600">
                      <CheckCircle size={14} className="text-green-500 shrink-0" /> {f}
                    </li>
                  ))}
                </ul>
                <a href="#form"
                  className={`block text-center py-2.5 rounded-xl text-sm font-semibold transition-colors ${
                    pkg.highlight
                      ? "bg-[#65a30d] text-white hover:bg-[#4d7c0f]"
                      : "border border-gray-200 text-gray-700 hover:border-lime-300"
                  }`}>
                  Sorğu göndər
                </a>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Clients */}
      <section className="max-w-7xl mx-auto px-4 py-12">
        <p className="text-center text-sm text-gray-400 font-medium uppercase tracking-widest mb-8">
          Bizi seçən şirkətlər
        </p>
        <div className="flex flex-wrap justify-center gap-x-10 gap-y-4">
          {CLIENTS.map((c) => (
            <span key={c} className="text-gray-400 font-semibold text-sm hover:text-gray-600 transition-colors">{c}</span>
          ))}
        </div>
      </section>

      {/* Form */}
      <section id="form" className="bg-gray-50 py-16 px-4">
        <div className="max-w-2xl mx-auto">
          <div className="text-center mb-10">
            <h2 className="text-3xl font-bold text-gray-800 mb-3">Korporativ Sorğu Göndər</h2>
            <p className="text-gray-500">Formu doldurun, 24 saat ərzində sizinlə əlaqə quracağıq.</p>
          </div>

          {submitted ? (
            <div className="bg-white rounded-2xl border border-green-200 p-10 text-center">
              <CheckCircle size={52} className="text-green-500 mx-auto mb-4" />
              <h3 className="text-xl font-bold text-gray-800 mb-2">Sorğunuz qəbul edildi!</h3>
              <p className="text-gray-500 mb-6">
                Korporativ satış komandamız 24 saat ərzində{" "}
                <strong>{form.email}</strong> ünvanına e-poçt göndərəcək.
              </p>
              <Link to="/" className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f] transition-colors">
                Ana səhifəyə qayıt
              </Link>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-100 p-8 space-y-5">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Şirkət adı *</label>
                  <input value={form.company} onChange={set("company")} placeholder="MMC / ASC adı"
                    className={inputCls(errors.company)} />
                  {errors.company && <p className="text-xs text-red-500 mt-1">{errors.company}</p>}
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Əlaqə şəxsi *</label>
                  <input value={form.name} onChange={set("name")} placeholder="Ad Soyad"
                    className={inputCls(errors.name)} />
                  {errors.name && <p className="text-xs text-red-500 mt-1">{errors.name}</p>}
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Korporativ e-poçt *</label>
                  <input type="email" value={form.email} onChange={set("email")} placeholder="siz@sirket.az"
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
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Hədiyyə məqsədi *</label>
                  <select value={form.occasion} onChange={set("occasion")} className={inputCls(errors.occasion)}>
                    <option value="">Seçin...</option>
                    {OCCASIONS.map((o) => <option key={o} value={o}>{o}</option>)}
                  </select>
                  {errors.occasion && <p className="text-xs text-red-500 mt-1">{errors.occasion}</p>}
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1.5">Miqdar (ədəd) *</label>
                  <input value={form.quantity} onChange={set("quantity")} placeholder="Ən az 10 ədəd"
                    className={inputCls(errors.quantity)} />
                  {errors.quantity && <p className="text-xs text-red-500 mt-1">{errors.quantity}</p>}
                </div>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Büdcə (ədəd başına, ₼)</label>
                <input value={form.budget} onChange={set("budget")} placeholder="Məs. 50–100 ₼"
                  className={inputCls()} />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1.5">Əlavə qeydlər</label>
                <textarea value={form.message} onChange={set("message")} rows={4}
                  placeholder="Çatdırılma tarixi, xüsusi istəklər, brend tələbləri..."
                  className={`${inputCls()} resize-none`} />
              </div>
              <button type="submit" disabled={loading}
                className="w-full bg-[#65a30d] text-white py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] disabled:opacity-60 transition-colors flex items-center justify-center gap-2">
                {loading ? (
                  <><span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />Göndərilir...</>
                ) : (
                  <><Mail size={16} /> Sorğu göndər</>
                )}
              </button>
              <p className="text-xs text-gray-400 text-center">
                Göndərməklə <Link to="/mexfilik" className="underline hover:text-gray-600">Məxfilik Siyasəti</Link>ni qəbul etmiş olursunuz.
              </p>
            </form>
          )}
        </div>
      </section>

      {/* Contact CTA */}
      <section className="max-w-7xl mx-auto px-4 py-12">
        <div className="bg-[#65a30d] rounded-2xl p-8 flex flex-col md:flex-row items-center justify-between gap-6 text-white">
          <div>
            <h3 className="text-xl font-bold mb-1">Bilavasitə danışmaq istəyirsiniz?</h3>
            <p className="text-lime-100 text-sm">Korporativ satış komandamız həftə içi 09:00–18:00 arasında xidmətinizdədir.</p>
          </div>
          <div className="flex flex-wrap gap-3">
            <a href="tel:*6500" className="flex items-center gap-2 bg-white text-[#65a30d] px-5 py-2.5 rounded-xl text-sm font-semibold hover:bg-lime-50 transition-colors">
              <Phone size={16} /> *6500
            </a>
            <a href="mailto:korporativ@gulora.az" className="flex items-center gap-2 border border-white/40 text-white px-5 py-2.5 rounded-xl text-sm font-semibold hover:bg-white/10 transition-colors">
              <Mail size={16} /> korporativ@gulora.az
            </a>
          </div>
        </div>
      </section>

      {/* Trust */}
      <section className="max-w-7xl mx-auto px-4 pb-16">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[
            { Icon: Star, text: "Hər sifarişdə mükəmməl keyfiyyəti zəmanət edirik." },
            { Icon: CheckCircle, text: "Rəsmi fatura, müqavilə və KDV hesabı." },
            { Icon: Users, text: "Həsr olunmuş menecerlər ilə şəffaf prosess." },
          ].map(({ Icon, text }, i) => (
            <div key={i} className="flex items-center gap-3 p-4 rounded-xl bg-gray-50">
              <Icon size={20} className="text-[#65a30d] shrink-0" />
              <p className="text-sm text-gray-600">{text}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
