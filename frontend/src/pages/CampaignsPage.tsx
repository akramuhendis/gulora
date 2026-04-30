import { Link } from "react-router-dom";
import { ChevronRight, Tag, Truck } from "lucide-react";
import { getDiscountedProducts } from "../data/products";
import ProductCard from "../components/ProductCard";

export default function CampaignsPage() {
  const discounted = getDiscountedProducts();

  return (
    <div className="max-w-7xl mx-auto px-4 py-10">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-8">
        <Link to="/" className="hover:text-[#65a30d]">
          Ana səhifə
        </Link>
        <ChevronRight size={14} className="text-gray-300" />
        <span className="text-gray-700 font-medium">Kampaniyalar</span>
      </nav>

      <div className="mb-10">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Kampaniyalar</h1>
        <p className="text-gray-600 text-sm max-w-2xl">
          Cari endirimli məhsullar və çatdırılma təklifləri. Kampaniya şərtləri məhsul kartında göstərilir.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-12">
        <div className="bg-gradient-to-r from-rose-500 to-lime-600 rounded-2xl p-6 text-white">
          <Tag className="mb-2 opacity-90" size={24} />
          <h2 className="text-xl font-bold mb-1">Endirimli çiçəklər</h2>
          <p className="text-sm text-rose-100 mb-4">
            Seçilmiş buket və kompozisiyalarda endirim — məhsullar aşağıdadır.
          </p>
          <p className="text-xs text-rose-200">
            Məsələn: {discounted.length} endirimli məhsul mövcuddur.
          </p>
        </div>
        <div className="bg-gradient-to-r from-violet-600 to-purple-700 rounded-2xl p-6 text-white">
          <Truck className="mb-2 opacity-90" size={24} />
          <h2 className="text-xl font-bold mb-1">Pulsuz çatdırılma</h2>
          <p className="text-sm text-violet-100 mb-4">
            Çoxlu məhsullarda eyni gün və pulsuz çatdırılma nişanına baxın.
          </p>
          <p className="text-xs text-violet-200">
            Minimum səbət məbləği tezliklə elan olunacaq.
          </p>
        </div>
      </div>

      <h2 className="text-xl font-bold text-gray-800 mb-4">Endirimli məhsullar</h2>
      {discounted.length === 0 ? (
        <p className="text-gray-500 text-sm">Hazırda endirimli məhsul yoxdur.</p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
          {discounted.map((p) => (
            <ProductCard key={p.id} product={p} />
          ))}
        </div>
      )}

      <p className="text-xs text-gray-400 mt-12 max-w-xl">
        Kampaniya tarixləri və şərtlər marketinq komandası tərəfindən təsdiqlənəndə bu səhifədə tam
        təqvim göstəriləcək. Nümunə məzmundur.
      </p>
    </div>
  );
}
