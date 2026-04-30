import { useState, useRef, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  Search, ShoppingCart, User, Heart, MapPin, Phone,
  ChevronDown, Menu, X, LogOut, Package, Settings, UserCircle,
} from "lucide-react";
import { useCart } from "../context/CartContext";
import { useFavorites } from "../context/FavoritesContext";
import { useAuth } from "../context/AuthContext";
import { categories } from "../data/products";

export default function Navbar() {
  const { totalItems } = useCart();
  const { favoriteCount } = useFavorites();
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState("");
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const userMenuRef = useRef<HTMLDivElement>(null);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
  };

  const handleLogout = () => {
    logout();
    setUserMenuOpen(false);
    navigate("/");
  };

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
        setUserMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  const initials = user ? `${user.firstName[0]}${user.lastName[0]}`.toUpperCase() : null;

  return (
    <header className="sticky top-0 z-50 bg-white border-b border-gray-200">
      {/* Top promo bar */}
      <div className="bg-gray-900 text-gray-100 text-xs py-2">
        <div className="max-w-[1400px] mx-auto px-4 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <span className="flex items-center gap-1.5">
              <Phone size={11} className="text-[#65a30d]" /> *6500
            </span>
            <span className="hidden sm:flex items-center gap-1.5 font-semibold text-white">
              🚀 Eyni gün çatdırılma · Sifariş verin, axşam ünvanınızda
            </span>
          </div>
          <div className="flex items-center gap-4">
            <Link to="/korporativ" className="hidden md:block text-gray-400 hover:text-white">Korporativ</Link>
            <Link to="/satici-ol" className="hidden md:block text-gray-400 hover:text-white">Satıcı ol</Link>
            <Link to="/haqqimizda" className="hidden sm:block text-gray-400 hover:text-white">Haqqımızda</Link>
            <Link to="/yardim" className="text-gray-400 hover:text-white">Kömək</Link>
          </div>
        </div>
      </div>

      {/* Main navbar */}
      <div className="max-w-[1400px] mx-auto px-4 py-3">
        <div className="flex items-center gap-4">
          {/* Logo */}
          <Link to="/" className="shrink-0">
            <div className="flex items-center gap-2">
              <div className="w-9 h-9 bg-[#65a30d] rounded-lg flex items-center justify-center">
                <span className="text-white text-xl">🌸</span>
              </div>
              <div>
                <span className="text-[#65a30d] font-extrabold text-xl tracking-tight">Gülora</span>
                <div className="text-gray-400 text-[9px] leading-none font-semibold tracking-widest uppercase">gül və hədiyyə</div>
              </div>
            </div>
          </Link>

          {/* Delivery location */}
          <button className="hidden md:flex items-center gap-1.5 text-sm text-gray-600 border border-gray-200 rounded-lg px-3 py-2 shrink-0">
            <MapPin size={15} className="text-[#65a30d]" />
            <div className="text-left">
              <div className="text-[10px] text-gray-400 leading-none mb-0.5">Çatdırılma ünvanı</div>
              <div className="font-semibold text-xs text-gray-800">Bakı</div>
            </div>
            <ChevronDown size={12} className="text-gray-400 ml-1" />
          </button>

          {/* Search */}
          <form onSubmit={handleSearch} className="flex-1 relative">
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Gül, hədiyyə və ya məhsul axtarın..."
              className="w-full border-2 border-gray-200 focus:border-[#65a30d] rounded-xl py-2.5 px-4 pr-12 text-sm outline-none bg-gray-50 focus:bg-white"
            />
            <button
              type="submit"
              className="absolute right-0 top-0 h-full px-4 bg-[#65a30d] text-white rounded-r-xl flex items-center"
            >
              <Search size={16} />
            </button>
          </form>

          {/* Actions */}
          <div className="flex items-center gap-1">
            {/* User */}
            {user ? (
              <div className="relative hidden sm:block" ref={userMenuRef}>
                <button
                  type="button"
                  onClick={() => setUserMenuOpen(!userMenuOpen)}
                  className="flex flex-col items-center gap-0.5 px-3 py-1.5 text-gray-600 rounded-lg"
                >
                  <div className="w-7 h-7 rounded-full bg-[#65a30d] text-white flex items-center justify-center text-[10px] font-bold">
                    {initials}
                  </div>
                  <span className="text-[10px] font-semibold max-w-[60px] truncate">{user.firstName}</span>
                </button>
                {userMenuOpen && (
                  <div className="absolute right-0 top-full mt-2 w-56 bg-white rounded-xl border border-gray-200 shadow-xl py-2 z-50">
                    <div className="px-4 py-2.5 border-b border-gray-100">
                      <p className="text-sm font-bold text-gray-800 truncate">{user.firstName} {user.lastName}</p>
                      <p className="text-xs text-gray-500 truncate">{user.email}</p>
                    </div>
                    <Link to="/hesabim" onClick={() => setUserMenuOpen(false)}
                      className="flex items-center gap-2.5 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50">
                      <UserCircle size={16} /> Hesabım
                    </Link>
                    <Link to="/hesabim/sifarisler" onClick={() => setUserMenuOpen(false)}
                      className="flex items-center gap-2.5 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50">
                      <Package size={16} /> Sifarişlərim
                    </Link>
                    <Link to="/hesabim/tenzimlemeler" onClick={() => setUserMenuOpen(false)}
                      className="flex items-center gap-2.5 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50">
                      <Settings size={16} /> Tənzimləmələr
                    </Link>
                    <div className="border-t border-gray-100 mt-1 pt-1">
                      <button type="button" onClick={handleLogout}
                        className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50">
                        <LogOut size={16} /> Çıxış
                      </button>
                    </div>
                  </div>
                )}
              </div>
            ) : (
              <Link to="/giris"
                className="hidden sm:flex flex-col items-center gap-0.5 px-3 py-1.5 text-gray-600 rounded-lg">
                <User size={20} />
                <span className="text-[10px] font-semibold">Giriş</span>
              </Link>
            )}

            <Link to="/favoriler"
              className="hidden sm:flex flex-col items-center gap-0.5 px-3 py-1.5 text-gray-600 rounded-lg relative">
              <Heart size={20} />
              <span className="text-[10px] font-semibold">Seçilmişlər</span>
              {favoriteCount > 0 && (
                <span className="absolute -top-0.5 right-0.5 bg-[#65a30d] text-white text-[10px] min-w-[18px] min-h-[18px] px-1 rounded-full flex items-center justify-center font-bold">
                  {favoriteCount > 99 ? "99+" : favoriteCount}
                </span>
              )}
            </Link>

            <Link to="/sepet"
              className="flex flex-col items-center gap-0.5 px-3 py-1.5 text-gray-600 rounded-lg relative">
              <ShoppingCart size={20} />
              <span className="text-[10px] font-semibold">Səbət</span>
              {totalItems > 0 && (
                <span className="absolute -top-0.5 right-0.5 bg-[#65a30d] text-white text-[10px] rounded-full flex items-center justify-center font-bold min-w-[18px] min-h-[18px] text-center">
                  {totalItems > 99 ? "99+" : totalItems}
                </span>
              )}
            </Link>

            <button className="sm:hidden p-2 text-gray-600" onClick={() => setMobileMenuOpen(!mobileMenuOpen)}>
              {mobileMenuOpen ? <X size={22} /> : <Menu size={22} />}
            </button>
          </div>
        </div>
      </div>

      {/* Category nav */}
      <nav className="hidden md:block border-t border-gray-100 bg-white">
        <div className="max-w-[1400px] mx-auto px-4">
          <ul className="flex items-center gap-0 overflow-x-auto scrollbar-hide">
            {categories.map((cat) => (
              <li key={cat.id}>
                <Link
                  to={`/kategori/${cat.slug}`}
                  className="flex items-center gap-1.5 px-3 py-2.5 text-sm text-gray-700 font-medium whitespace-nowrap"
                >
                  {cat.name}
                </Link>
              </li>
            ))}
            <li className="ml-auto">
              <Link
                to="/kampanyalar"
                className="flex items-center gap-1 px-3 py-2.5 text-sm text-[#65a30d] font-bold whitespace-nowrap border-l border-gray-100"
              >
                🔥 Kampaniyalar
              </Link>
            </li>
          </ul>
        </div>
      </nav>

      {/* Mobile menu */}
      {mobileMenuOpen && (
        <div className="md:hidden border-t border-gray-100 bg-white pb-4">
          <div className="max-w-[1400px] mx-auto px-4 pt-3">
            <div className="flex gap-4 mb-3 border-b border-gray-100 pb-3">
              {user ? (
                <>
                  <Link to="/hesabim" className="flex items-center gap-2 text-sm text-gray-700" onClick={() => setMobileMenuOpen(false)}>
                    <div className="w-6 h-6 rounded-full bg-[#65a30d] text-white flex items-center justify-center text-[10px] font-bold">{initials}</div>
                    {user.firstName}
                  </Link>
                  <button type="button" onClick={() => { handleLogout(); setMobileMenuOpen(false); }}
                    className="flex items-center gap-2 text-sm text-red-600">
                    <LogOut size={16} /> Çıxış
                  </button>
                </>
              ) : (
                <Link to="/giris" className="flex items-center gap-2 text-sm text-gray-700" onClick={() => setMobileMenuOpen(false)}>
                  <User size={18} /> Giriş / Qeydiyyat
                </Link>
              )}
              <Link to="/favoriler" className="flex items-center gap-2 text-sm text-gray-700" onClick={() => setMobileMenuOpen(false)}>
                <Heart size={18} /> Seçilmişlər
              </Link>
            </div>
            <div className="grid grid-cols-2 gap-2">
              {categories.map((cat) => (
                <Link key={cat.id} to={`/kategori/${cat.slug}`}
                  className="flex items-center gap-2 py-2 px-3 text-sm text-gray-700 bg-gray-50 rounded-lg border border-gray-200"
                  onClick={() => setMobileMenuOpen(false)}>
                  <span>{cat.icon}</span> {cat.name}
                </Link>
              ))}
            </div>
          </div>
        </div>
      )}
    </header>
  );
}
