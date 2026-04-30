import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import {
  ShoppingCart, Heart, Star, Truck, Clock, Shield, RotateCcw,
  ChevronLeft, ChevronRight, Plus, Minus, Share2, CheckCircle, ThumbsUp,
  BadgeCheck,
} from "lucide-react";
import { getProductBySlug, products } from "../data/products";
import { useCart } from "../context/CartContext";
import { useFavorites } from "../context/FavoritesContext";
import { useAuth } from "../context/AuthContext";
import ProductCard from "../components/ProductCard";
import { formatMoney } from "../lib/format";
import { usePageMeta } from "../hooks/usePageMeta";

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

const ADDONS: ProductAddon[] = [
  { id: "chocolate", name: "Ferrero Rocher (8 ədəd)", price: 18, icon: "🍫" },
  { id: "card", name: "Xüsusi kart (yazılı)", price: 5, icon: "💌" },
  { id: "balloon", name: "Balon dəsti (3 ədəd)", price: 12, icon: "🎈" },
  { id: "plushy", name: "Yumşaq oyuncaq (30 sm)", price: 25, icon: "🧸" },
  { id: "candle", name: "Aromatik şam", price: 15, icon: "🕯️" },
];

interface Review {
  id: string;
  productId: number;
  userName: string;
  rating: number;
  text: string;
  date: string;
  helpfulCount: number;
  verified?: boolean;
  images?: string[];
}

function loadReviews(productId: number): Review[] {
  try {
    const raw = localStorage.getItem(`gulora_reviews_${productId}`);
    return raw ? JSON.parse(raw) : [];
  } catch { return []; }
}

function saveReviews(productId: number, reviews: Review[]) {
  localStorage.setItem(`gulora_reviews_${productId}`, JSON.stringify(reviews));
}

const STATIC_REVIEWS: Review[] = [
  { id: "sr1", productId: 0, userName: "Leyla M.", rating: 5, text: "Çox gözəl buket gəldi, ərim çox sevindi. Tövsiyə edirəm.", date: "2026-04-15", helpfulCount: 12, verified: true },
  { id: "sr2", productId: 0, userName: "Elçin Q.", rating: 5, text: "Eyni gün çatdırılma həqiqətən işləyir. Saat 15:00-da sifariş verdim, axşam 19:00-da gəldi.", date: "2026-04-10", helpfulCount: 8, verified: true },
  { id: "sr3", productId: 0, userName: "Nərgiz Ə.", rating: 4, text: "Çiçəklər təzə və gözəl gəldi. Qablaşdırma da çox zərif idi.", date: "2026-04-05", helpfulCount: 5, verified: true },
];

export default function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const { addToCart } = useCart();
  const { isFavorite, toggleFavorite } = useFavorites();
  const { user } = useAuth();

  const productForMeta = getProductBySlug(slug || "");
  usePageMeta(
    productForMeta?.name ?? "Məhsul",
    productForMeta
      ? `${productForMeta.name} — ${productForMeta.description.slice(0, 120)}`
      : undefined
  );

  const [selectedImg, setSelectedImg] = useState(0);
  const [quantity, setQuantity] = useState(1);
  const [addedToCart, setAddedToCart] = useState(false);
  const [activeTab, setActiveTab] = useState<"desc" | "review" | "info">("desc");

  const product = getProductBySlug(slug || "");
  const favorited = product ? isFavorite(product.id) : false;

  const variants = product ? (PRODUCT_VARIANTS[product.categorySlug] ?? null) : null;
  const [selectedVariant, setSelectedVariant] = useState<string>(variants?.[1]?.label ?? variants?.[0]?.label ?? "");
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

  useEffect(() => {
    if (product) setReviews(loadReviews(product.id));
  }, [product?.id]);

  useEffect(() => {
    if (user) setReviewName(`${user.firstName} ${user.lastName[0]}.`);
  }, [user]);

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

  const handleSubmitReview = () => {
    setReviewError("");
    if (!reviewName.trim()) { setReviewError("Ad tələb olunur."); return; }
    if (reviewText.trim().length < 10) { setReviewError("Rəy ən azı 10 simvol olmalıdır."); return; }
    const newReview: Review = {
      id: `rv_${Date.now()}`,
      productId: product.id,
      userName: reviewName.trim(),
      rating: reviewRating,
      text: reviewText.trim(),
      date: new Date().toISOString().slice(0, 10),
      helpfulCount: 0,
    };
    const updated = [newReview, ...reviews];
    setReviews(updated);
    saveReviews(product.id, updated);
    setReviewText("");
    setReviewSubmitted(true);
    setTimeout(() => setReviewSubmitted(false), 3000);
  };

  const handleHelpful = (id: string) => {
    setReviews((prev) => {
      const updated = prev.map((r) => r.id === id ? { ...r, helpfulCount: r.helpfulCount + 1 } : r);
      saveReviews(product.id, updated);
      return updated;
    });
  };

  const allReviews = [...reviews, ...STATIC_REVIEWS.map((r) => ({ ...r, productId: product.id }))];
  const avgRating = allReviews.length > 0 ? allReviews.reduce((s, r) => s + r.rating, 0) / allReviews.length : product.rating;

  const related = products
    .filter((p) => p.categorySlug === product.categorySlug && p.id !== product.id)
    .slice(0, 4);

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
              <button type="button" onClick={() => toggleFavorite(product)}
                aria-label={favorited ? "Seçilmişlərdən çıxar" : "Seçilmişlərə əlavə et"}
                className={`p-2 rounded-full border transition-all ${favorited ? "bg-lime-50 border-lime-300 text-lime-500" : "border-gray-200 text-gray-400 hover:border-lime-300 hover:text-lime-400"}`}>
                <Heart size={18} fill={favorited ? "currentColor" : "none"} />
              </button>
            </div>
            <h1 className="text-2xl md:text-3xl font-bold text-gray-800 mb-3">{product.name}</h1>

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
            </div>
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

          <div className="flex items-center gap-2 text-xs text-gray-500 border-t border-gray-100 pt-4">
            <RotateCcw size={13} />
            7 gün ərzində pulsuz geri qaytarma imkanı
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
                    const pct = allReviews.length > 0 ? (count / allReviews.length) * 100 : star === 5 ? 75 : star === 4 ? 15 : star === 3 ? 7 : 2;
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
