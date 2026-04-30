import { Link } from "react-router-dom";
import { Package, MapPin, Heart, ArrowRight, Sparkles, RotateCcw, TrendingUp } from "lucide-react";
import { useAuth } from "../../context/AuthContext";
import { useOrders, orderStatusLabel, orderStatusTone } from "../../context/OrderContext";
import { useAddresses } from "../../context/AddressContext";
import { useFavorites } from "../../context/FavoritesContext";
import { formatMoney } from "../../lib/format";

export default function AccountDashboard() {
  const { user } = useAuth();
  const { orders } = useOrders();
  const { addresses } = useAddresses();
  const { favoriteCount } = useFavorites();

  const recent = orders.slice(0, 3);
  const totalSpent = orders.filter((o) => o.status !== "cancelled").reduce((s, o) => s + o.totalPrice, 0);
  const bonusPoints = Math.floor(totalSpent * 0.5);

  const displayName = user?.firstName ?? "Qonaq";

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-800">Xoş gəldiniz, {displayName}!</h1>
        <p className="text-sm text-gray-500 mt-1">Son fəaliyyətiniz və qısa statistikalar burada göstərilir.</p>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-white rounded-2xl border border-gray-100 p-4">
          <div className="text-xs text-gray-500 mb-1">Ümumi sifariş</div>
          <div className="text-2xl font-bold text-gray-800">{orders.length}</div>
        </div>
        <div className="bg-white rounded-2xl border border-gray-100 p-4">
          <div className="text-xs text-gray-500 mb-1">Ünvan</div>
          <div className="text-2xl font-bold text-gray-800">{addresses.length}</div>
        </div>
        <div className="bg-white rounded-2xl border border-gray-100 p-4">
          <div className="text-xs text-gray-500 mb-1">Seçilmişlər</div>
          <div className="text-2xl font-bold text-gray-800">{favoriteCount}</div>
        </div>
        <div className="bg-white rounded-2xl border border-gray-100 p-4">
          <div className="text-xs text-gray-500 mb-1">Bonus xal</div>
          <div className="text-2xl font-bold text-[#65a30d] flex items-center gap-1">
            <Sparkles size={18} /> {bonusPoints}
          </div>
        </div>
      </div>

      {totalSpent > 0 && (
        <div className="bg-gradient-to-r from-lime-50 to-purple-50 rounded-2xl border border-lime-100 p-4 flex items-center gap-4">
          <div className="w-10 h-10 bg-[#65a30d] rounded-full flex items-center justify-center shrink-0">
            <TrendingUp size={20} className="text-white" />
          </div>
          <div>
            <p className="text-sm font-semibold text-gray-800">Ümumi xərc: {formatMoney(totalSpent)}</p>
            <p className="text-xs text-gray-500">{bonusPoints} bonus xal qazandınız — növbəti sifarişdə istifadə edin</p>
          </div>
        </div>
      )}

      <div className="bg-white rounded-2xl border border-gray-100 overflow-hidden">
        <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100">
          <h2 className="font-semibold text-gray-800 flex items-center gap-2">
            <Package size={18} className="text-[#65a30d]" /> Son sifarişlər
          </h2>
          <Link to="/hesabim/sifarisler" className="text-sm text-[#65a30d] font-medium flex items-center gap-1 hover:underline">
            Hamısı <ArrowRight size={14} />
          </Link>
        </div>
        {recent.length === 0 ? (
          <div className="px-4 py-8 text-center text-gray-400 text-sm">
            <Package size={32} className="mx-auto mb-2 opacity-30" />
            Hələ heç bir sifariş yoxdur.
          </div>
        ) : (
          <ul className="divide-y divide-gray-100">
            {recent.map((order) => (
              <li key={order.id} className="px-4 py-3 flex flex-wrap items-center justify-between gap-2">
                <div>
                  <p className="font-medium text-gray-800">{order.id}</p>
                  <p className="text-xs text-gray-500">{new Date(order.placedAt).toLocaleDateString("az-AZ", { day: "numeric", month: "long", year: "numeric" })}</p>
                </div>
                <span className={`text-xs font-semibold px-2 py-1 rounded-full border ${orderStatusTone(order.status)}`}>
                  {orderStatusLabel(order.status)}
                </span>
                <span className="text-sm font-bold text-gray-900">{formatMoney(order.totalPrice)}</span>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <Link to="/hesabim/unvanlar" className="bg-white rounded-2xl border border-gray-100 p-4 flex items-center gap-3 hover:border-lime-200 transition-colors group">
          <div className="w-10 h-10 rounded-xl bg-lime-50 flex items-center justify-center text-[#65a30d]"><MapPin size={20} /></div>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-gray-800 group-hover:text-[#65a30d]">Ünvanlar</p>
            <p className="text-xs text-gray-500">{addresses.length} ünvan saxlanılıb</p>
          </div>
          <ArrowRight size={18} className="text-gray-300 group-hover:text-[#65a30d] shrink-0" />
        </Link>
        <Link to="/favoriler" className="bg-white rounded-2xl border border-gray-100 p-4 flex items-center gap-3 hover:border-lime-200 transition-colors group">
          <div className="w-10 h-10 rounded-xl bg-lime-50 flex items-center justify-center text-[#65a30d]"><Heart size={20} /></div>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-gray-800 group-hover:text-[#65a30d]">Seçilmişlər</p>
            <p className="text-xs text-gray-500">{favoriteCount > 0 ? `${favoriteCount} məhsul` : "Məhsul əlavə edin"}</p>
          </div>
          <ArrowRight size={18} className="text-gray-300 group-hover:text-[#65a30d] shrink-0" />
        </Link>
        <Link to="/hesabim/iadeler" className="bg-white rounded-2xl border border-gray-100 p-4 flex items-center gap-3 hover:border-lime-200 transition-colors group">
          <div className="w-10 h-10 rounded-xl bg-lime-50 flex items-center justify-center text-[#65a30d]"><RotateCcw size={20} /></div>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-gray-800 group-hover:text-[#65a30d]">İadə / Şikayət</p>
            <p className="text-xs text-gray-500">Sifariş problemi bildirin</p>
          </div>
          <ArrowRight size={18} className="text-gray-300 group-hover:text-[#65a30d] shrink-0" />
        </Link>
      </div>
    </div>
  );
}
