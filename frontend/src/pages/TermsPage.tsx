import { Link } from "react-router-dom";
import { ArrowLeft, FileText, ShoppingBag, Truck, CreditCard, AlertTriangle, Scale } from "lucide-react";

const SECTIONS = [
  {
    icon: ShoppingBag,
    title: "1. Xidmət şərtləri",
    content: `Gülora platformasını istifadə etməklə aşağıdakı şərtləri qəbul etmiş olursunuz:

• Platformanı yalnız qanuni məqsədlər üçün istifadə edirsiniz
• Hesab məlumatlarınızın gizliliyini qoruyursunuz
• 18 yaşından yuxarı olduğunuzu təsdiqləyirsiniz (və ya valideyn icazəsi)
• Yanlış məlumat təqdim etməyəcəyinizi öhdəsinizə götürürsünüz`,
  },
  {
    icon: CreditCard,
    title: "2. Qiymət və ödəniş",
    content: `• Bütün qiymətlər AZN (Azərbaycan manatı) ilə göstərilir
• Ödəniş bank kartı, nağd (çatdırılma zamanı) və ya bank köçürməsi ilə edilə bilər
• Kart ödənişlərində 3D Secure təhlükəsizlik protokolu tətbiq edilir
• Taksit seçimi yalnız müəyyən kartlar üçün mövcuddur
• Qiymətlər öncədən xəbərdarlıq olmadan dəyişdirilə bilər
• Kampaniya endirimləri digər endirimlər ilə üst-üstə düşə bilməz`,
  },
  {
    icon: Truck,
    title: "3. Çatdırılma",
    content: `• Eyni gün çatdırılma: saat 14:00-a qədər verilən sifarişlər üçün keçərlidir (Bakı)
• Standart çatdırılma zamanı seçilmiş tarix və vaxt aralığıdır
• Kargo çatdırılması 2-3 iş günü sürgünlüqədir
• Yanlış ünvan məlumatı nəticəsindəki gecikmə müştəri məsuliyyətindədir
• Pulsuz çatdırılma şərtləri kampaniyaya görə dəyişə bilər`,
  },
  {
    icon: AlertTriangle,
    title: "4. Geri qaytarma və ləğv",
    content: `• Sifariş çatdırılmadan əvvəl ləğv edilə bilər (hazırlanma mərhələsinə qədər)
• Çatdırılmadan sonra 24 saat ərzində məhsul problemi bildirilməlidir
• Zədəli, yanlış və ya keyfiyyətsiz məhsullar pulsuz dəyişdirilir
• Müştəri fikir dəyişikliyi ilə iade qəbul edilmir (taze çiçek mahsulları)
• İadə sorğusu Hesabım → İadə / Şikayət bölməsindən edilir`,
  },
  {
    icon: Scale,
    title: "5. Məsuliyyət məhdudiyyəti",
    content: `Gülora aşağıdakı hallarda məsuliyyət daşımır:

• Müştərinin yanlış ünvan/əlaqə məlumatı ilə bağlı gecikmə
• Doğal fəlakət, hava şəraiti, yol bağlanması kimi force majeure hallar
• Üçüncü tərəf marketplace satıcılarının məhsulları (satıcı məsuldur)
• Platformanın qısa müddətli texniki fasiləsi
• İstifadəçinin özünün hesab məlumatlarını paylaşması nəticəsindəki zərər`,
  },
  {
    icon: FileText,
    title: "6. Fikri mülkiyyət",
    content: `• Gülora adı, loqosu, dizaynı qorunmuş ticarət nişanlarıdır
• Platforma məzmunu (şəkillər, mətnlər, kod) müəllif hüququ ilə qorunur
• İstifadəçilər tərəfindən yüklənən rəy və şəkillərin platformada istifadəsinə icazə verilir
• Platformanı kopyalamaq, əks-mühəndislik etmək qadağandır`,
  },
];

export default function TermsPage() {
  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <Link to="/" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-[#65a30d] mb-6">
        <ArrowLeft size={16} /> Ana səhifə
      </Link>

      <div className="mb-8">
        <div className="flex items-center gap-3 mb-3">
          <div className="w-10 h-10 bg-lime-50 rounded-xl flex items-center justify-center">
            <FileText size={22} className="text-[#65a30d]" />
          </div>
          <h1 className="text-3xl font-bold text-gray-800">İstifadə Şərtləri</h1>
        </div>
        <p className="text-gray-500 text-sm">Son yenilənmə: 1 aprel 2026 · Gülora MMC, Bakı, Azərbaycan</p>
        <p className="text-sm text-gray-600 mt-3 leading-relaxed">
          Bu sənəd Gülora platformasının istifadə qaydalarını müəyyən edir. Platformadan istifadə etməklə bu şərtləri qəbul etmiş olursunuz.
        </p>
      </div>

      <div className="space-y-6">
        {SECTIONS.map(({ icon: Icon, title, content }) => (
          <section key={title} className="bg-white rounded-2xl border border-gray-100 p-5">
            <h2 className="font-semibold text-gray-800 flex items-center gap-2 mb-3">
              <Icon size={18} className="text-[#65a30d]" /> {title}
            </h2>
            <div className="text-sm text-gray-600 whitespace-pre-line leading-relaxed">{content}</div>
          </section>
        ))}

        <section className="bg-gray-50 rounded-2xl border border-gray-200 p-5">
          <h2 className="font-semibold text-gray-800 mb-2">7. Tətbiq olunan hüquq</h2>
          <p className="text-sm text-gray-600">
            Bu şərtlər Azərbaycan Respublikasının qanunvericiliyinə uyğun olaraq idarə edilir. Mübahisəli hallarda Bakı şəhər məhkəmələri yetkilidir. Şərtlər öncədən xəbərdarlıq edilmədən dəyişdirilə bilər; əsas dəyişikliklər haqqında e-poçt ilə məlumat veriləcək.
          </p>
        </section>

        <section className="bg-lime-50 rounded-2xl border border-lime-100 p-5">
          <h2 className="font-semibold text-gray-800 mb-2">Əlaqə</h2>
          <p className="text-sm text-gray-600">
            Şərtlər ilə bağlı suallarınız üçün:<br />
            <strong>E-poçt:</strong> destek@gulora.az<br />
            <strong>Telefon:</strong> *6500 (həftə içi 08:00–22:00)
          </p>
        </section>
      </div>

      <div className="mt-8 flex gap-3 text-sm">
        <Link to="/mexfilik" className="text-[#65a30d] hover:underline">Məxfilik siyasəti</Link>
        <span className="text-gray-300">·</span>
        <Link to="/yardim" className="text-[#65a30d] hover:underline">Yardım mərkəzi</Link>
      </div>
    </div>
  );
}
