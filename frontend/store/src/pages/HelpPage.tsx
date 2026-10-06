import { useEffect } from "react";
import { Link, useLocation } from "react-router-dom";
import { ChevronRight, Mail, Phone } from "lucide-react";

const sections = [
  {
    id: "karyera",
    title: "Karyera",
    body: "Gülora komandasına qoşulmaq istəyirsinizsə, CV-nizi destek@gulora.az ünvanına göndərin. Vakansiyalar tezliklə bu bölmədə dərc olunacaq.",
  },
  {
    id: "metbuat",
    title: "Mətbuat",
    body: "Mətbuat sorğuları üçün e-poçt: destek@gulora.az mövzusu sətirində «Mətbuat» qeyd edin.",
  },
  {
    id: "mexfilik",
    title: "Məxfilik siyasəti",
    body: "Şəxsi məlumatlarınız yalnız sifarişin icrası və müştəri xidməti üçün emal olunur. Üçüncü tərəflərə satışı həyata keçirmirik. Ətraflı mətn hüquq şöbəsi tərəfindən təsdiqlənəndə yenilənəcək.",
  },
  {
    id: "istifade",
    title: "İstifadə şərtləri",
    body: "Saytdan istifadə Gülora qaydalarına tabedir. Qiymətlər və çatdırılma şərtləri sifariş anında təsdiqlənən məlumatlar üzrədir.",
  },
  {
    id: "kvkk",
    title: "Şəxsi məlumatların qorunması",
    body: "Azərbaycan Respublikasının müvafiq qanunvericiliyinə uyğun olaraq məlumatlarınızı qoruyuruq. Sorğular üçün *6500 və ya destek@gulora.az.",
  },
];

export default function HelpPage() {
  const { hash } = useLocation();

  useEffect(() => {
    if (!hash) return;
    const id = hash.replace("#", "");
    const el = document.getElementById(id);
    el?.scrollIntoView({ behavior: "smooth", block: "start" });
  }, [hash]);

  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-8">
        <Link to="/" className="hover:text-[#65a30d]">
          Ana səhifə
        </Link>
        <ChevronRight size={14} className="text-gray-300" />
        <span className="text-gray-700 font-medium">Kömək</span>
      </nav>

      <h1 className="text-3xl font-bold text-gray-900 mb-2">Kömək mərkəzi</h1>
      <p className="text-sm text-gray-500 mb-10">
        Tez-tez verilən suallar və əlaqə.
      </p>

      <div className="bg-white rounded-2xl border border-gray-100 p-5 mb-10 space-y-3">
        <a href="tel:*6500" className="flex items-center gap-3 text-sm text-gray-700 hover:text-[#65a30d]">
          <Phone size={18} className="text-[#65a30d] shrink-0" />
          Telefon: *6500
        </a>
        <a href="mailto:destek@gulora.az" className="flex items-center gap-3 text-sm text-gray-700 hover:text-[#65a30d]">
          <Mail size={18} className="text-[#65a30d] shrink-0" />
          destek@gulora.az
        </a>
      </div>

      <div className="space-y-10">
        <section>
          <h2 className="text-lg font-bold text-gray-800 mb-3">Sifariş və çatdırılma</h2>
          <ul className="list-disc pl-5 text-sm text-gray-600 space-y-2">
            <li>Sifarişi təsdiqlədikdən sonra çatdırılma statusu haqqında məlumat alacaqsınız.</li>
            <li>Eyni gün çatdırılma üçün sifariş saatları haqqında məhsul səhifəsindəki qeydlərə baxın.</li>
            <li>Ünvanı səhv daxil etmisinizsə, tez bir zamanda dəstəklə əlaqə saxlayın.</li>
          </ul>
        </section>

        {sections.map((s) => (
          <section key={s.id} id={s.id} className="scroll-mt-24">
            <h2 className="text-lg font-bold text-gray-800 mb-3">{s.title}</h2>
            <p className="text-sm text-gray-600 leading-relaxed">{s.body}</p>
          </section>
        ))}
      </div>

      <Link to="/" className="inline-block mt-10 text-sm font-semibold text-[#65a30d] hover:underline">
        ← Ana səhifə
      </Link>
    </div>
  );
}
