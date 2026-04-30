import { BrowserRouter, Routes, Route } from "react-router-dom";
import { ToastProvider } from "./context/ToastContext";
import { AuthProvider } from "./context/AuthContext";
import { CartProvider } from "./context/CartContext";
import { FavoritesProvider } from "./context/FavoritesContext";
import { OrderProvider } from "./context/OrderContext";
import { AddressProvider } from "./context/AddressContext";
import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import ToastContainer from "./components/Toast";
import HomePage from "./pages/HomePage";
import CategoryPage from "./pages/CategoryPage";
import ProductDetailPage from "./pages/ProductDetailPage";
import CartPage from "./pages/CartPage";
import CheckoutPage from "./pages/CheckoutPage";
import OrderConfirmationPage from "./pages/OrderConfirmationPage";
import SearchPage from "./pages/SearchPage";
import FavoritesPage from "./pages/FavoritesPage";
import AboutPage from "./pages/AboutPage";
import BlogPage from "./pages/BlogPage";
import HelpPage from "./pages/HelpPage";
import CampaignsPage from "./pages/CampaignsPage";
import LoginPage from "./pages/LoginPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import PrivacyPage from "./pages/PrivacyPage";
import TermsPage from "./pages/TermsPage";
import CorporatePage from "./pages/CorporatePage";
import SellerPage from "./pages/SellerPage";
import AccountLayout from "./pages/account/AccountLayout";
import AccountDashboard from "./pages/account/AccountDashboard";
import AccountOrders from "./pages/account/AccountOrders";
import AccountAddresses from "./pages/account/AccountAddresses";
import AccountProfile from "./pages/account/AccountProfile";
import AccountSettings from "./pages/account/AccountSettings";
import AccountReturns from "./pages/account/AccountReturns";

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
                      <Routes>
                        <Route path="/" element={<HomePage />} />
                        <Route path="/haqqimizda" element={<AboutPage />} />
                        <Route path="/blog" element={<BlogPage />} />
                        <Route path="/yardim" element={<HelpPage />} />
                        <Route path="/kampanyalar" element={<CampaignsPage />} />
                        <Route path="/mehsullar" element={<CategoryPage />} />
                        <Route path="/kategori/:slug" element={<CategoryPage />} />
                        <Route path="/urun/:slug" element={<ProductDetailPage />} />
                        <Route path="/sepet" element={<CartPage />} />
                        <Route path="/odeme" element={<CheckoutPage />} />
                        <Route path="/sifaris-tesdiq" element={<OrderConfirmationPage />} />
                        <Route path="/favoriler" element={<FavoritesPage />} />
                        <Route path="/search" element={<SearchPage />} />
                        <Route path="/giris" element={<LoginPage />} />
                        <Route path="/sifre-yenile" element={<ForgotPasswordPage />} />
                        <Route path="/mexfilik" element={<PrivacyPage />} />
                        <Route path="/istifade-sertleri" element={<TermsPage />} />
                        <Route path="/korporativ" element={<CorporatePage />} />
                        <Route path="/satici-ol" element={<SellerPage />} />
                        <Route path="/hesabim" element={<AccountLayout />}>
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
                              <a href="/" className="inline-block bg-[#65a30d] text-white px-6 py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors">
                                Ana səhifəyə qayıt
                              </a>
                            </div>
                          }
                        />
                      </Routes>
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
