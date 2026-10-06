import { Link } from "react-router-dom";
import { ArrowLeft, Shield, Eye, Lock, Database, UserCheck, Trash2 } from "lucide-react";

const SECTIONS = [
  {
    icon: Database,
    title: "1. Toplanan məlumatlar",
    content: `Gülora platformasında istifadə zamanı aşağıdakı məlumatlar toplanır:
    
• Ad, soyad, e-poçt ünvanı, telefon nömrəsi (qeydiyyat zamanı)
• Çatdırılma ünvanları (şəhər, rayon, küçə)
• Sifariş tarixi, məhsullar, ödəniş üsulu (kart nömrəsi saxlanılmır)
• Brauzer növü, IP ünvanı, sessiya məlumatları (analitika üçün)
• Cihaz məlumatları (mobil tətbiq istifadəsində)`,
  },
  {
    icon: Eye,
    title: "2. Məlumatların istifadəsi",
    content: `Topladığımız məlumatlar aşağıdakı məqsədlər üçün istifadə edilir:

• Sifarişlərin emalı və çatdırılma koordinasiyası
• Hesab yaradılması və təhlükəsizliyin təmin edilməsi
• Müştəri dəstəyi təqdim edilməsi
• Bildiriş və kampaniya məlumatlarının göndərilməsi (razılıq varsa)
• Platforma funksionallığının təkmilləşdirilməsi
• Hüquqi öhdəliklərin yerinə yetirilməsi`,
  },
  {
    icon: Shield,
    title: "3. Məlumatların qorunması",
    content: `İstifadəçi məlumatları aşağıdakı metodlarla qorunur:

• HTTPS/SSL şifrələmə — bütün məlumat ötürülməsini əhatə edir
• Kart məlumatları heç vaxt serverdə saxlanılmır (PCI-DSS uyğunluğu)
• Ödəniş məlumatları iyzico/bank POS vasitəsilə idarə edilir
• Şifrələr hash algoritmı ilə saxlanılır (düz mətn saxlanılmır)
• Müntəzəm təhlükəsizlik auditi və sızma testi`,
  },
  {
    icon: UserCheck,
    title: "4. İstifadəçi hüquqları",
    content: `KVKK (Şəxsi Məlumatların Qorunması haqqında Qanun) çərçivəsində aşağıdakı hüquqlara sahibsiniz:

• Məlumatlarınızı öyrənmə hüququ
• Məlumatların düzəldilməsini tələb etmə hüququ
• Məlumatların silinməsini tələb etmə hüququ ("unudulma hüququ")
• Emal məqsədlerinə etiraz etmə hüququ
• Məlumatların köçürülməsini tələb etmə hüququ
• Avtomatik qərar qəbuluna qarşı olmaq hüququ

Bu hüquqları həyata keçirmək üçün: destek@gulora.az`,
  },
  {
    icon: Lock,
    title: "5. Çərəzlər (Cookies)",
    content: `Gülora aşağıdakı çərəz növlərindən istifadə edir:

• Zəruri çərəzlər: Sessiya idarəetməsi, giriş vəziyyəti (silinə bilməz)
• Funksional çərəzlər: Dil, valyuta seçimləri
• Analitik çərəzlər: Platforma istifadəsi statistikası (Google Analytics)
• Reklam çərəzləri: Fərdiləşdirilmiş reklam (razılıq əsaslı)

Çərəz tənzimləmələrini brauzer parametrlərindən dəyişdirə bilərsiniz.`,
  },
  {
    icon: Trash2,
    title: "6. Məlumatların silinməsi",
    content: `Hesabınızı silmək üçün:
    
1. Hesabım → Tənzimləmələr → Hesabı sil
2. destek@gulora.az ünvanına müraciət

Silmə sorğusu qəbul edildikdən 30 gün ərzində bütün şəxsi məlumatlarınız silinir. Hüquqi öhdəliklər səbəbindən saxlanılması tələb olunan məlumatlar (vergi, sifariş qeydləri) müvafiq müddət ərzində arxivdə saxlanılır.`,
  },
];

export default function PrivacyPage() {
  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <Link to="/" className="inline-flex items-center gap-1.5 text-sm text-gray-500 hover:text-[#65a30d] mb-6">
        <ArrowLeft size={16} /> Ana səhifə
      </Link>

      <div className="mb-8">
        <div className="flex items-center gap-3 mb-3">
          <div className="w-10 h-10 bg-lime-50 rounded-xl flex items-center justify-center">
            <Shield size={22} className="text-[#65a30d]" />
          </div>
          <h1 className="text-3xl font-bold text-gray-800">Məxfilik Siyasəti</h1>
        </div>
        <p className="text-gray-500 text-sm">
          Son yenilənmə: 1 aprel 2026 · Gülora MMC, Bakı, Azərbaycan
        </p>
        <p className="text-sm text-gray-600 mt-3 leading-relaxed">
          Gülora olaraq şəxsi məlumatlarınızın qorunmasına böyük önəm veririk. Bu sənəd hansı məlumatları topladığımızı, necə istifadə etdiyimizi və hüquqlarınızı izah edir.
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

        <section className="bg-lime-50 rounded-2xl border border-lime-100 p-5">
          <h2 className="font-semibold text-gray-800 mb-2">Əlaqə</h2>
          <p className="text-sm text-gray-600">
            Məxfilik siyasəti ilə bağlı suallarınız üçün:<br />
            <strong>E-poçt:</strong> destek@gulora.az<br />
            <strong>Ünvan:</strong> Nizami küç. 203, Bakı, Azərbaycan<br />
            <strong>Telefon:</strong> *6500
          </p>
        </section>
      </div>

      <div className="mt-8 flex gap-3 text-sm">
        <Link to="/istifade-sertleri" className="text-[#65a30d] hover:underline">İstifadə şərtləri</Link>
        <span className="text-gray-300">·</span>
        <Link to="/yardim" className="text-[#65a30d] hover:underline">Yardım mərkəzi</Link>
      </div>
    </div>
  );
}
