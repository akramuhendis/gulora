import { Link } from "react-router-dom";
import { Heart } from "lucide-react";
import { useFavorites } from "../context/FavoritesContext";
import ProductCard from "../components/ProductCard";

export default function FavoritesPage() {
  const { favoriteProducts, favoriteCount } = useFavorites();

  if (favoriteCount === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-12 text-center">
        <Heart className="mx-auto text-lime-200 mb-4" size={48} />
        <h1 className="text-2xl font-bold text-gray-800 mb-2">Seçilmişlər</h1>
        <p className="text-gray-500 text-sm max-w-md mx-auto mb-6">
          Hələ seçilmiş məhsul yoxdur. Məhsul kartında və ya məhsul səhifəsində ürək ikonasına
          toxunun — siyahı burada saxlanılacaq (bu cihazda).
        </p>
        <Link
          to="/mehsullar"
          className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl text-sm font-semibold hover:bg-[#4d7c0f] transition-colors"
        >
          Məhsullara bax
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-2">
        <Heart className="text-[#65a30d]" size={28} fill="currentColor" />
        <h1 className="text-2xl font-bold text-gray-800">Seçilmişlər</h1>
        <span className="text-sm text-gray-500">({favoriteCount})</span>
      </div>
      <p className="text-sm text-gray-500 mb-8">
        Seçdiyiniz məhsullar bu cihazda saxlanılır. Ürək ikonası ilə istənilənini siyahıdan çıxara
        bilərsiniz.
      </p>
      <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4">
        {favoriteProducts.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>
    </div>
  );
}
