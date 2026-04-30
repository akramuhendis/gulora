import { Link } from "react-router-dom";
import { Truck, Shield, Star, RotateCcw } from "lucide-react";

const trustItems = [
  { icon: Truck, title: "Eyni gün çatdırılma", desc: "Saat 14:00-a qədər sifariş verin" },
  { icon: Star, title: "100% Təzə çiçəklər", desc: "Zəmanətlə təzə, ya pul geri" },
  { icon: Shield, title: "Təhlükəsiz ödəniş", desc: "256-bit SSL şifrələnməsi" },
  { icon: RotateCcw, title: "7 gün zəmanət", desc: "Problemsiz geri qaytarma" },
];

const quickCategories = [
  { label: "Anneler Günü", emoji: "🌷", to: "/kampanyalar", highlight: true },
  { label: "Çiçək", emoji: "🌸", to: "/kategori/cicekler" },
  { label: "Yenilebilir", emoji: "🍓", to: "/kategori/gurme-lezzet" },
  { label: "Doğum Günü", emoji: "🎂", to: "/kategori/kek-pasta" },
  { label: "Aynı Gün Hediye", emoji: "🎁", to: "/mehsullar", highlight: true },
  { label: "Trend Çiçekler", emoji: "💐", to: "/mehsullar" },
  { label: "Orkide", emoji: "🪴", to: "/kategori/orkide" },
  { label: "Geçmiş Olsun", emoji: "🤍", to: "/mehsullar" },
  { label: "Özür Çiçekleri", emoji: "🌹", to: "/mehsullar" },
];

export default function HeroBanner() {
  return (
    <>
      <section className="bg-white border-b border-gray-200">
        <div className="max-w-[1400px] mx-auto px-4">
          <div className="py-5">
            <div className="flex items-start gap-3 overflow-x-auto scrollbar-hide pb-2">
              {quickCategories.map((category) => (
                <Link
                  key={category.label}
                  to={category.to}
                  className="group min-w-[84px] max-w-[84px] flex flex-col items-center text-center gap-2"
                >
                  <span
                    className={`relative w-16 h-16 rounded-full border shadow-sm flex items-center justify-center text-2xl transition-all duration-200 group-hover:-translate-y-0.5 group-hover:shadow-md ${
                      category.highlight
                        ? "border-lime-200 bg-linear-to-b from-lime-50 to-white"
                        : "border-gray-200 bg-linear-to-b from-gray-50 to-white"
                    }`}
                  >
                    {category.emoji}
                    {category.highlight && (
                      <span className="absolute -top-1.5 -right-1.5 bg-[#65a30d] text-white text-[9px] font-bold px-1.5 py-0.5 rounded-full">
                        Popüler
                      </span>
                    )}
                  </span>
                  <span className="text-[11px] leading-tight text-gray-700 font-semibold line-clamp-2 group-hover:text-[#65a30d] transition-colors">
                    {category.label}
                  </span>
                </Link>
              ))}
            </div>
          </div>

          <div className="pb-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-12 gap-3">
            <Link
              to="/kampanyalar"
              className="lg:col-span-6 relative rounded-xl overflow-hidden h-52 md:h-56"
            >
              <img
                src="https://images.unsplash.com/photo-1526047932273-341f2a7631f9?w=1200&q=80"
                alt="Anneler günü çiçekleri"
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-black/20" />
              <div className="absolute left-5 top-5 text-white">
                <p className="text-xs font-bold bg-red-500/90 px-2 py-1 rounded-md inline-block mb-2">Aynı gün teslimat</p>
                <h2 className="text-2xl font-extrabold leading-tight">Anneler Günü<br />Çiçekleri</h2>
              </div>
            </Link>

            <Link
              to="/kategori/gurme-lezzet"
              className="lg:col-span-6 relative rounded-xl overflow-hidden h-52 md:h-56"
            >
              <img
                src="https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=1200&q=80"
                alt="Yenilebilir çiçekler"
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-black/25" />
              <div className="absolute left-5 top-5 text-white">
                <p className="text-xs font-bold bg-red-500/90 px-2 py-1 rounded-md inline-block mb-2">Aynı gün teslimat</p>
                <h2 className="text-2xl font-extrabold leading-tight">Annelere<br />Yenilebilir Çiçekler</h2>
              </div>
            </Link>

            <Link
              to="/kategori/cicekler"
              className="lg:col-span-4 relative rounded-xl overflow-hidden h-40 md:h-44"
            >
              <img
                src="https://images.unsplash.com/photo-1487070183336-b863922373d4?w=800&q=80"
                alt="Çiçek aranjmanları"
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-black/25" />
              <div className="absolute left-4 top-4 text-white">
                <h3 className="text-lg font-extrabold leading-tight">Çiçek<br />Aranjmanları</h3>
              </div>
            </Link>

            <Link
              to="/kategori/orkide"
              className="lg:col-span-4 relative rounded-xl overflow-hidden h-40 md:h-44"
            >
              <img
                src="https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800&q=80"
                alt="Annelere orkide"
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-black/25" />
              <div className="absolute left-4 top-4 text-white">
                <h3 className="text-lg font-extrabold leading-tight">Annelere<br />Orkideler</h3>
              </div>
            </Link>

            <Link
              to="/kategori/kek-pasta"
              className="lg:col-span-4 relative rounded-xl overflow-hidden h-40 md:h-44"
            >
              <img
                src="https://images.unsplash.com/photo-1464349095431-e9a21285b5f3?w=800&q=80"
                alt="Doğum günü çiçekleri"
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-purple-700/40" />
              <div className="absolute left-4 top-4 text-white">
                <h3 className="text-lg font-extrabold leading-tight">Doğum Günü<br />Çiçekleri</h3>
              </div>
            </Link>
          </div>
        </div>
      </section>

      {/* Trust Bar */}
      <section className="bg-white border-b border-gray-200">
        <div className="max-w-[1400px] mx-auto px-4">
          <div className="grid grid-cols-2 md:grid-cols-4">
            {trustItems.map((item, i) => (
              <div
                key={i}
                className={`flex items-center gap-3 px-6 py-5 ${i % 2 === 0 && i < 2 ? "border-r border-gray-100" : ""} ${i < 2 ? "border-b border-gray-100 md:border-b-0" : ""} ${i === 1 ? "md:border-r border-gray-100" : ""} ${i === 2 ? "md:border-r border-gray-100" : ""}`}
              >
                <span className="w-10 h-10 rounded-full bg-rose-50 flex items-center justify-center shrink-0">
                  <item.icon size={18} className="text-[#65a30d]" />
                </span>
                <div>
                  <div className="text-sm font-bold text-gray-900">{item.title}</div>
                  <div className="text-xs text-gray-500">{item.desc}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>
    </>
  );
}
