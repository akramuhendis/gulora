import { lazy, Suspense, type ReactNode } from "react";
import { BrowserRouter, Routes, Route, Navigate, useLocation } from "react-router-dom";
import { ToastProvider } from "./context/ToastContext";
import { AuthProvider } from "./context/AuthContext";
import { CartProvider } from "./context/CartContext";
import { FavoritesProvider } from "./context/FavoritesContext";
import { OrderProvider } from "./context/OrderContext";
import { AddressProvider } from "./context/AddressContext";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import ToastContainer from "./components/Toast";
import RoutePageFallback from "./components/RoutePageFallback";
import { useAuth } from "./hooks/useAuth";

function RequireAuth({ children }: { children: ReactNode }) {
  const { user, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return <RoutePageFallback />;
  }

  if (!user) {
    return <Navigate to="/giris" replace state={{ from: location.pathname }} />;
  }

  return <>{children}</>;
}

const HomePage = lazy(() => import("./pages/HomePage"));
const CategoryPage = lazy(() => import("./pages/CategoryPage"));
const ProductDetailPage = lazy(() => import("./pages/ProductDetailPage"));
const CartPage = lazy(() => import("./pages/CartPage"));
const CheckoutPage = lazy(() => import("./pages/CheckoutPage"));
const OrderConfirmationPage = lazy(() => import("./pages/OrderConfirmationPage"));
const SearchPage = lazy(() => import("./pages/SearchPage"));
const FavoritesPage = lazy(() => import("./pages/FavoritesPage"));
const AboutPage = lazy(() => import("./pages/AboutPage"));
const BlogPage = lazy(() => import("./pages/BlogPage"));
const HelpPage = lazy(() => import("./pages/HelpPage"));
const CampaignsPage = lazy(() => import("./pages/CampaignsPage"));
const LoginPage = lazy(() => import("./pages/LoginPage"));
const ForgotPasswordPage = lazy(() => import("./pages/ForgotPasswordPage"));
const PrivacyPage = lazy(() => import("./pages/PrivacyPage"));
const TermsPage = lazy(() => import("./pages/TermsPage"));
const CorporatePage = lazy(() => import("./pages/CorporatePage"));
const SellerPage = lazy(() => import("./pages/SellerPage"));
const SellerStorePage = lazy(() => import("./pages/SellerStorePage"));
const AccountLayout = lazy(() => import("./pages/account/AccountLayout"));
const AccountDashboard = lazy(() => import("./pages/account/AccountDashboard"));
const AccountOrders = lazy(() => import("./pages/account/AccountOrders"));
const AccountAddresses = lazy(() => import("./pages/account/AccountAddresses"));
const AccountProfile = lazy(() => import("./pages/account/AccountProfile"));
const AccountSettings = lazy(() => import("./pages/account/AccountSettings"));
const AccountReturns = lazy(() => import("./pages/account/AccountReturns"));
const NotificationsPage = lazy(() => import("./pages/NotificationsPage"));

export default function App() {
  return (
    <BrowserRouter>
      <ToastProvider>
        <AuthProvider>
          <CartProvider>
            <FavoritesProvider>
              <OrderProvider>
                <AddressProvider>
                  <div className="min-h-screen flex flex-col bg-gray-50/50">
                    <Navbar />
                    <main className="flex-1">
                      <Suspense fallback={<RoutePageFallback />}>
                        <Routes>
                          <Route path="/" element={<HomePage />} />
                          <Route path="/haqqimizda" element={<AboutPage />} />
                          <Route path="/blog" element={<BlogPage />} />
                          <Route path="/yardim" element={<HelpPage />} />
                          <Route path="/kampanyalar" element={<CampaignsPage />} />
                          <Route path="/mehsullar" element={<CategoryPage />} />
                          <Route path="/kategori/:slug" element={<CategoryPage />} />
                          <Route path="/urun/:slug" element={<ProductDetailPage />} />
                          <Route path="/magaza/:slug" element={<SellerStorePage />} />
                          <Route path="/sepet" element={<CartPage />} />
                          <Route path="/odeme" element={<CheckoutPage />} />
                          <Route path="/sifaris-tesdiq" element={<OrderConfirmationPage />} />
                          <Route path="/favoriler" element={<FavoritesPage />} />
                          <Route path="/search" element={<SearchPage />} />
                          <Route path="/bildirisler" element={<NotificationsPage />} />
                          <Route path="/giris" element={<LoginPage />} />
                          <Route path="/sifre-yenile" element={<ForgotPasswordPage />} />
                          <Route path="/mexfilik" element={<PrivacyPage />} />
                          <Route path="/istifade-sertleri" element={<TermsPage />} />
                          <Route path="/korporativ" element={<CorporatePage />} />
                          <Route path="/satici-ol" element={<SellerPage />} />
                          <Route
                            path="/hesabim"
                            element={
                              <RequireAuth>
                                <AccountLayout />
                              </RequireAuth>
                            }
                          >
                            <Route index element={<AccountDashboard />} />
                            <Route path="sifarisler" element={<AccountOrders />} />
                            <Route path="unvanlar" element={<AccountAddresses />} />
                            <Route path="iadeler" element={<AccountReturns />} />
                            <Route path="profil" element={<AccountProfile />} />
                            <Route path="tenzimlemeler" element={<AccountSettings />} />
                          </Route>
                          <Route
                            path="*"
                            element={
                              <div className="max-w-7xl mx-auto px-4 py-20 text-center">
                                <div className="text-8xl mb-6">🌸</div>
                                <h2 className="text-3xl font-bold text-gray-700 mb-3">Səhifə tapılmadı</h2>
                                <p className="text-gray-400 mb-8">Axtardığınız səhifə mövcud deyil.</p>
                                <a
                                  href="/"
                                  className="inline-block bg-[#65a30d] text-white px-6 py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors"
                                >
                                  Ana səhifəyə qayıt
                                </a>
                              </div>
                            }
                          />
                        </Routes>
                      </Suspense>
                    </main>
                    <Footer />
                    <ToastContainer />
                  </div>
                </AddressProvider>
              </OrderProvider>
            </FavoritesProvider>
          </CartProvider>
        </AuthProvider>
      </ToastProvider>
    </BrowserRouter>
  );
}
