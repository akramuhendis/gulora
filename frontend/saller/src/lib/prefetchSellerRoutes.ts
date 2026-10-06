/**
 * Naviqasiya hover/focus-da səhifə chunk-larını əvvəlcədən yükləyir (nav klikindən əvvəl).
 * Marşrutlar `App.tsx`-dəki `lazy()` importları ilə eyni faylları hədəf almalıdır.
 */
const SIMPLE: Record<string, () => Promise<unknown>> = {
  '/': () => import('../pages/DashboardPage'),
  '/account': () => import('../pages/AccountPage'),
  '/help': () => import('../pages/HelpCenterPage'),
  '/performance': () => import('../pages/PerformancePage'),
  '/orders': () => import('../pages/OrdersPage'),
  '/analytics': () => import('../pages/AnalyticsPage'),
  '/finance': () => import('../pages/FinancePage'),
  '/products': () => import('../pages/ProductsPage'),
  '/inventory': () => import('../pages/InventoryPage'),
  '/reviews': () => import('../pages/ReviewsRatingsPage'),
  '/messages': () => import('../pages/MessagesPage'),
  '/campaigns': () => import('../pages/CampaignsPage'),
  '/shop': () => import('../pages/SellerShopSettingsPage'),
}

function normalizePath(raw: string): string {
  const path = raw.split('?')[0].split('#')[0]
  if (path === '' || path === '/') return '/'
  return path.replace(/\/+$/, '') || '/'
}

export function prefetchSellerPath(raw: string): void {
  const p = normalizePath(raw)

  const simple = SIMPLE[p]
  if (simple) {
    void simple()
    return
  }

  if (p === '/products/new') {
    void import('../pages/ProductCreatePage')
    return
  }
  if (/^\/products\/[^/]+\/edit$/.test(p)) {
    void import('../pages/ProductEditPage')
    return
  }
  if (/^\/orders\/[^/]+$/.test(p)) {
    void import('../pages/OrderDetailPage')
    return
  }
}

/** Link atributları — `NavLink` / `Link` ilə yayıla bilər */
export function prefetchOnIntent(to: string) {
  return {
    onMouseEnter: () => {
      prefetchSellerPath(to)
    },
    onFocus: () => {
      prefetchSellerPath(to)
    },
  } as const
}
