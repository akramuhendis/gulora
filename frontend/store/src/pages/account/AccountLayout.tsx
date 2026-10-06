import { NavLink, Outlet, Link, useNavigate } from "react-router-dom";
import { Suspense } from "react";
import {
  LayoutDashboard, Package, MapPin, UserCircle, Settings,
  ChevronRight, LogOut, RotateCcw,
} from "lucide-react";
import { useAuth } from "../../hooks/useAuth";
import { usePageMeta } from "../../hooks/usePageMeta";
import RoutePageFallback from "../../components/RoutePageFallback";
import { prefetchOnIntent } from "../../lib/prefetchStoreRoutes";

const navItems = [
  { to: "/hesabim", end: true, label: "Ümumi baxış", icon: LayoutDashboard },
  { to: "/hesabim/sifarisler", end: false, label: "Sifarişlərim", icon: Package },
  { to: "/hesabim/unvanlar", end: false, label: "Ünvanlarım", icon: MapPin },
  { to: "/hesabim/iadeler", end: false, label: "İadə / Şikayət", icon: RotateCcw },
  { to: "/hesabim/profil", end: false, label: "Şəxsi məlumat", icon: UserCircle },
  { to: "/hesabim/tenzimlemeler", end: false, label: "Tənzimləmələr", icon: Settings },
];

function NavClasses({ isActive }: { isActive: boolean }) {
  return [
    "flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-colors",
    isActive
      ? "bg-lime-50 text-[#65a30d] border border-lime-100"
      : "text-gray-600 hover:bg-gray-50 border border-transparent",
  ].join(" ");
}

export default function AccountLayout() {
  usePageMeta("Hesabım", "Sifarişlər, ünvanlar və profil — Gülora müştəri hesabı.");

  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    void logout().then(() => navigate("/giris"));
  };

  const displayName = user ? `${user.firstName} ${user.lastName}` : "Qonaq";
  const email = user?.email ?? "";
  const initials = user
    ? `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()
    : "?";

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-6">
        <Link to="/" className="hover:text-[#65a30d]">Ana səhifə</Link>
        <ChevronRight size={14} className="text-gray-300" />
        <span className="text-gray-700 font-medium">Hesabım</span>
      </nav>

      <div className="flex flex-col lg:flex-row gap-8">
        <aside className="lg:w-64 shrink-0">
          <div className="bg-white rounded-2xl border border-gray-100 p-4 mb-4">
            <div className="flex items-center gap-3">
              <div className="w-12 h-12 rounded-full bg-[#65a30d] text-white flex items-center justify-center text-lg font-bold">
                {initials}
              </div>
              <div className="min-w-0">
                <p className="font-semibold text-gray-800 truncate">{displayName}</p>
                <p className="text-xs text-gray-500 truncate">{email}</p>
              </div>
            </div>
            {user && !user.emailVerified && (
              <div className="mt-3 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-xl px-3 py-2">
                E-poçtunuz təsdiqlənməyib.
              </div>
            )}
          </div>

          <nav className="bg-white rounded-2xl border border-gray-100 p-2 hidden lg:block">
            {navItems.map(({ to, end, label, icon: Icon }) => (
              <NavLink
                key={to}
                to={to}
                end={end}
                className={({ isActive }) => NavClasses({ isActive })}
                {...prefetchOnIntent(to)}
              >
                <Icon size={18} className="shrink-0 opacity-80" />
                {label}
              </NavLink>
            ))}
            <button type="button" onClick={handleLogout}
              className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-red-600 hover:bg-red-50 transition-colors mt-1">
              <LogOut size={18} />
              Çıxış
            </button>
          </nav>

          <div className="lg:hidden flex gap-2 overflow-x-auto pb-1 -mx-1 px-1 scrollbar-hide">
            {navItems.map(({ to, end, label }) => (
              <NavLink
                key={to}
                to={to}
                end={end}
                className={({ isActive }) =>
                  ["shrink-0 px-3 py-2 rounded-full text-xs font-semibold border transition-colors",
                    isActive ? "bg-[#65a30d] text-white border-[#65a30d]" : "bg-white text-gray-600 border-gray-200"].join(" ")
                }
                {...prefetchOnIntent(to)}
              >
                {label}
              </NavLink>
            ))}
          </div>
        </aside>

        <div className="flex-1 min-w-0">
          <Suspense fallback={<RoutePageFallback />}>
            <Outlet />
          </Suspense>
        </div>
      </div>
    </div>
  );
}
