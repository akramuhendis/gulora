import { Link } from "react-router-dom";
import { Phone, Mail, MapPin, Share2, Send, Video } from "lucide-react";
import { categories } from "../data/products";

export default function Footer() {
  return (
    <footer className="bg-gray-950 text-gray-400 mt-16">
      {/* Main footer */}
      <div className="max-w-[1400px] mx-auto px-4 py-12">
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-10">
          {/* Brand */}
          <div>
            <div className="flex items-center gap-2 mb-4">
              <div className="w-9 h-9 bg-[#65a30d] rounded-lg flex items-center justify-center">
                <span className="text-white text-xl">🌸</span>
              </div>
              <span className="text-white font-extrabold text-xl">Gülora</span>
            </div>
            <p className="text-gray-500 text-sm leading-relaxed mb-5">
              Azərbaycanın ən etibarlı onlayn gül və hədiyyə platforması.
              Sevdiklərinizi xoşbəxt edin.
            </p>
            <div className="flex gap-2.5">
              {[
                { Icon: Share2, label: "Sosial media", href: "#" },
                { Icon: Send, label: "Telegram", href: "#" },
                { Icon: Video, label: "Video", href: "#" },
              ].map(({ Icon, label, href }, i) => (
                <a
                  key={i}
                  href={href}
                  aria-label={label}
                  className="w-9 h-9 bg-gray-800 border border-gray-700 rounded-lg flex items-center justify-center text-gray-400 hover:text-white hover:bg-[#65a30d] hover:border-[#65a30d]"
                >
                  <Icon size={15} />
                </a>
              ))}
            </div>
          </div>

          {/* Categories */}
          <div>
            <h3 className="text-white font-bold mb-4 text-sm uppercase tracking-widest">
              Kateqoriyalar
            </h3>
            <ul className="space-y-2">
              {categories.slice(0, 8).map((cat) => (
                <li key={cat.id}>
                  <Link
                    to={`/kategori/${cat.slug}`}
                    className="text-gray-500 hover:text-white text-sm"
                  >
                    {cat.name}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Company */}
          <div>
            <h3 className="text-white font-bold mb-4 text-sm uppercase tracking-widest">
              Şirkət
            </h3>
            <ul className="space-y-2">
              {[
                { to: "/haqqimizda", label: "Haqqımızda" },
                { to: "/korporativ", label: "Korporativ sifarişlər" },
                { to: "/satici-ol", label: "Satıcı ol" },
                { to: "/blog", label: "Bloq" },
                { to: "/kampanyalar", label: "Kampaniyalar" },
                { to: "/mexfilik", label: "Məxfilik siyasəti" },
                { to: "/istifade-sertleri", label: "İstifadə şərtləri" },
              ].map(({ to, label }) => (
                <li key={to}>
                  <Link to={to} className="text-gray-500 hover:text-white text-sm">
                    {label}
                  </Link>
                </li>
              ))}
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h3 className="text-white font-bold mb-4 text-sm uppercase tracking-widest">
              Əlaqə
            </h3>
            <ul className="space-y-3 mb-5">
              <li>
                <a href="tel:*6500" className="flex items-center gap-2.5 text-sm text-gray-500 hover:text-white">
                  <Phone size={14} className="text-[#65a30d] shrink-0" />
                  *6500
                </a>
              </li>
              <li>
                <a href="mailto:destek@gulora.az" className="flex items-center gap-2.5 text-sm text-gray-500 hover:text-white">
                  <Mail size={14} className="text-[#65a30d] shrink-0" />
                  destek@gulora.az
                </a>
              </li>
              <li className="flex items-start gap-2.5 text-sm text-gray-500">
                <MapPin size={14} className="text-[#65a30d] shrink-0 mt-0.5" />
                Nizami küç. 203, Bakı, Azərbaycan
              </li>
            </ul>

            <div className="p-4 bg-gray-900 rounded-xl border border-gray-800">
              <div className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-2">İş saatları</div>
              <div className="text-white text-sm font-semibold">Həftə içi: 08:00 – 22:00</div>
              <div className="text-white text-sm font-semibold">Həftə sonu: 09:00 – 21:00</div>
            </div>
          </div>
        </div>
      </div>

      {/* Bottom bar */}
      <div className="border-t border-gray-900">
        <div className="max-w-[1400px] mx-auto px-4 py-5 flex flex-col md:flex-row items-center justify-between gap-3 text-xs text-gray-600">
          <span>© 2026 Gülora. Bütün hüquqlar qorunur.</span>
          <div className="flex items-center gap-3 flex-wrap justify-center">
            <span className="text-gray-600">Ödəniş üsulları:</span>
            {["💳 Visa", "💳 MC", "💳 Troy", "🏦 Köçürmə"].map((p) => (
              <span key={p} className="bg-gray-900 border border-gray-800 px-2.5 py-1 rounded text-gray-500">
                {p}
              </span>
            ))}
          </div>
        </div>
      </div>
    </footer>
  );
}
