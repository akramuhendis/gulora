import { lazy, Suspense, type ReactNode } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './context/AuthProvider'
import { useAuth } from './hooks/useAuth'
import AppShellFallback from './components/AppShellFallback'
import RoutePageFallback from './components/RoutePageFallback'

const AppLayout = lazy(() => import('./components/AppLayout'))

const LoginPage = lazy(() => import('./pages/LoginPage'))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const ProductsPage = lazy(() => import('./pages/ProductsPage'))
const ProductCreatePage = lazy(() => import('./pages/ProductCreatePage'))
const ProductEditPage = lazy(() => import('./pages/ProductEditPage'))
const OrdersPage = lazy(() => import('./pages/OrdersPage'))
const OrderDetailPage = lazy(() => import('./pages/OrderDetailPage'))
const SellerShopSettingsPage = lazy(() => import('./pages/SellerShopSettingsPage'))
const InventoryPage = lazy(() => import('./pages/InventoryPage'))
const AnalyticsPage = lazy(() => import('./pages/AnalyticsPage'))
const FinancePage = lazy(() => import('./pages/FinancePage'))
const ReviewsRatingsPage = lazy(() => import('./pages/ReviewsRatingsPage'))
const CampaignsPage = lazy(() => import('./pages/CampaignsPage'))
const MessagesPage = lazy(() => import('./pages/MessagesPage'))
const AccountPage = lazy(() => import('./pages/AccountPage'))
const HelpCenterPage = lazy(() => import('./pages/HelpCenterPage'))
const PerformancePage = lazy(() => import('./pages/PerformancePage'))

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth()

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-slate-50 text-slate-500">
        Yüklənir…
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}

function AppRoutes() {
  return (
    <Routes>
      <Route
        path="/login"
        element={
          <Suspense fallback={<RoutePageFallback />}>
            <LoginPage />
          </Suspense>
        }
      />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <Suspense fallback={<AppShellFallback />}>
              <AppLayout />
            </Suspense>
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="products/new" element={<ProductCreatePage />} />
        <Route path="products/:slug/edit" element={<ProductEditPage />} />
        <Route path="products" element={<ProductsPage />} />
        <Route path="orders" element={<OrdersPage />} />
        <Route path="orders/:id" element={<OrderDetailPage />} />
        <Route path="inventory" element={<InventoryPage />} />
        <Route path="analytics" element={<AnalyticsPage />} />
        <Route path="finance" element={<FinancePage />} />
        <Route path="reviews" element={<ReviewsRatingsPage />} />
        <Route path="campaigns" element={<CampaignsPage />} />
        <Route path="messages" element={<MessagesPage />} />
        <Route path="account" element={<AccountPage />} />
        <Route path="shop" element={<SellerShopSettingsPage />} />
        <Route path="help" element={<HelpCenterPage />} />
        <Route path="performance" element={<PerformancePage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  )
}
