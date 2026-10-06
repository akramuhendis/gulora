import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ArrowRight, TrendingUp, Tag, Flame, Grid3X3, Megaphone } from "lucide-react";
import HeroBanner from "../components/HeroBanner";
import { HomeQuickCategoryChips } from "../components/HomeQuickCategoryChips";
import ProductCard from "../components/ProductCard";
import {
  categories as fallbackCategories,
  getBestSellers,
  getDiscountedProducts,
  getFeaturedProducts,
  products as fallbackProducts,
} from "../data/products";
import type { Category, Product } from "../data/products";
import {
  categoryApi,
  productApi,
  sellerShopApi,
  type SellerCampaignPlanHomeWire,
} from "../services/api";
import { usePageMeta } from "../hooks/usePageMeta";

function campaignKindShort(kind: string): string {
  const k = kind.trim().toLowerCase();
  if (k === "shipping") return "Çatdırılma";
  return "Faiz endirimi";
}

function formatCampaignEnd(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString("az-AZ", { day: "numeric", month: "short" });
}

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
  usePageMeta(
    "Ana səhifə",
    "Təzə güllər, hədiyyə setləri və eyni gün çatdırılma — Gülora onlayn mağaza."
  );

  const [categories, setCategories] = useState<Category[]>(fallbackCategories);
  const [bestSellers, setBestSellers] = useState<Product[]>(() =>
    getBestSellers().slice(0, 8)
  );
  const [discounted, setDiscounted] = useState<Product[]>(() =>
    getDiscountedProducts().slice(0, 6)
  );
  const [featured, setFeatured] = useState<Product[]>(() =>
    getFeaturedProducts().slice(0, 4)
  );
  const [allProducts, setAllProducts] = useState<Product[]>(fallbackProducts);
  const [storeCampaigns, setStoreCampaigns] = useState<SellerCampaignPlanHomeWire[]>([]);

  useEffect(() => {
    let cancelled = false;
    sellerShopApi
      .listActiveCampaignsForHome()
      .then((rows) => {
        if (!cancelled) setStoreCampaigns(rows);
      })
      .catch(() => {
        if (!cancelled) setStoreCampaigns([]);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    categoryApi
      .list()
      .then(setCategories)
      .catch(() => setCategories(fallbackCategories));
  }, []);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      const settled = await Promise.allSettled([
        productApi.bestSellers(8),
        productApi.discounted(12),
        productApi.featured(4),
        productApi.list({ page: 1, pageSize: 48 }),
      ]);
      if (cancelled) return;
      const [bestR, discR, featR, allR] = settled;

      setBestSellers(
        bestR.status === "fulfilled" && bestR.value.length > 0
          ? bestR.value.slice(0, 8)
          : getBestSellers().slice(0, 8)
      );
      setDiscounted(
        discR.status === "fulfilled" && discR.value.length > 0
          ? discR.value.slice(0, 6)
          : getDiscountedProducts().slice(0, 6)
      );
      setFeatured(
        featR.status === "fulfilled" && featR.value.length > 0
          ? featR.value.slice(0, 4)
          : getFeaturedProducts().slice(0, 4)
      );
      setAllProducts(
        allR.status === "fulfilled" && allR.value.length > 0 ? allR.value : fallbackProducts
      );
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const campaignSpotlight = storeCampaigns[0];

  return (
    <div className="bg-gray-50">
      <HeroBanner />

      <div className="max-w-[1400px] mx-auto px-4 py-10 space-y-14">
        <HomeQuickCategoryChips />
        {storeCampaigns.length > 0 && (
          <section aria-labelledby="home-active-campaigns-heading">
            <div className="flex items-end justify-between gap-4 mb-4">
              <div>
                <div className="flex items-center gap-2.5 mb-1">
                  <Megaphone size={20} className="text-red-500" aria-hidden />
                  <h2 id="home-active-campaigns-heading" className="text-xl md:text-2xl font-extrabold text-gray-900">
                    Aktiv kampaniyalar
                  </h2>
                </div>
                <p className="text-sm text-gray-500">
                  Satıcı mağazalarından cari endirim və təkliflər
                </p>
              </div>
              <Link
                to="/kampanyalar"
                className="shrink-0 flex items-center gap-1 text-sm text-[#65a30d] font-semibold border border-[#65a30d] px-3 py-1.5 rounded-lg"
              >
                Hamısına bax <ArrowRight size={14} />
              </Link>
            </div>
            <div className="flex gap-4 overflow-x-auto pb-1 snap-x snap-mandatory">
              {storeCampaigns.map((c, idx) => (
                <Link
                  key={`${c.storeSlug}-${c.name}-${idx}`}
                  to={`/magaza/${encodeURIComponent(c.storeSlug)}`}
                  className="snap-start shrink-0 w-[min(100%,280px)] rounded-xl border border-amber-200 bg-gradient-to-br from-amber-50 to-white p-4 shadow-sm hover:border-[#65a30d] transition-colors"
                >
                  <p className="text-xs font-semibold text-amber-800 uppercase tracking-wide mb-1">
                    {c.storeName}
                  </p>
                  <h3 className="text-lg font-extrabold text-gray-900 leading-tight mb-1">{c.name}</h3>
                  <p className="text-sm text-gray-600 mb-2">
                    {campaignKindShort(c.kind)} — %{c.percent}
                  </p>
                  <p className="text-xs text-gray-400">
                    Bitmə təxmini: {formatCampaignEnd(c.endsAt)}
                  </p>
                </Link>
              ))}
            </div>
          </section>
        )}

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
              {campaignSpotlight ? (
                <>
                  <h3 className="text-2xl font-extrabold mb-1 line-clamp-2">{campaignSpotlight.name}</h3>
                  <p className="text-purple-100 text-sm mb-5 line-clamp-2">
                    {campaignSpotlight.storeName} — {campaignKindShort(campaignSpotlight.kind)},{" "}
                    {campaignSpotlight.percent}%
                  </p>
                  <Link
                    to={`/magaza/${encodeURIComponent(campaignSpotlight.storeSlug)}`}
                    className="inline-flex items-center gap-2 bg-white text-purple-600 text-sm font-bold px-5 py-2.5 rounded-lg"
                  >
                    Mağazaya keç <ArrowRight size={14} />
                  </Link>
                </>
              ) : (
                <>
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
                </>
              )}
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
            {allProducts.map((product) => (
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
        <section className="relative overflow-hidden rounded-2xl bg-linear-to-br from-[#2d5016] via-[#4d7c0f] to-[#84cc16] px-8 py-10 md:px-12 md:py-12 text-center text-white shadow-lg">
          <div
            className="pointer-events-none absolute -right-16 -top-16 h-56 w-56 rounded-full bg-white/10 blur-3xl"
            aria-hidden
          />
          <div
            className="pointer-events-none absolute -bottom-12 -left-12 h-44 w-44 rounded-full bg-lime-200/15 blur-3xl"
            aria-hidden
          />
          <div className="relative z-10">
            <h2 className="text-xl md:text-2xl font-extrabold mb-2">
              Kampaniyalardan ilk siz xəbər tutun
            </h2>
            <p className="text-sm text-lime-50/90 mb-6 max-w-md mx-auto">
              E-poçtunuzu daxil edin, xüsusi endirim kodunu anında alın.
            </p>
            <div className="flex flex-col sm:flex-row gap-2.5 max-w-md mx-auto">
              <input
                type="email"
                placeholder="E-poçt ünvanınız"
                className="flex-1 rounded-xl border border-white/25 bg-white/95 px-4 py-3 text-sm text-gray-900 placeholder:text-gray-400 outline-none focus:border-white/60 focus:ring-2 focus:ring-white/30"
              />
              <button
                type="button"
                className="rounded-xl bg-white px-6 py-3 text-sm font-bold text-[#3f6212] whitespace-nowrap shadow-sm hover:bg-lime-50"
              >
                Abunə ol
              </button>
            </div>
            <p className="text-xs text-lime-100/75 mt-3">
              Spam göndərmirik. İstədiyiniz zaman abunəlikdən çıxa bilərsiniz.
            </p>
          </div>
        </section>

      </div>
    </div>
  );
}
