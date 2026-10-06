import { Suspense, useEffect, useRef, useState, type ReactNode } from 'react'
import { Link, NavLink, Outlet } from 'react-router-dom'
import {
  BarChart3,
  Bell,
  Boxes,
  Gauge,
  HelpCircle,
  Landmark,
  LayoutDashboard,
  LogOut,
  Megaphone,
  Menu,
  MessageSquare,
  Package,
  ShoppingCart,
  Star,
  Store,
  User,
  X,
} from 'lucide-react'
import { useAuth } from '../hooks/useAuth'
import RoutePageFallback from './RoutePageFallback'
import { prefetchOnIntent } from '../lib/prefetchSellerRoutes'
import { PARTNER_ANNOUNCEMENTS } from '../data/partnerHub'

const PRIMARY = '#006437'

const navCls = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-2 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2 ${
    isActive
      ? 'bg-[#006437] text-white shadow-sm'
      : 'text-slate-600 hover:bg-white hover:text-slate-900'
  }`

function NavGroup({
  title,
  children,
}: {
  title: string
  children: ReactNode
}) {
  return (
    <div className="mt-4 first:mt-0">
      <p className="mb-1.5 px-3 text-[10px] font-bold uppercase tracking-wider text-slate-400">
        {title}
      </p>
      <div className="flex flex-col gap-0.5">{children}</div>
    </div>
  )
}

export default function AppLayout() {
  const { user, logout } = useAuth()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [notifOpen, setNotifOpen] = useState(false)
  const asideRef = useRef<HTMLElement | null>(null)
  const notifMobileRef = useRef<HTMLDivElement | null>(null)
  const notifDesktopRef = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    if (!mobileOpen) return
    window.requestAnimationFrame(() => {
      const first = asideRef.current?.querySelector<HTMLElement>(
        'a[href], button:not([disabled])'
      )
      first?.focus()
    })
  }, [mobileOpen])

  useEffect(() => {
    if (!mobileOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMobileOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [mobileOpen])

  useEffect(() => {
    if (!notifOpen) return
    const onDoc = (e: MouseEvent) => {
      const t = e.target as Node
      const a = notifMobileRef.current
      const b = notifDesktopRef.current
      if ((a && a.contains(t)) || (b && b.contains(t))) return
      setNotifOpen(false)
    }
    document.addEventListener('mousedown', onDoc)
    return () => document.removeEventListener('mousedown', onDoc)
  }, [notifOpen])

  useEffect(() => {
    if (mobileOpen) {
      document.body.style.overflow = 'hidden'
      return () => {
        document.body.style.overflow = ''
      }
    }
    return undefined
  }, [mobileOpen])

  function closeMobile() {
    setMobileOpen(false)
  }

  return (
    <div className="flex min-h-screen flex-col bg-[#f9f9f9] lg:flex-row">
      <header className="sticky top-0 z-30 flex items-center justify-between gap-3 border-b border-slate-200 bg-[#f9f9f9] px-4 py-3 lg:hidden">
        <div>
          <p className="text-base font-bold uppercase tracking-tight" style={{ color: PRIMARY }}>
            GÜLORA
          </p>
          <p className="text-xs text-slate-600">Satıcı paneli</p>
        </div>
        <div className="flex items-center gap-0.5">
          <div className="relative" ref={notifMobileRef}>
            <button
              type="button"
              onClick={() => setNotifOpen((o) => !o)}
              className="rounded-lg p-2 text-slate-700 hover:bg-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
              aria-expanded={notifOpen}
              aria-haspopup="dialog"
              aria-label="Bildirişlər"
            >
              <Bell className="h-6 w-6" aria-hidden />
            </button>
            {notifOpen && (
              <div
                className="absolute right-0 top-full z-50 mt-2 w-[min(100vw-2rem,20rem)] rounded-xl border border-slate-200 bg-white p-3 shadow-xl"
                role="dialog"
                aria-label="Duyurular"
              >
                <p className="text-xs font-bold uppercase tracking-wide text-slate-500">Duyurular</p>
                <ul className="mt-2 max-h-64 space-y-2 overflow-y-auto text-sm">
                  {PARTNER_ANNOUNCEMENTS.map((a) => (
                    <li key={a.id} className="rounded-lg bg-slate-50 px-3 py-2">
                      <p className="font-semibold text-slate-900">{a.title}</p>
                      <p className="mt-1 text-xs text-slate-600">{a.body}</p>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
          <Link
            to="/help"
            {...prefetchOnIntent('/help')}
            className="rounded-lg p-2 text-slate-700 hover:bg-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
            aria-label="Yardım mərkəzi"
          >
            <HelpCircle className="h-6 w-6" aria-hidden />
          </Link>
          <button
            type="button"
            onClick={() => setMobileOpen(true)}
            className="rounded-lg p-2 text-slate-700 hover:bg-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
            aria-expanded={mobileOpen}
            aria-controls="seller-sidebar"
            aria-label="Menyunu aç"
          >
            <Menu className="h-6 w-6" aria-hidden />
          </button>
        </div>
      </header>

      {mobileOpen && (
        <button
          type="button"
          className="fixed inset-0 z-40 bg-black/40 lg:hidden"
          aria-label="Menyunu bağla"
          onClick={closeMobile}
        />
      )}

      <aside
        ref={asideRef}
        id="seller-sidebar"
        className={`fixed inset-y-0 left-0 z-50 flex w-64 shrink-0 flex-col border-r border-slate-200 bg-[#f9f9f9] shadow-xl motion-safe:transition-transform motion-safe:duration-200 lg:static lg:z-0 lg:shadow-none ${
          mobileOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        }`}
      >
        <div className="flex items-center justify-end border-b border-slate-200 px-3 py-2 lg:hidden">
          <button
            type="button"
            onClick={closeMobile}
            className="rounded-lg p-2 text-slate-600 hover:bg-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
            aria-label="Menyunu bağla"
          >
            <X className="h-5 w-5" aria-hidden />
          </button>
        </div>

        <div className="border-b border-slate-200/80 px-4 py-5">
          <p className="text-lg font-bold uppercase tracking-tight" style={{ color: PRIMARY }}>
            GÜLORA
          </p>
          <p className="mt-0.5 text-sm font-semibold text-slate-800">Satıcı paneli</p>
          {user ? (
            <p className="mt-2 truncate text-xs text-slate-500" title={user.email}>
              {user.fullName}
            </p>
          ) : (
            <p className="mt-2 text-xs text-slate-400">—</p>
          )}

          <div className="relative mt-3 hidden lg:block" ref={notifDesktopRef}>
            <div className="flex gap-1">
              <button
                type="button"
                onClick={() => setNotifOpen((o) => !o)}
                className="inline-flex flex-1 items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-2 py-2 text-xs font-semibold text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
                aria-expanded={notifOpen}
                aria-haspopup="dialog"
              >
                <Bell className="h-4 w-4 shrink-0 text-slate-700" aria-hidden />
                Duyuru
              </button>
              <Link
                to="/help"
                {...prefetchOnIntent('/help')}
                onClick={closeMobile}
                className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-2 py-2 text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
                aria-label="Yardım"
                title="Yardım mərkəzi"
              >
                <HelpCircle className="h-4 w-4" aria-hidden />
              </Link>
              <Link
                to="/performance"
                {...prefetchOnIntent('/performance')}
                onClick={closeMobile}
                className="inline-flex items-center justify-center rounded-lg border border-slate-200 bg-white px-2 py-2 text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
                aria-label="Performans"
                title="Performans"
              >
                <Gauge className="h-4 w-4" aria-hidden />
              </Link>
            </div>
            {notifOpen && (
              <div
                className="absolute left-0 right-0 top-full z-50 mt-2 max-h-72 overflow-y-auto rounded-xl border border-slate-200 bg-white p-3 shadow-xl"
                role="dialog"
                aria-label="Duyurular"
              >
                <p className="text-xs font-bold uppercase tracking-wide text-slate-500">Duyurular</p>
                <ul className="mt-2 space-y-2 text-sm">
                  {PARTNER_ANNOUNCEMENTS.map((a) => (
                    <li key={a.id} className="rounded-lg bg-slate-50 px-3 py-2">
                      <p className="font-semibold text-slate-900">{a.title}</p>
                      <p className="mt-1 text-xs text-slate-600">{a.body}</p>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        </div>

        <nav className="flex flex-1 flex-col overflow-y-auto p-3 pb-2" aria-label="Əsas naviqasiya">
          <NavGroup title="Ümumi">
            <NavLink to="/" end className={navCls} onClick={closeMobile} {...prefetchOnIntent('/')}>
              <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
              İcmal
            </NavLink>
            <NavLink to="/account" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/account')}>
              <User className="h-4 w-4 shrink-0" aria-hidden />
              Hesabım
            </NavLink>
            <NavLink to="/help" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/help')}>
              <HelpCircle className="h-4 w-4 shrink-0" aria-hidden />
              Yardım mərkəzi
            </NavLink>
            <NavLink to="/performance" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/performance')}>
              <Gauge className="h-4 w-4 shrink-0" aria-hidden />
              Performans
            </NavLink>
          </NavGroup>

          <NavGroup title="Satış">
            <NavLink to="/orders" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/orders')}>
              <ShoppingCart className="h-4 w-4 shrink-0" aria-hidden />
              Sifarişlər
            </NavLink>
            <NavLink to="/analytics" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/analytics')}>
              <BarChart3 className="h-4 w-4 shrink-0" aria-hidden />
              Analitika
            </NavLink>
            <NavLink to="/finance" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/finance')}>
              <Landmark className="h-4 w-4 shrink-0" aria-hidden />
              Maliyyə
            </NavLink>
          </NavGroup>

          <NavGroup title="Məhsul">
            <NavLink to="/products" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/products')}>
              <Package className="h-4 w-4 shrink-0" aria-hidden />
              Məhsullar
            </NavLink>
            <NavLink to="/inventory" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/inventory')}>
              <Boxes className="h-4 w-4 shrink-0" aria-hidden />
              İnventar
            </NavLink>
          </NavGroup>

          <NavGroup title="Müştəri və marketinq">
            <NavLink to="/reviews" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/reviews')}>
              <Star className="h-4 w-4 shrink-0" aria-hidden />
              Rəylər
            </NavLink>
            <NavLink to="/messages" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/messages')}>
              <MessageSquare className="h-4 w-4 shrink-0" aria-hidden />
              Mesajlar
            </NavLink>
            <NavLink to="/campaigns" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/campaigns')}>
              <Megaphone className="h-4 w-4 shrink-0" aria-hidden />
              Kampaniyalar
            </NavLink>
          </NavGroup>

          <NavGroup title="Mağaza">
            <NavLink to="/shop" className={navCls} onClick={closeMobile} {...prefetchOnIntent('/shop')}>
              <Store className="h-4 w-4 shrink-0" aria-hidden />
              Vitrin parametrləri
            </NavLink>
          </NavGroup>
        </nav>

        <div className="border-t border-slate-200 p-3">
          <button
            type="button"
            onClick={() => void logout()}
            className="flex w-full items-center gap-2 rounded-lg px-3 py-2.5 text-sm font-medium text-red-700 hover:bg-red-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:ring-offset-2"
            aria-label="Hesabdan çıxış"
          >
            <LogOut className="h-4 w-4 shrink-0" aria-hidden />
            Çıxış
          </button>
        </div>
      </aside>

      <main className="min-h-0 flex-1 bg-white lg:min-h-screen">
        <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6 sm:py-8 lg:px-8">
          <Suspense fallback={<RoutePageFallback />}>
            <Outlet />
          </Suspense>
        </div>
      </main>
    </div>
  )
}
