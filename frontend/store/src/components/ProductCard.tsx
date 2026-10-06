import { useState } from "react";
import { Link } from "react-router-dom";
import { Heart, Star, Truck, Clock } from "lucide-react";
import type { Product } from "../data/products";
import { useFavorites } from "../hooks/useFavorites";
import { formatMoney, formatReviewCount } from "../lib/format";

interface ProductCardProps {
  product: Product;
  size?: "normal" | "large";
}

export default function ProductCard({ product, size = "normal" }: ProductCardProps) {
  const { isFavorite, toggleFavorite } = useFavorites();
  const favorited = isFavorite(product.id);
  const [imgError, setImgError] = useState(false);

  const campaignKind = product.sellerCampaign?.kind === "shipping" ? "shipping" : "discount";
  const promoPct =
    campaignKind === "discount" && product.discount && product.discount > 0
      ? product.discount
      : product.sellerCampaign && product.sellerCampaign.percent > 0
        ? product.sellerCampaign.percent
        : undefined;

  const handleWishlist = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    void toggleFavorite(product);
  };

  return (
    <Link
      to={`/urun/${encodeURIComponent(product.slug)}`}
      target="_blank"
      rel="noopener noreferrer"
      className="bg-white rounded-xl overflow-hidden border border-gray-200 flex flex-col cursor-pointer"
    >
      <div className="relative overflow-hidden bg-gray-50">
        <div className={`${size === "large" ? "aspect-4/3" : "aspect-square"} w-full`}>
          {!imgError ? (
            <img
              src={product.image}
              alt={product.name}
              className="w-full h-full object-cover"
              onError={() => setImgError(true)}
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center bg-lime-50 text-6xl">
              🌸
            </div>
          )}
        </div>

        {/* Wishlist */}
        <button
          type="button"
          onClick={handleWishlist}
          aria-label={favorited ? "Seçilmişlərdən çıxar" : "Seçilmişlərə əlavə et"}
          aria-pressed={favorited}
          className={`absolute top-2.5 right-2.5 w-8 h-8 rounded-full flex items-center justify-center border ${
            favorited
              ? "bg-[#65a30d] text-white border-[#65a30d]"
              : "bg-white text-gray-500 border-gray-200"
          }`}
        >
          <Heart size={14} fill={favorited ? "currentColor" : "none"} />
        </button>

        {/* Badges */}
        <div className="absolute top-2.5 left-2.5 flex flex-col gap-1">
          {promoPct != null && promoPct > 0 && (
            <span className="bg-red-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-full">
              {campaignKind === "shipping" ? `Çatdır. %${promoPct}` : `%${promoPct} endirim`}
            </span>
          )}
          {product.badge && (
            <span className="bg-[#65a30d] text-white text-[10px] font-bold px-2 py-0.5 rounded-full">
              {product.badge}
            </span>
          )}
        </div>
      </div>

      <div className="p-3 flex flex-col flex-1 gap-1.5">
        {/* Delivery badges */}
        <div className="flex items-center gap-1.5 flex-wrap">
          {product.isSameDay && (
            <span className="flex items-center gap-0.5 text-[10px] text-green-700 bg-green-50 border border-green-200 px-1.5 py-0.5 rounded-full font-medium">
              <Clock size={9} />
              Eyni gün
            </span>
          )}
          {product.isFreeDelivery && (
            <span className="flex items-center gap-0.5 text-[10px] text-blue-700 bg-blue-50 border border-blue-200 px-1.5 py-0.5 rounded-full font-medium">
              <Truck size={9} />
              Pulsuz
            </span>
          )}
        </div>

        <h3 className="text-sm font-semibold text-gray-800 leading-snug line-clamp-2">
          {product.name}
        </h3>

        {/* Rating */}
        <div className="flex items-center gap-1">
          <div className="flex">
            {[1, 2, 3, 4, 5].map((star) => (
              <Star
                key={star}
                size={11}
                className={star <= Math.round(product.rating) ? "text-yellow-400" : "text-gray-200"}
                fill={star <= Math.round(product.rating) ? "currentColor" : "none"}
              />
            ))}
          </div>
          <span className="text-[10px] text-gray-400">
            ({formatReviewCount(product.reviewCount)})
          </span>
        </div>

        {/* Price */}
        <div className="mt-auto pt-1">
          <div className="flex items-baseline gap-1.5">
            <span className="text-base font-bold text-gray-900">
              {formatMoney(product.price)}
            </span>
            {product.originalPrice && (
              <span className="text-xs text-gray-400 line-through">
                {formatMoney(product.originalPrice)}
              </span>
            )}
            {promoPct != null && promoPct > 0 && product.originalPrice && campaignKind === "discount" && (
              <span className="text-xs font-bold text-red-500 ml-auto">
                {formatMoney(product.originalPrice - product.price)} qənaət
              </span>
            )}
          </div>
        </div>

      </div>
    </Link>
  );
}
