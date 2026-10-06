/**
 * Navbar/Footer hover-da səhifə chunk-larını əvvəlcədən yükləyir.
 * `App.tsx`-dəki `lazy()` ilə eyni modulları hədəf alır.
 */
const SIMPLE: Record<string, () => Promise<unknown>> = {
  "/": () => import("../pages/HomePage"),
  "/haqqimizda": () => import("../pages/AboutPage"),
  "/blog": () => import("../pages/BlogPage"),
  "/yardim": () => import("../pages/HelpPage"),
  "/kampanyalar": () => import("../pages/CampaignsPage"),
  "/mehsullar": () => import("../pages/CategoryPage"),
  "/sepet": () => import("../pages/CartPage"),
  "/odeme": () => import("../pages/CheckoutPage"),
  "/sifaris-tesdiq": () => import("../pages/OrderConfirmationPage"),
  "/favoriler": () => import("../pages/FavoritesPage"),
  "/search": () => import("../pages/SearchPage"),
  "/giris": () => import("../pages/LoginPage"),
  "/sifre-yenile": () => import("../pages/ForgotPasswordPage"),
  "/mexfilik": () => import("../pages/PrivacyPage"),
  "/istifade-sertleri": () => import("../pages/TermsPage"),
  "/korporativ": () => import("../pages/CorporatePage"),
  "/satici-ol": () => import("../pages/SellerPage"),
};

const ACCOUNT_TAB: Record<string, () => Promise<unknown>> = {
  "": () => import("../pages/account/AccountDashboard"),
  sifarisler: () => import("../pages/account/AccountOrders"),
  unvanlar: () => import("../pages/account/AccountAddresses"),
  iadeler: () => import("../pages/account/AccountReturns"),
  profil: () => import("../pages/account/AccountProfile"),
  tenzimlemeler: () => import("../pages/account/AccountSettings"),
};

function normalizePath(raw: string): string {
  const path = raw.split("?")[0].split("#")[0];
  if (path === "" || path === "/") return "/";
  return path.replace(/\/+$/, "") || "/";
}

export function prefetchStorePath(raw: string): void {
  const p = normalizePath(raw);

  const hit = SIMPLE[p];
  if (hit) {
    void hit();
    return;
  }

  if (p.startsWith("/kategori/")) {
    void import("../pages/CategoryPage");
    return;
  }
  if (p.startsWith("/urun/")) {
    void import("../pages/ProductDetailPage");
    return;
  }
  if (p.startsWith("/magaza/")) {
    void import("../pages/SellerStorePage");
    return;
  }

  if (p === "/hesabim") {
    void import("../pages/account/AccountLayout");
    void import("../pages/account/AccountDashboard");
    return;
  }
  if (p.startsWith("/hesabim/")) {
    void import("../pages/account/AccountLayout");
    const rest = p.slice("/hesabim/".length).split("/")[0] ?? "";
    const tab = ACCOUNT_TAB[rest];
    if (tab) void tab();
    return;
  }
}

export function prefetchOnIntent(to: string) {
  return {
    onMouseEnter: () => {
      prefetchStorePath(to);
    },
    onFocus: () => {
      prefetchStorePath(to);
    },
  } as const;
}
