import { useState, useEffect, startTransition } from "react";
import { useParams, Link } from "react-router-dom";
import {
  ShoppingCart, Heart, Star, Truck, Clock, Shield, RotateCcw,
  ChevronLeft, ChevronRight, Plus, Minus, Share2, CheckCircle, ThumbsUp,
  BadgeCheck, Store,
} from "lucide-react";
import { getProductBySlug, products as fallbackProducts } from "../data/products";
import type { Product } from "../data/products";
import { useCart } from "../hooks/useCart";
import { useFavorites } from "../hooks/useFavorites";
import { useAuth } from "../hooks/useAuth";
import { useToast } from "../hooks/useToast";
import ProductCard from "../components/ProductCard";
import { formatMoney } from "../lib/format";
import { usePageMeta } from "../hooks/usePageMeta";
import { productApi, reviewApi, ApiError } from "../services/api";

interface ProductVariant {
  label: string;
  priceAdjust: number;
}

interface ProductAddon {
  id: string;
  name: string;
  price: number;
  icon: string;
}

const PRODUCT_VARIANTS: Record<string, ProductVariant[]> = {
  "gul-buketi": [
    { label: "Mini (5 ədəd)", priceAdjust: -15 },
    { label: "Standart (9–11 ədəd)", priceAdjust: 0 },
    { label: "Böyük (15 ədəd)", priceAdjust: 20 },
    { label: "Premium (25 ədəd)", priceAdjust: 45 },
  ],
};

/** API ayrıca başlıq + mətn qaytarır; köhnə rəylərdə ikisi eyni olanda təkrar göstərməmək üçün. */
function reviewTextFromParts(title: string | null | undefined, body: string | null | undefined): string {
  const t = (title ?? "").trim();
  const b = (body ?? "").trim();
  if (!t) return b;
  if (!b) return t;
  if (t === b) return t;
  if (b.startsWith(t)) return b;
  return `${t} — ${b}`;
}

const ADDONS: ProductAddon[] = [
  { id: "chocolate", name: "Ferrero Rocher (8 ədəd)", price: 18, icon: "🍫" },
  { id: "card", name: "Xüsusi kart (yazılı)", price: 5, icon: "💌" },
  { id: "balloon", name: "Balon dəsti (3 ədəd)", price: 12, icon: "🎈" },
  { id: "plushy", name: "Yumşaq oyuncaq (30 sm)", price: 25, icon: "🧸" },
  { id: "candle", name: "Aromatik şam", price: 15, icon: "🕯️" },
];

interface Review {
  id: string;
  productId: string;
  userName: string;
  rating: number;
  text: string;
  date: string;
  helpfulCount: number;
  verified?: boolean;
  images?: string[];
}

export default function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const { addToCart } = useCart();
  const { isFavorite, toggleFavorite } = useFavorites();
  const { user } = useAuth();
  const { showToast } = useToast();

  const [product, setProduct] = useState<Product | null>(null);
  const [loadingProduct, setLoadingProduct] = useState(true);

  useEffect(() => {
    if (!slug) {
      startTransition(() => {
        setProduct(null);
        setLoadingProduct(false);
      });
      return;
    }
    let cancelled = false;
    startTransition(() => {
      setLoadingProduct(true);
    });
    productApi
      .bySlug(slug)
      .then((p) => {
        if (cancelled) return;
        setProduct(p ?? getProductBySlug(slug) ?? null);
      })
      .catch(() => {
        if (!cancelled) setProduct(getProductBySlug(slug) ?? null);
      })
      .finally(() => {
        if (!cancelled) setLoadingProduct(false);
      });
    return () => {
      cancelled = true;
    };
  }, [slug]);

  usePageMeta(
    product?.name ?? "Məhsul",
    product ? `${product.name} — ${product.description.slice(0, 120)}` : undefined
  );

  const [selectedImg, setSelectedImg] = useState(0);
  const [quantity, setQuantity] = useState(1);
  const [addedToCart, setAddedToCart] = useState(false);
  const [activeTab, setActiveTab] = useState<"desc" | "review" | "info">("desc");

  const favorited = product ? isFavorite(product.id) : false;

  const variants = product ? (PRODUCT_VARIANTS[product.categorySlug] ?? null) : null;
  const [selectedVariant, setSelectedVariant] = useState("");

  useEffect(() => {
    startTransition(() => {
      setSelectedImg(0);
    });
  }, [product?.id]);

  useEffect(() => {
    startTransition(() => {
      if (!product) {
        setSelectedVariant("");
        return;
      }
      const v = PRODUCT_VARIANTS[product.categorySlug];
      setSelectedVariant(v?.[1]?.label ?? v?.[0]?.label ?? "");
    });
  }, [product]);

  const variantAdj = variants?.find((v) => v.label === selectedVariant)?.priceAdjust ?? 0;

  const [selectedAddons, setSelectedAddons] = useState<Set<string>>(new Set());
  const addonTotal = ADDONS.filter((a) => selectedAddons.has(a.id)).reduce((s, a) => s + a.price, 0);
  const effectivePrice = (product ? product.price + variantAdj : 0) + addonTotal;

  const [reviews, setReviews] = useState<Review[]>([]);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewHover, setReviewHover] = useState(0);
  const [reviewText, setReviewText] = useState("");
  const [reviewName, setReviewName] = useState(user ? `${user.firstName} ${user.lastName[0]}.` : "");
  const [reviewSubmitted, setReviewSubmitted] = useState(false);
  const [reviewError, setReviewError] = useState("");

  const [related, setRelated] = useState<Product[]>([]);

  useEffect(() => {
    startTransition(() => {
      if (!product?.id) {
        setReviews([]);
      }
    });
    if (!product?.id) {
      return;
    }
    let cancelled = false;
    reviewApi
      .list(product.id, 1, 40)
      .then((rows) => {
        if (cancelled) return;
        setReviews(
          rows.map((r) => ({
            id: r.id,
            productId: r.productId,
            userName: r.reviewerName ?? "Alıcı",
            rating: r.rating,
            text: reviewTextFromParts(r.title, r.body),
            date: r.createdAt.slice(0, 10),
            helpfulCount: 0,
            verified: Boolean(r.orderId),
          }))
        );
      })
      .catch(() => {
        if (!cancelled) setReviews([]);
      });
    return () => {
      cancelled = true;
    };
  }, [product?.id]);

  useEffect(() => {
    if (!product) {
      startTransition(() => {
        setRelated([]);
      });
      return;
    }
    let cancelled = false;
    if (product.backendCategoryId) {
      productApi
        .list({ categoryId: product.backendCategoryId, page: 1, pageSize: 12 })
        .then((rows) => {
          if (cancelled) return;
          setRelated(rows.filter((p) => p.slug !== product.slug).slice(0, 4));
        })
        .catch(() => {
          if (!cancelled)
            setRelated(
              fallbackProducts
                .filter(
                  (p) =>
                    p.categorySlug === product.categorySlug && p.id !== product.id
                )
                .slice(0, 4)
            );
        });
    } else {
      startTransition(() => {
        setRelated(
          fallbackProducts
            .filter(
              (p) =>
                p.categorySlug === product.categorySlug && p.id !== product.id
            )
            .slice(0, 4)
        );
      });
    }
    return () => {
      cancelled = true;
    };
  }, [product]);

  useEffect(() => {
    startTransition(() => {
      if (user) setReviewName(`${user.firstName} ${user.lastName[0]}.`);
    });
  }, [user]);

  if (loadingProduct) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 text-center text-gray-500 text-sm">
        Məhsul yüklənir…
      </div>
    );
  }

  if (!product) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 text-center">
        <div className="text-6xl mb-4">🌸</div>
        <h2 className="text-2xl font-bold text-gray-700 mb-2">Məhsul tapılmadı</h2>
        <p className="text-gray-400 mb-6">Axtardığınız məhsul mövcud deyil.</p>
        <Link to="/" className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl font-medium hover:bg-[#4d7c0f] transition-colors">
          Ana səhifəyə qayıt
        </Link>
      </div>
    );
  }

  const handleAddToCart = () => {
    const addonsArr = ADDONS.filter((a) => selectedAddons.has(a.id)).map((a) => ({ name: a.name, price: a.price }));
    addToCart(
      { ...product, price: product.price + variantAdj },
      quantity,
      selectedVariant || undefined,
      addonsArr.length > 0 ? addonsArr : undefined
    );
    setAddedToCart(true);
    setTimeout(() => setAddedToCart(false), 2000);
  };

  const toggleAddon = (id: string) => {
    setSelectedAddons((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const handleSubmitReview = async () => {
    setReviewError("");
    if (!user) {
      setReviewError("Rəy yazmaq üçün daxil olun.");
      return;
    }
    if (!reviewName.trim()) {
      setReviewError("Ad tələb olunur.");
      return;
    }
    if (reviewText.trim().length < 10) {
      setReviewError("Rəy ən azı 10 simvol olmalıdır.");
      return;
    }
    try {
      const prevLen = reviews.length;
      await reviewApi.create(product.id, {
        rating: reviewRating,
        title: null,
        body: reviewText.trim(),
        orderId: null,
      });
      const rows = await reviewApi.list(product.id, 1, 40);
      setReviews(
        rows.map((r) => ({
          id: r.id,
          productId: r.productId,
          userName: r.reviewerName ?? "Alıcı",
          rating: r.rating,
          text: reviewTextFromParts(r.title, r.body),
          date: r.createdAt.slice(0, 10),
          helpfulCount: 0,
          verified: Boolean(r.orderId),
        }))
      );
      showToast(
        rows.length > prevLen
          ? "Rəy dərc olundu."
          : "Rəy qəbul edildi — moderator təsdiqindən sonra görünəcək.",
        "success"
      );
      setReviewText("");
      setReviewSubmitted(true);
      setTimeout(() => setReviewSubmitted(false), 3000);
    } catch (e) {
      setReviewError(e instanceof ApiError ? e.message : "Rəy göndərilmədi.");
    }
  };

  const handleHelpful = (id: string) => {
    setReviews((prev) =>
      prev.map((r) =>
        r.id === id ? { ...r, helpfulCount: r.helpfulCount + 1 } : r
      )
    );
  };

  const allReviews = reviews;
  const avgRating =
    allReviews.length > 0
      ? allReviews.reduce((s, r) => s + r.rating, 0) / allReviews.length
      : product.rating;

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-6">
        <Link to="/" className="hover:text-[#65a30d]">Ana səhifə</Link>
        <span>/</span>
        <Link to={`/kategori/${product.categorySlug}`} className="hover:text-[#65a30d]">{product.category}</Link>
        <span>/</span>
        <span className="text-gray-600 truncate max-w-48">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 lg:gap-12">
        {/* Images */}
        <div>
          <div className="relative rounded-2xl overflow-hidden bg-gray-50 aspect-square mb-3">
            <img src={product.images[selectedImg]} alt={product.name} className="w-full h-full object-cover" />
            {product.discount && (
              <span className="absolute top-4 left-4 bg-red-500 text-white text-sm font-bold px-3 py-1 rounded-full">
                %{product.discount} endirim
              </span>
            )}
            {product.images.length > 1 && (
              <>
                <button type="button"
                  onClick={() => setSelectedImg((p) => (p - 1 + product.images.length) % product.images.length)}
                  className="absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 bg-white/90 rounded-full flex items-center justify-center hover:bg-white transition-colors border border-gray-100">
                  <ChevronLeft size={18} />
                </button>
                <button type="button"
                  onClick={() => setSelectedImg((p) => (p + 1) % product.images.length)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 bg-white/90 rounded-full flex items-center justify-center hover:bg-white transition-colors border border-gray-100">
                  <ChevronRight size={18} />
                </button>
              </>
            )}
          </div>
          {product.images.length > 1 && (
            <div className="flex gap-2">
              {product.images.map((img, i) => (
                <button key={i} type="button" onClick={() => setSelectedImg(i)}
                  className={`w-20 h-20 rounded-xl overflow-hidden border-2 transition-all ${selectedImg === i ? "border-[#65a30d]" : "border-gray-200 hover:border-gray-300"}`}>
                  <img src={img} alt="" className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Info */}
        <div className="flex flex-col gap-4">
          <div>
            <div className="flex items-center justify-between mb-2">
              <Link to={`/kategori/${product.categorySlug}`} className="text-sm text-[#65a30d] font-medium hover:underline">
                {product.category}
              </Link>
              <button type="button" onClick={() => void toggleFavorite(product)}
                aria-label={favorited ? "Seçilmişlərdən çıxar" : "Seçilmişlərə əlavə et"}
                className={`p-2 rounded-full border transition-all ${favorited ? "bg-lime-50 border-lime-300 text-lime-500" : "border-gray-200 text-gray-400 hover:border-lime-300 hover:text-lime-400"}`}>
                <Heart size={18} fill={favorited ? "currentColor" : "none"} />
              </button>
            </div>
            <h1 className="text-2xl md:text-3xl font-bold text-gray-800 mb-3">{product.name}</h1>

            {product.sellerShop && (
              <Link
                to={`/magaza/${encodeURIComponent(product.sellerShop.storeSlug)}`}
                className="mb-4 flex items-center gap-3 rounded-xl border border-gray-100 bg-gray-50/80 px-3 py-2.5 transition-colors hover:border-lime-200 hover:bg-lime-50/50"
              >
                <span className="flex h-11 w-11 shrink-0 items-center justify-center overflow-hidden rounded-lg bg-white shadow-sm">
                  {product.sellerShop.logoUrl ? (
                    <img
                      src={product.sellerShop.logoUrl}
                      alt=""
                      className="h-full w-full object-cover"
                    />
                  ) : (
                    <Store className="h-6 w-6 text-[#65a30d]" aria-hidden />
                  )}
                </span>
                <span className="min-w-0 text-left">
                  <span className="block text-xs font-medium uppercase tracking-wide text-gray-400">
                    Satıcı mağazası
                  </span>
                  <span className="font-semibold text-gray-800 truncate">
                    {product.sellerShop.storeName}
                  </span>
                </span>
                <span className="ml-auto text-sm font-medium text-[#65a30d]">Mağazaya keç →</span>
              </Link>
            )}

            <div className="flex items-center gap-2 mb-4">
              <div className="flex">
                {[1, 2, 3, 4, 5].map((s) => (
                  <Star key={s} size={16} className={s <= Math.round(avgRating) ? "text-yellow-400" : "text-gray-200"} fill={s <= Math.round(avgRating) ? "currentColor" : "none"} />
                ))}
              </div>
              <span className="text-sm font-semibold text-gray-700">{avgRating.toFixed(1)}</span>
              <span className="text-sm text-gray-400">({(product.reviewCount + reviews.length).toLocaleString("az-AZ")} rəy)</span>
            </div>

            <div className="flex items-baseline gap-3 mb-4 flex-wrap">
              <span className="text-3xl font-bold text-gray-900">{formatMoney(effectivePrice)}</span>
              {product.originalPrice && variantAdj === 0 && (
                <span className="text-lg text-gray-400 line-through">{formatMoney(product.originalPrice)}</span>
              )}
              {product.discount && variantAdj === 0 && (
                <span className="bg-red-100 text-red-600 text-sm font-bold px-2 py-0.5 rounded-full">%{product.discount} qənaət</span>
              )}
              {product.sellerCampaign && variantAdj === 0 && (
                <span
                  className={`text-xs font-semibold px-2 py-0.5 rounded-full max-w-48 truncate ${
                    product.sellerCampaign.kind === "shipping"
                      ? "bg-sky-100 text-sky-800"
                      : "bg-lime-100 text-lime-800"
                  }`}
                  title={product.sellerCampaign.name}
                >
                  {product.sellerCampaign.kind === "shipping"
                    ? `Çatdırılma −${product.sellerCampaign.percent}%`
                    : product.sellerCampaign.name}
                </span>
              )}
            </div>
            {product.sellerCampaign?.minimumCartAmount != null && product.sellerCampaign.minimumCartAmount > 0 && variantAdj === 0 && (
              <p className="text-xs text-amber-800 -mt-2 mb-4">
                Bu kampaniya üçün səbət minimum {formatMoney(product.sellerCampaign.minimumCartAmount)} olmalıdır.
              </p>
            )}
          </div>

          {/* Variants */}
          {variants && (
            <div>
              <p className="text-xs font-medium text-gray-600 mb-2">Ölçü / say seçimi</p>
              <div className="flex flex-wrap gap-2">
                {variants.map((v) => (
                  <button key={v.label} type="button" onClick={() => setSelectedVariant(v.label)}
                    className={`px-3 py-1.5 rounded-xl text-xs font-semibold border transition-colors ${selectedVariant === v.label ? "border-[#65a30d] bg-lime-50 text-[#65a30d]" : "border-gray-200 text-gray-600 hover:border-lime-200"}`}>
                    {v.label}
                    {v.priceAdjust !== 0 && <span className="ml-1 opacity-70">{v.priceAdjust > 0 ? "+" : ""}{formatMoney(v.priceAdjust)}</span>}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Add-ons */}
          <div>
            <p className="text-xs font-medium text-gray-600 mb-2">Əlavə məhsullar (isteğə bağlı)</p>
            <div className="grid grid-cols-2 gap-2">
              {ADDONS.map((addon) => (
                <button key={addon.id} type="button" onClick={() => toggleAddon(addon.id)}
                  className={`flex items-center gap-2 p-2.5 rounded-xl border text-xs transition-colors text-left ${selectedAddons.has(addon.id) ? "border-[#65a30d] bg-lime-50" : "border-gray-200 hover:border-lime-200"}`}>
                  <span className="text-lg shrink-0">{addon.icon}</span>
                  <div className="min-w-0">
                    <p className="font-medium text-gray-700 leading-tight">{addon.name}</p>
                    <p className="text-[#65a30d] font-semibold">+{formatMoney(addon.price)}</p>
                  </div>
                  {selectedAddons.has(addon.id) && <CheckCircle size={14} className="text-[#65a30d] ml-auto shrink-0" />}
                </button>
              ))}
            </div>
          </div>

          {/* Delivery info */}
          <div className="bg-gray-50 rounded-xl p-4 space-y-2.5">
            <div className="flex items-center gap-3">
              <Clock size={16} className="text-green-600 shrink-0" />
              <div>
                <span className="text-sm font-semibold text-gray-800">Eyni gün çatdırılma</span>
                <p className="text-xs text-gray-500">Saat 14:00-a qədər sifariş verin, bu gün çatdırılsın</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Truck size={16} className="text-blue-600 shrink-0" />
              <div>
                <span className="text-sm font-semibold text-gray-800">Pulsuz çatdırılma</span>
                <p className="text-xs text-gray-500">Bakı daxilində pulsuz çatdırılma</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <Shield size={16} className="text-purple-600 shrink-0" />
              <div>
                <span className="text-sm font-semibold text-gray-800">100% təzə zəmanət</span>
                <p className="text-xs text-gray-500">Təzə olmayan məhsulu dəyişdiririk</p>
              </div>
            </div>
          </div>

          {/* Add to cart */}
          <div className="flex items-center gap-3 flex-wrap">
            <div className="flex items-center border border-gray-200 rounded-xl overflow-hidden">
              <button type="button" onClick={() => setQuantity((q) => Math.max(1, q - 1))} className="w-10 h-11 flex items-center justify-center hover:bg-gray-50 text-gray-600 transition-colors">
                <Minus size={16} />
              </button>
              <span className="w-10 text-center text-sm font-semibold text-gray-800">{quantity}</span>
              <button type="button" onClick={() => setQuantity((q) => q + 1)} className="w-10 h-11 flex items-center justify-center hover:bg-gray-50 text-gray-600 transition-colors">
                <Plus size={16} />
              </button>
            </div>
            <button type="button" onClick={handleAddToCart}
              className={`flex-1 min-w-[160px] py-3 font-semibold rounded-xl flex items-center justify-center gap-2 transition-all ${addedToCart ? "bg-green-500 text-white" : "bg-[#65a30d] text-white hover:bg-[#4d7c0f]"}`}>
              <ShoppingCart size={18} />
              {addedToCart ? "Səbətə əlavə edildi ✓" : "Səbətə əlavə et"}
            </button>
            <button type="button" className="w-11 h-11 border border-gray-200 rounded-xl flex items-center justify-center text-gray-500 hover:text-[#65a30d] hover:border-lime-300 transition-colors">
              <Share2 size={18} />
            </button>
          </div>

          <div className="flex flex-wrap gap-2">
            {product.tags.map((tag) => (
              <span key={tag} className="text-xs bg-gray-100 text-gray-600 px-3 py-1 rounded-full">#{tag}</span>
            ))}
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="mt-12">
        <div className="border-b border-gray-200 flex gap-0 flex-wrap">
          {([
            { id: "desc", label: "Məhsul təsviri" },
            { id: "review", label: `Rəylər (${allReviews.length})` },
            { id: "info", label: "Çatdırılma və geri qaytarma" },
          ] as const).map((tab) => (
            <button key={tab.id} type="button" onClick={() => setActiveTab(tab.id)}
              className={`px-5 py-3 text-sm font-medium border-b-2 -mb-px transition-colors ${activeTab === tab.id ? "border-[#65a30d] text-[#65a30d]" : "border-transparent text-gray-500 hover:text-gray-700"}`}>
              {tab.label}
            </button>
          ))}
        </div>

        <div className="py-6">
          {activeTab === "desc" && (
            <div className="prose prose-sm max-w-none">
              <p className="text-gray-600 leading-relaxed">{product.description}</p>
              <ul className="mt-4 space-y-1.5">
                <li className="flex items-center gap-2 text-sm text-gray-600"><span className="text-green-500">✓</span> Təzə çiçək zəmanəti</li>
                <li className="flex items-center gap-2 text-sm text-gray-600"><span className="text-green-500">✓</span> Eyni gün çatdırılma (14:00-a qədər sifariş)</li>
                <li className="flex items-center gap-2 text-sm text-gray-600"><span className="text-green-500">✓</span> Diqqətlə qablaşdırılır</li>
                <li className="flex items-center gap-2 text-sm text-gray-600"><span className="text-green-500">✓</span> Anbar: {product.stock} ədəd</li>
              </ul>
            </div>
          )}

          {activeTab === "review" && (
            <div className="space-y-6">
              {/* Stats */}
              <div className="flex items-center gap-6 flex-wrap">
                <div className="text-center">
                  <div className="text-5xl font-bold text-gray-800">{avgRating.toFixed(1)}</div>
                  <div className="flex justify-center mt-1">
                    {[1, 2, 3, 4, 5].map((s) => (
                      <Star key={s} size={14} className="text-yellow-400" fill="currentColor" />
                    ))}
                  </div>
                  <div className="text-xs text-gray-500 mt-1">{(product.reviewCount + reviews.length).toLocaleString("az-AZ")} rəy</div>
                </div>
                <div className="flex-1 space-y-1.5 min-w-[200px]">
                  {[5, 4, 3, 2, 1].map((star) => {
                    const count = allReviews.filter((r) => r.rating === star).length;
                    const pct = allReviews.length > 0 ? (count / allReviews.length) * 100 : 0;
                    return (
                      <div key={star} className="flex items-center gap-2">
                        <span className="text-xs text-gray-500 w-4">{star}★</span>
                        <div className="flex-1 h-2 bg-gray-100 rounded-full overflow-hidden">
                          <div className="h-full bg-yellow-400 rounded-full" style={{ width: `${pct}%` }} />
                        </div>
                        <span className="text-xs text-gray-400 w-4">{count}</span>
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Write review */}
              <div className="bg-gray-50 rounded-2xl p-5">
                <h4 className="font-semibold text-gray-800 mb-4">Rəy yaz</h4>
                <div className="flex gap-1 mb-4">
                  {[1, 2, 3, 4, 5].map((s) => (
                    <button key={s} type="button"
                      onClick={() => setReviewRating(s)}
                      onMouseEnter={() => setReviewHover(s)}
                      onMouseLeave={() => setReviewHover(0)}
                      className="text-2xl transition-transform hover:scale-110">
                      <Star size={24} className={(reviewHover || reviewRating) >= s ? "text-yellow-400" : "text-gray-200"} fill={(reviewHover || reviewRating) >= s ? "currentColor" : "none"} />
                    </button>
                  ))}
                </div>
                {!user && (
                  <input type="text" value={reviewName} onChange={(e) => setReviewName(e.target.value)}
                    placeholder="Adınız"
                    className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] mb-3" />
                )}
                <textarea value={reviewText} onChange={(e) => setReviewText(e.target.value)}
                  rows={3} placeholder="Məhsul haqqında fikirlərinizi yazın..."
                  className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm outline-none focus:border-[#65a30d] resize-none mb-3" />
                {reviewError && <p className="text-xs text-red-500 mb-2">{reviewError}</p>}
                {reviewSubmitted && (
                  <div className="flex items-center gap-2 text-green-600 text-sm mb-2">
                    <CheckCircle size={16} /> Rəyiniz əlavə edildi!
                  </div>
                )}
                <button type="button" onClick={handleSubmitReview}
                  className="bg-[#65a30d] text-white px-5 py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f] transition-colors">
                  Rəy göndər
                </button>
              </div>

              {/* Reviews list */}
              <div className="space-y-4">
                {allReviews.map((review) => (
                  <div key={review.id} className="border border-gray-100 rounded-xl p-4">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <div className="w-8 h-8 bg-lime-100 rounded-full flex items-center justify-center text-sm font-bold text-lime-600">
                          {review.userName[0]}
                        </div>
                        <div>
                          <div className="flex items-center gap-1.5">
                            <span className="text-sm font-semibold text-gray-700">{review.userName}</span>
                            {review.verified && (
                              <span className="inline-flex items-center gap-0.5 text-[10px] font-semibold text-green-700 bg-green-50 border border-green-200 px-1.5 py-0.5 rounded-full">
                                <BadgeCheck size={10} /> Doğrulanmış alış
                              </span>
                            )}
                          </div>
                          <div className="text-xs text-gray-400">{new Date(review.date).toLocaleDateString("az-AZ", { day: "numeric", month: "long", year: "numeric" })}</div>
                        </div>
                      </div>
                      <div className="flex">
                        {[1, 2, 3, 4, 5].map((s) => (
                          <Star key={s} size={12} className={s <= review.rating ? "text-yellow-400" : "text-gray-200"} fill={s <= review.rating ? "currentColor" : "none"} />
                        ))}
                      </div>
                    </div>
                    <p className="text-sm text-gray-600 mb-3">{review.text}</p>
                    <button type="button" onClick={() => handleHelpful(review.id)}
                      className="flex items-center gap-1.5 text-xs text-gray-400 hover:text-gray-600 transition-colors">
                      <ThumbsUp size={12} /> Faydalı ({review.helpfulCount})
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {activeTab === "info" && (
            <div className="space-y-4 text-sm text-gray-600">
              <div className="bg-blue-50 rounded-xl p-4">
                <h4 className="font-semibold text-gray-800 mb-2 flex items-center gap-2">
                  <Truck size={16} className="text-blue-600" /> Çatdırılma məlumatı
                </h4>
                <ul className="space-y-1 text-sm">
                  <li>• Eyni gün çatdırılma: saat 14:00-a qədər verilən sifarişlər üçün keçərlidir.</li>
                  <li>• Çatdırılma vaxtı: 10:00 – 21:00 arası.</li>
                  <li>• Bakı daxilində pulsuz çatdırılma. Regionlara ayrı qiymət.</li>
                  <li>• Kargo ilə göndəriş: 2-3 iş günü.</li>
                </ul>
              </div>
              <div className="bg-green-50 rounded-xl p-4">
                <h4 className="font-semibold text-gray-800 mb-2 flex items-center gap-2">
                  <RotateCcw size={16} className="text-green-600" /> Geri qaytarma siyasəti
                </h4>
                <ul className="space-y-1 text-sm">
                  <li>• Təzə olmayan və ya zədələnmiş məhsullar pulsuz dəyişdirilir.</li>
                  <li>• Çatdırmadan sonra 24 saat ərzində bildirilən problemlər qəbul edilir.</li>
                  <li>• İadə sorğusu Hesabım &rarr; Sifarişlərim bölməsindən edilə bilər.</li>
                  <li>• Müştəri xidmətimizə *6500 nömrəsi ilə zəng edə bilərsiniz.</li>
                </ul>
              </div>
            </div>
          )}
        </div>
      </div>

      {related.length > 0 && (
        <div className="mt-12">
          <h2 className="text-xl font-bold text-gray-800 mb-5">Oxşar məhsullar</h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
            {related.map((p) => <ProductCard key={p.id} product={p} />)}
          </div>
        </div>
      )}
    </div>
  );
}
