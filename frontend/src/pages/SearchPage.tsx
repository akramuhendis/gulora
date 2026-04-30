import { useSearchParams, Link } from "react-router-dom";
import { Search } from "lucide-react";
import { products } from "../data/products";
import ProductCard from "../components/ProductCard";

export default function SearchPage() {
  const [searchParams] = useSearchParams();
  const query = (searchParams.get("q") ?? "").trim();
  const hasQuery = query.length > 0;
  const qLower = query.toLowerCase();

  const results = hasQuery
    ? products.filter(
        (p) =>
          p.name.toLowerCase().includes(qLower) ||
          p.tags.some((t) => t.toLowerCase().includes(qLower)) ||
          p.category.toLowerCase().includes(qLower)
      )
    : [];

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="mb-6">
        <div className="flex items-center gap-2 mb-2">
          <Search size={20} className="text-gray-400" />
          <h1 className="text-xl font-bold text-gray-800">
            {hasQuery ? (
              <>«{query}» üçün axtarış nəticələri</>
            ) : (
              <>Məhsul axtarışı</>
            )}
          </h1>
        </div>
        <p className="text-sm text-gray-500">
          {hasQuery ? `${results.length} məhsul tapıldı` : "Axtarış sorğusu daxil edin"}
        </p>
      </div>

      {!hasQuery ? (
        <div className="text-center py-20">
          <div className="text-6xl mb-4">🔎</div>
          <h3 className="text-xl font-semibold text-gray-600 mb-2">
            Axtarış sorğusu boşdur
          </h3>
          <p className="text-gray-400 mb-6 max-w-md mx-auto text-sm">
            Yuxarıdakı axtarış xanasına məhsul adı və ya açar söz yazın. Boş sorğu ilə bütün
            kataloq göstərilmir.
          </p>
          <Link
            to="/mehsullar"
            className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl font-medium hover:bg-[#4d7c0f] transition-colors mr-3"
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
      ) : results.length === 0 ? (
        <div className="text-center py-20">
          <div className="text-6xl mb-4">🔍</div>
          <h3 className="text-xl font-semibold text-gray-600 mb-2">
            Nəticə tapılmadı
          </h3>
          <p className="text-gray-400 mb-6">
            «{query}» üçün məhsul tapılmadı. Başqa axtarış sınayın.
          </p>
          <Link
            to="/"
            className="inline-block bg-[#65a30d] text-white px-6 py-2.5 rounded-xl font-medium hover:bg-[#4d7c0f] transition-colors"
          >
            Ana səhifəyə qayıt
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
          {results.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  );
}
