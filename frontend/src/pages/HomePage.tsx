import { Link } from "react-router-dom";
import { ArrowRight, TrendingUp, Tag, Flame, Grid3X3 } from "lucide-react";
import HeroBanner from "../components/HeroBanner";
import ProductCard from "../components/ProductCard";
import {
  categories,
  getBestSellers,
  getDiscountedProducts,
  getFeaturedProducts,
  products,
} from "../data/products";

function SectionHeader({
  icon,
  title,
  subtitle,
  link,
  badge,
}: {
  icon: React.ReactNode;
  title: string;
  subtitle?: string;
  link?: string;
  badge?: string;
}) {
  return (
    <div className="flex items-end justify-between mb-6">
      <div>
        <div className="flex items-center gap-2.5 mb-1">
          {icon}
          <h2 className="text-xl md:text-2xl font-extrabold text-gray-900">{title}</h2>
          {badge && (
            <span className="bg-red-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-full">
              {badge}
            </span>
          )}
        </div>
        {subtitle && <p className="text-sm text-gray-500">{subtitle}</p>}
      </div>
      {link && (
        <Link
          to={link}
          className="flex items-center gap-1 text-sm text-[#65a30d] font-semibold border border-[#65a30d] px-3 py-1.5 rounded-lg"
        >
          Hamısına bax <ArrowRight size={14} />
        </Link>
      )}
    </div>
  );
}

export default function HomePage() {
  const bestSellers = getBestSellers().slice(0, 8);
  const discounted = getDiscountedProducts().slice(0, 6);
  const featured = getFeaturedProducts().slice(0, 4);

  return (
    <div className="bg-gray-50">
      <HeroBanner />

      <div className="max-w-[1400px] mx-auto px-4 py-10 space-y-14">

        {/* Categories */}
        <section>
          <SectionHeader
            icon={<Grid3X3 size={20} className="text-[#65a30d]" />}
            title="Kateqoriyalar"
            subtitle="Nə axtarırsınız? Birbaşa keçin"
          />
          <div className="grid grid-cols-3 sm:grid-cols-5 md:grid-cols-10 gap-2">
            {categories.map((cat) => (
              <Link
                key={cat.id}
                to={`/kategori/${cat.slug}`}
                className="flex flex-col items-center gap-2 p-3 rounded-xl bg-white border border-gray-200 text-center"
              >
                <div
                  className="w-12 h-12 rounded-full flex items-center justify-center text-xl"
                  style={{ backgroundColor: `${cat.color}18` }}
                >
                  {cat.icon}
                </div>
                <span className="text-xs font-semibold text-gray-700 leading-tight">
                  {cat.name}
                </span>
              </Link>
            ))}
          </div>
        </section>

        {/* Promotional Banners */}
        <section className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="md:col-span-2 relative rounded-xl overflow-hidden bg-linear-to-r from-rose-600 to-lime-500 p-7 text-white">
            <div className="relative z-10">
              <div className="text-xs font-bold bg-white/20 inline-block px-2.5 py-1 rounded-full mb-3 uppercase tracking-wider">
                Xüsusi təklif
              </div>
              <h3 className="text-2xl font-extrabold mb-1">Eyni gün çatdırılma</h3>
              <p className="text-rose-100 text-sm mb-5 max-w-xs">
                Saat 14:00-a qədər sifariş verin, axşam ünvanınızda olsun.
                Bütün şəhərlərə çatdırılma.
              </p>
              <Link
                to="/kategori/cicekler"
                className="inline-flex items-center gap-2 bg-white text-rose-600 text-sm font-bold px-5 py-2.5 rounded-lg"
              >
                İndi sifariş ver <ArrowRight size={14} />
              </Link>
            </div>
            <span className="absolute right-8 top-1/2 -translate-y-1/2 text-8xl opacity-10 pointer-events-none select-none">
              🌸
            </span>
          </div>

          <div className="relative rounded-xl overflow-hidden bg-linear-to-r from-violet-600 to-purple-500 p-7 text-white">
            <div className="relative z-10">
              <div className="text-xs font-bold bg-white/20 inline-block px-2.5 py-1 rounded-full mb-3 uppercase tracking-wider">
                Kampaniya
              </div>
              <h3 className="text-2xl font-extrabold mb-1">%25 Endirim</h3>
              <p className="text-purple-100 text-sm mb-5">
                Seçilmiş məhsullarda böyük endirim. Tələsin!
              </p>
              <Link
                to="/kampanyalar"
                className="inline-flex items-center gap-2 bg-white text-purple-600 text-sm font-bold px-5 py-2.5 rounded-lg"
              >
                Kəşf et <ArrowRight size={14} />
              </Link>
            </div>
            <span className="absolute right-6 top-1/2 -translate-y-1/2 text-8xl opacity-10 pointer-events-none select-none">
              🎁
            </span>
          </div>
        </section>

        {/* Best Sellers */}
        <section>
          <SectionHeader
            icon={<TrendingUp size={20} className="text-orange-500" />}
            title="Ən çox satılanlar"
            subtitle="Ən bəyənilən, ən çox sifariş olunan məhsullar"
            link="/mehsullar"
          />
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {bestSellers.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        </section>

        {/* Full-width CTA Banner */}
        <section className="rounded-xl overflow-hidden bg-gray-900 p-8 md:p-12 flex flex-col md:flex-row items-center justify-between gap-6">
          <div>
            <div className="text-xs font-bold text-yellow-400 uppercase tracking-widest mb-3">
              Xüsusi müştərilər üçün
            </div>
            <h2 className="text-2xl md:text-3xl font-extrabold text-white mb-2">
              Korporativ sifarişlər üçün xüsusi qiymətlər
            </h2>
            <p className="text-gray-400 text-sm max-w-md">
              Şirkətiniz üçün toplu sifarişlər, xüsusi dizayn və daimi müqavilə ilə
              gəlir. Bizimlə əlaqə saxlayın.
            </p>
          </div>
          <Link
            to="/korporativ"
            className="shrink-0 bg-[#65a30d] text-white font-bold px-8 py-3.5 rounded-lg text-sm flex items-center gap-2 whitespace-nowrap"
          >
            Ətraflı məlumat <ArrowRight size={15} />
          </Link>
        </section>

        {/* Discounted Products */}
        {discounted.length > 0 && (
          <section>
            <SectionHeader
              icon={<Tag size={20} className="text-red-500" />}
              title="Endirimli məhsullar"
              subtitle="Tələsin, müddət məhduddur!"
              link="/kampanyalar"
              badge="HOT"
            />
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
              {discounted.map((product) => (
                <ProductCard key={product.id} product={product} />
              ))}
            </div>
          </section>
        )}

        {/* Featured */}
        <section>
          <SectionHeader
            icon={<Flame size={20} className="text-yellow-500" />}
            title="Redaksiyanın seçimi"
            subtitle="Tövsiyə etdiyimiz ən gözəl məhsullar"
          />
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
            {featured.map((product) => (
              <ProductCard key={product.id} product={product} size="large" />
            ))}
          </div>
        </section>

        {/* All Products */}
        <section>
          <SectionHeader
            icon={<Grid3X3 size={20} className="text-gray-500" />}
            title="Bütün məhsullar"
            subtitle="Bütün gül və hədiyyə çeşidlərimizi kəşf edin"
          />
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {products.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        </section>

        {/* Social Proof + About */}
        <section className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <div className="bg-white rounded-xl p-6 border border-gray-200">
            <h3 className="text-lg font-extrabold text-gray-900 mb-3">
              Azərbaycanın ən etibarlı gül platforması
            </h3>
            <p className="text-sm text-gray-500 leading-relaxed">
              Gülora ölkə üzrə eyni gün çatdırılma təklif edən onlayn gül və hədiyyə
              mağazasıdır. 500-dən çox çiçək çeşidi, xüsusi günlər üçün hədiyyə
              dəstləri və qourmet ləzzətlərlə sevdiklərinizi sevindirin. Təhlükəsiz
              ödəniş, pulsuz çatdırılma və təzə çiçək zəmanəti ilə alış-verişiniz
              həmişə etibarlıdır.
            </p>
            <Link
              to="/haqqimizda"
              className="inline-flex items-center gap-1.5 mt-4 text-sm text-[#65a30d] font-semibold"
            >
              Haqqımızda daha çox <ArrowRight size={14} />
            </Link>
          </div>

          <div className="bg-white rounded-xl p-6 border border-gray-200">
            <h3 className="text-lg font-extrabold text-gray-900 mb-4">
              Müştərilərimiz nə deyir?
            </h3>
            <div className="space-y-3">
              {[
                { name: "Aytən M.", text: "Eyni gün çatdırıldı, güllar çox təzə idi. Dostuma hədiyyə etdim, çox sevindi!", rating: 5 },
                { name: "Rauf K.", text: "Korporativ sifarişimizi vaxtında hazırladılar. Çox peşəkar komanda.", rating: 5 },
                { name: "Günel A.", text: "Dizayn çox gözəl idi. Qiymətlər ədalətli, xidmət əla. Tövsiyə edirəm!", rating: 5 },
              ].map((review, i) => (
                <div key={i} className="flex gap-3 border-b border-gray-100 pb-3 last:border-0 last:pb-0">
                  <div className="w-8 h-8 rounded-full bg-[#65a30d]/10 flex items-center justify-center shrink-0 text-sm font-bold text-[#65a30d]">
                    {review.name[0]}
                  </div>
                  <div>
                    <div className="flex items-center gap-1.5 mb-0.5">
                      <span className="text-xs font-bold text-gray-900">{review.name}</span>
                      <span className="text-yellow-400 text-xs">{"★".repeat(review.rating)}</span>
                    </div>
                    <p className="text-xs text-gray-500 leading-relaxed">{review.text}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Newsletter CTA */}
        <section className="bg-[#65a30d] rounded-xl p-8 text-center text-white">
          <h2 className="text-xl md:text-2xl font-extrabold mb-2">
            Kampaniyalardan ilk siz xəbər tutun
          </h2>
          <p className="text-rose-100 text-sm mb-6 max-w-md mx-auto">
            E-poçtunuzu daxil edin, xüsusi endirim kodunu anında alın.
          </p>
          <div className="flex flex-col sm:flex-row gap-2 max-w-md mx-auto">
            <input
              type="email"
              placeholder="E-poçt ünvanınız"
              className="flex-1 px-4 py-3 rounded-lg text-sm text-gray-900 outline-none border-0"
            />
            <button
              type="button"
              className="bg-gray-900 text-white font-bold px-6 py-3 rounded-lg text-sm whitespace-nowrap"
            >
              Abunə ol
            </button>
          </div>
          <p className="text-rose-200 text-xs mt-3">Spam göndərmirik. İstədiyiniz zaman abunəlikdən çıxa bilərsiniz.</p>
        </section>

      </div>
    </div>
  );
}
