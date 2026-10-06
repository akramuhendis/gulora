import { useState, useEffect, useMemo } from "react";
import { useParams, Link, useLocation } from "react-router-dom";
import { SlidersHorizontal, ChevronDown, X } from "lucide-react";
import {
  getProductsByCategory,
  categories as fallbackCategories,
  products as fallbackProducts,
} from "../data/products";
import type { Category, Product } from "../data/products";
import ProductCard from "../components/ProductCard";
import { usePageMeta } from "../hooks/usePageMeta";
import { categoryApi, productApi } from "../services/api";

type SortOption = "popular" | "price-asc" | "price-desc" | "discount";

const MAX_FILTER_PRICE = 500;

export default function CategoryPage() {
  const { slug } = useParams<{ slug: string }>();
  const location = useLocation();
  const pathname = location.pathname.replace(/\/$/, "") || "/";
  const isAllProducts = pathname === "/mehsullar";

  const [navCats, setNavCats] = useState<Category[]>(fallbackCategories);
  const [apiProducts, setApiProducts] = useState<Product[] | undefined>(undefined);

  useEffect(() => {
    categoryApi
      .list()
      .then(setNavCats)
      .catch(() => setNavCats(fallbackCategories));
  }, []);

  const category =
    slug && !isAllProducts ? navCats.find((c) => c.slug === slug) : undefined;
  const isUnknownCategory = Boolean(slug && !isAllProducts && !category);

  const pageTitle = isAllProducts
    ? "Bütün məhsullar"
    : category?.name ?? (slug ? slug : "Məhsullar");
  usePageMeta(pageTitle, `Gülora-da ${pageTitle.toLowerCase()} — Eyni gün çatdırılma, ən yaxşı qiymətlər.`);

  const [sort, setSort] = useState<SortOption>("popular");
  const [filterOpen, setFilterOpen] = useState(false);
  const [sameDayOnly, setSameDayOnly] = useState(false);
  const [discountOnly, setDiscountOnly] = useState(false);
  const [priceRange, setPriceRange] = useState<[number, number]>([0, MAX_FILTER_PRICE]);

  const staticProducts = useMemo(
    () =>
      isAllProducts ? [] : slug ? getProductsByCategory(slug) : fallbackProducts,
    [isAllProducts, slug]
  );

  useEffect(() => {
    if (isUnknownCategory) return;
    let cancelled = false;
    const sortBy =
      sort === "price-asc"
        ? "price_asc"
        : sort === "price-desc"
          ? "price_desc"
          : sort === "discount"
            ? "newest"
            : "rating";

    (async () => {
      try {
        if (isAllProducts) {
          const rows = await productApi.list({ page: 1, pageSize: 120, sortBy });
          if (!cancelled) setApiProducts(rows);
          return;
        }
        if (slug) {
          const rows = await productApi.byCategorySlug(slug, 1, 120);
          if (!cancelled)
            setApiProducts(rows.length > 0 ? rows : getProductsByCategory(slug));
          return;
        }
        if (!cancelled) setApiProducts(undefined);
      } catch {
        if (!cancelled) {
          if (isAllProducts) setApiProducts([]);
          else setApiProducts(undefined);
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [slug, isAllProducts, sort, isUnknownCategory, staticProducts]);

  const rawProducts = apiProducts ?? staticProducts;

  let filtered = rawProducts.filter((p) => {
    if (sameDayOnly && !p.isSameDay) return false;
    if (discountOnly && !p.discount) return false;
    if (p.price < priceRange[0] || p.price > priceRange[1]) return false;
    return true;
  });

  filtered = [...filtered].sort((a, b) => {
    if (sort === "price-asc") return a.price - b.price;
    if (sort === "price-desc") return b.price - a.price;
    if (sort === "discount")
      return (b.discount || 0) - (a.discount || 0);
    return b.reviewCount - a.reviewCount;
  });

  const sortLabels: Record<SortOption, string> = {
    popular: "Ən populyar",
    "price-asc": "Qiymət: aşağıdan yuxarıya",
    "price-desc": "Qiymət: yuxarıdan aşağıya",
    discount: "Ən çox endirim",
  };

  const filtersActive =
    sameDayOnly ||
    discountOnly ||
    priceRange[0] > 0 ||
    priceRange[1] < MAX_FILTER_PRICE;

  if (isUnknownCategory) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-6">
        <nav className="flex items-center gap-2 text-sm text-gray-400 mb-4">
          <Link to="/" className="hover:text-[#65a30d]">
            Ana səhifə
          </Link>
          <span>/</span>
          <span className="text-gray-700 font-medium">Kateqoriya tapılmadı</span>
        </nav>
        <div className="text-center py-16 max-w-lg mx-auto">
          <div className="text-6xl mb-4">📂</div>
          <h1 className="text-2xl font-bold text-gray-800 mb-2">Kateqoriya mövcud deyil</h1>
          <p className="text-gray-500 text-sm mb-2">
            «{slug}» üzrə kateqoriya tapılmadı. Ünvanı yoxlayın və ya kataloqdan seçin.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-3 mt-8">
            <Link
              to="/mehsullar"
              className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl font-medium hover:bg-[#4d7c0f] transition-colors"
            >
              Bütün məhsullar
            </Link>
            <Link
              to="/"
              className="inline-block border border-gray-200 text-gray-700 px-6 py-2.5 rounded-xl font-medium hover:border-lime-300 transition-colors"
            >
              Ana səhifə
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const chipActiveAll = isAllProducts;
  const chipActiveClass = "bg-[#65a30d] text-white border-[#65a30d]";
  const chipIdleClass = "bg-white text-gray-600 border-gray-200 hover:border-lime-300";

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-4">
        <Link to="/" className="hover:text-[#65a30d]">Ana səhifə</Link>
        <span>/</span>
        <span className="text-gray-700 font-medium">
          {isAllProducts ? "Bütün məhsullar" : category?.name ?? "Bütün məhsullar"}
        </span>
      </nav>

      <div className="mb-6">
        {(category || isAllProducts) && (
          <div className="flex items-center gap-3 mb-2">
            <span className="text-3xl">{category?.icon ?? "🛍️"}</span>
            <h1 className="text-2xl font-bold text-gray-800">
              {isAllProducts ? "Bütün məhsullar" : category!.name}
            </h1>
          </div>
        )}
        <p className="text-sm text-gray-500">
          {filtered.length} məhsul göstərilir
        </p>
      </div>

      <div className="flex gap-2 overflow-x-auto pb-2 mb-6 scrollbar-hide">
        <Link
          to="/mehsullar"
          className={`shrink-0 flex items-center gap-1.5 px-4 py-2 rounded-full border text-sm font-medium transition-all ${
            chipActiveAll ? chipActiveClass : chipIdleClass
          }`}
        >
          Hamısı
        </Link>
        {navCats.map((cat) => (
          <Link
            key={cat.id}
            to={`/kategori/${cat.slug}`}
            className={`shrink-0 flex items-center gap-1.5 px-4 py-2 rounded-full border text-sm font-medium transition-all ${
              !isAllProducts && cat.slug === slug ? chipActiveClass : chipIdleClass
            }`}
          >
            <span>{cat.icon}</span>
            {cat.name}
          </Link>
        ))}
      </div>

      <div className="flex gap-6">
        <aside className="hidden lg:block w-56 shrink-0">
          <div className="bg-white rounded-2xl border border-gray-100 p-4 space-y-5 sticky top-24">
            <h3 className="font-semibold text-gray-800">Filtrlər</h3>

            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-2">Çatdırılma</h4>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={sameDayOnly}
                  onChange={(e) => setSameDayOnly(e.target.checked)}
                  className="accent-[#65a30d] w-4 h-4"
                />
                <span className="text-sm text-gray-600">Eyni gün çatdırılma</span>
              </label>
            </div>

            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-2">Kampaniya</h4>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={discountOnly}
                  onChange={(e) => setDiscountOnly(e.target.checked)}
                  className="accent-[#65a30d] w-4 h-4"
                />
                <span className="text-sm text-gray-600">Endirimli məhsullar</span>
              </label>
            </div>

            <div>
              <h4 className="text-sm font-medium text-gray-700 mb-3">
                Qiymət aralığı (₼)
              </h4>
              <div className="flex gap-2">
                <input
                  type="number"
                  value={priceRange[0]}
                  onChange={(e) =>
                    setPriceRange([Number(e.target.value), priceRange[1]])
                  }
                  className="w-full border border-gray-200 rounded-lg px-2 py-1.5 text-sm outline-none focus:border-[#65a30d]"
                  placeholder="Min"
                />
                <input
                  type="number"
                  value={priceRange[1]}
                  onChange={(e) =>
                    setPriceRange([priceRange[0], Number(e.target.value)])
                  }
                  className="w-full border border-gray-200 rounded-lg px-2 py-1.5 text-sm outline-none focus:border-[#65a30d]"
                  placeholder="Maks"
                />
              </div>
            </div>

            {(sameDayOnly || discountOnly || priceRange[0] > 0 || priceRange[1] < MAX_FILTER_PRICE) && (
              <button
                onClick={() => {
                  setSameDayOnly(false);
                  setDiscountOnly(false);
                  setPriceRange([0, MAX_FILTER_PRICE]);
                }}
                className="w-full text-sm text-red-500 flex items-center justify-center gap-1 hover:text-red-600 transition-colors"
              >
                <X size={13} /> Filtrləri təmizlə
              </button>
            )}
          </div>
        </aside>

        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-3 mb-4">
            <button
              onClick={() => setFilterOpen(!filterOpen)}
              className="lg:hidden flex items-center gap-1.5 px-3 py-2 border border-gray-200 rounded-xl text-sm text-gray-600 hover:border-lime-300"
            >
              <SlidersHorizontal size={15} />
              Filtr
            </button>
            <div className="ml-auto relative">
              <select
                value={sort}
                onChange={(e) => setSort(e.target.value as SortOption)}
                className="appearance-none bg-white border border-gray-200 rounded-xl px-3 py-2 pr-8 text-sm text-gray-700 outline-none focus:border-[#65a30d] transition-colors cursor-pointer"
              >
                {Object.entries(sortLabels).map(([val, label]) => (
                  <option key={val} value={val}>
                    {label}
                  </option>
                ))}
              </select>
              <ChevronDown size={14} className="absolute right-2.5 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" />
            </div>
          </div>

          {filterOpen && (
            <div className="lg:hidden bg-white rounded-2xl border border-gray-100 p-4 mb-4 space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="font-semibold text-gray-800">Filtrlər</h3>
                <button type="button" onClick={() => setFilterOpen(false)}>
                  <X size={16} className="text-gray-400" />
                </button>
              </div>
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" checked={sameDayOnly} onChange={(e) => setSameDayOnly(e.target.checked)} className="accent-[#65a30d]" />
                <span className="text-sm">Eyni gün çatdırılma</span>
              </label>
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" checked={discountOnly} onChange={(e) => setDiscountOnly(e.target.checked)} className="accent-[#65a30d]" />
                <span className="text-sm">Endirimli məhsullar</span>
              </label>
            </div>
          )}

          {filtered.length === 0 ? (
            <div className="text-center py-20 max-w-md mx-auto px-2">
              {category && rawProducts.length === 0 ? (
                <>
                  <div className="text-5xl mb-4">🌱</div>
                  <h3 className="text-lg font-semibold text-gray-600 mb-2">
                    Tezliklə bu kateqoriyada məhsullar olacaq
                  </h3>
                  <p className="text-gray-400 text-sm mb-6">
                    Hazırda bu bölmədə məhsul yoxdur. Digər kateqoriyalara baxın və ya bütün
                    kataloqa keçin.
                  </p>
                  <Link
                    to="/mehsullar"
                    className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl text-sm font-medium hover:bg-[#4d7c0f] transition-colors"
                  >
                    Bütün məhsullar
                  </Link>
                </>
              ) : rawProducts.length > 0 && filtersActive ? (
                <>
                  <div className="text-5xl mb-4">🔍</div>
                  <h3 className="text-lg font-semibold text-gray-600 mb-2">
                    Filtrə uyğun məhsul yoxdur
                  </h3>
                  <p className="text-gray-400 text-sm">
                    Filtrləri dəyişməyi və ya təmizləməyi sınayın.
                  </p>
                </>
              ) : (
                <>
                  <div className="text-5xl mb-4">🔍</div>
                  <h3 className="text-lg font-semibold text-gray-600 mb-2">
                    Məhsul tapılmadı
                  </h3>
                  <p className="text-gray-400 text-sm">
                    Filtrləri dəyişməyi sınayın.
                  </p>
                </>
              )}
            </div>
          ) : (
            <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4">
              {filtered.map((product) => (
                <ProductCard key={product.id} product={product} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
