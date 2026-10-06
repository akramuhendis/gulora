/**
 * Bütün REST çağrıları bu moduldan keçir. UI birbaşa fetch çağırmır.
 */

import type { Category, Product } from "../data/products";
import {
  flattenCategories,
  mapDetailToProduct,
  mapOrderResponseWire,
  mapSummaryToProduct,
  wireCategoryToDisplay,
  type CategoryWire,
  type OrderResponseWire,
  type ProductDetailWire,
  type ProductSummaryWire,
  type WishlistItemWire,
} from "../lib/catalogMap";
import type { Order } from "../models/order";
import { enrichOrdersWithProductImages } from "../lib/enrichOrderImages";
import {
  API_MAX_PAGE_SIZE,
  clampApiPageSize,
  normalizeProductSortBy,
  NOTIFICATIONS_MAX_PAGE_SIZE,
  PRODUCT_PUBLIC_REVIEWS_MAX_PAGE_SIZE,
  USER_ORDERS_MAX_PAGE_SIZE,
} from "../lib/apiConstants";

const BASE_URL = import.meta.env.VITE_API_URL ?? "/api/v1";

const ACCESS_TOKEN_KEY = "gulora_store_access_token";
const REFRESH_TOKEN_KEY = "gulora_store_refresh_token";

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

export function clearAuthTokens(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function setAuthTokens(accessToken: string, refreshToken: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
}

function authHeaders(includeJson = true): HeadersInit {
  const token = localStorage.getItem(ACCESS_TOKEN_KEY);
  return {
    ...(includeJson ? { "Content-Type": "application/json" } : {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

function collectValidationMessages(errors: unknown): string | null {
  if (!errors || typeof errors !== "object") return null;
  const seen = new Set<string>();
  const out: string[] = [];
  for (const arr of Object.values(errors as Record<string, unknown>)) {
    if (!Array.isArray(arr)) continue;
    for (const item of arr) {
      if (typeof item === "string" && item.trim() && !seen.has(item)) {
        seen.add(item);
        out.push(item);
      }
    }
  }
  return out.length > 0 ? out.join(" ") : null;
}

async function readErrorMessage(res: Response): Promise<string> {
  try {
    const j = (await res.json()) as {
      detail?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };
    const fromFields =
      j.errors && typeof j.errors === "object" ? collectValidationMessages(j.errors) : null;
    if (fromFields) return fromFields;
    return j.detail || j.title || res.statusText || "Xəta baş verdi.";
  } catch {
    return res.statusText || "Xəta baş verdi.";
  }
}

let refreshInFlight: Promise<boolean> | null = null;

async function tryRefreshToken(): Promise<boolean> {
  if (refreshInFlight) return refreshInFlight;
  const rt = localStorage.getItem(REFRESH_TOKEN_KEY);
  if (!rt) return false;

  refreshInFlight = (async () => {
    try {
      const res = await fetch(`${BASE_URL}/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken: rt }),
      });
      if (!res.ok) return false;
      const data = (await res.json()) as AuthResponseWire;
      localStorage.setItem(ACCESS_TOKEN_KEY, data.accessToken);
      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
      return true;
    } catch {
      return false;
    } finally {
      refreshInFlight = null;
    }
  })();

  return refreshInFlight;
}

async function request<T>(
  path: string,
  options: RequestInit = {},
  allowRefresh = true
): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: {
      ...authHeaders(!(options.body instanceof FormData)),
      ...options.headers,
    },
  });

  const isAuthRefreshExcluded =
    path.startsWith("/auth/login") ||
    path.startsWith("/auth/register") ||
    path.startsWith("/auth/refresh") ||
    path.startsWith("/auth/forgot-password") ||
    path.startsWith("/auth/reset-password");

  if (res.status === 401 && allowRefresh && !isAuthRefreshExcluded) {
    const refreshed = await tryRefreshToken();
    if (refreshed) return request<T>(path, options, false);
    clearAuthTokens();
    throw new ApiError(401, "session_expired");
  }

  if (!res.ok) {
    throw new ApiError(res.status, await readErrorMessage(res));
  }

  if (res.status === 204) return undefined as T;
  const text = await res.text();
  return text ? (JSON.parse(text) as T) : (undefined as T);
}

// ── Category slug köməkçiləri ───────────────────────────────────────────────

let categoryTreeCache: CategoryWire[] | null = null;

async function loadCategoryTree(): Promise<CategoryWire[]> {
  if (!categoryTreeCache) {
    categoryTreeCache = await request<CategoryWire[]>("/categories");
  }
  return categoryTreeCache;
}

export function invalidateCategoryTreeCache(): void {
  categoryTreeCache = null;
}

async function categorySlugMap(): Promise<Map<string, string>> {
  const tree = await loadCategoryTree();
  const m = new Map<string, string>();
  for (const n of flattenCategories(tree)) {
    m.set(n.id, n.slug);
  }
  return m;
}

interface PagedResultWire<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

function qs(params: Record<string, string | number | boolean | undefined | null>): string {
  const u = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null || v === "") continue;
    u.set(k, String(v));
  }
  const s = u.toString();
  return s ? `?${s}` : "";
}

// ── Auth wire tipləri ───────────────────────────────────────────────────────

interface AuthUserWire {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

interface AuthResponseWire {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  user: AuthUserWire;
}

export interface CurrentUserWire {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string | null;
  role: string;
  isActive: boolean;
  emailVerified: boolean;
  phoneVerified: boolean;
  lastLoginAt?: string | null;
  createdAt: string;
}

export interface AddressWire {
  id: string;
  title: string;
  recipientName: string;
  phoneNumber: string;
  city: string;
  district: string;
  fullLine: string;
  postalCode?: string | null;
  countryCode: string;
  isDefault: boolean;
}

export interface UserProfileWire {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string | null;
  role: string;
  isActive: boolean;
  emailVerified: boolean;
  lastLoginAt?: string | null;
  createdAt: string;
  addresses: AddressWire[];
}

interface OtpWire {
  isVerified: boolean;
  message?: string | null;
  expiresAt?: string | null;
}

interface ValidateCouponWire {
  isValid: boolean;
  code: string;
  type: string;
  value: number;
  discountAmount: number;
  maxDiscountAmount?: number | null;
  message?: string | null;
}

interface WishlistWire {
  id: string;
  userId: string;
  itemCount: number;
  items: WishlistItemWire[];
}

interface ReviewWire {
  id: string;
  productId: string;
  userId: string;
  orderId?: string | null;
  reviewerName?: string | null;
  rating: number;
  title?: string | null;
  body?: string | null;
  status: string;
  rejectionReason?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

interface SellerApplicationResponseWire {
  id: string;
  fullName: string;
  company: string;
  email: string;
  status: string;
  rejectionReason?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

interface NotificationWire {
  id: string;
  title: string;
  body: string;
  isRead: boolean;
  createdAt: string;
}

// ── Products ───────────────────────────────────────────────────────────────

export const productApi = {
  async list(params?: {
    categoryId?: string;
    sellerId?: string;
    page?: number;
    pageSize?: number;
    isFeatured?: boolean;
    sortBy?: string;
    inStock?: boolean;
    /** GET /products?onPromotion=true — kataloq endirimi və ya aktiv satıcı kampaniyası. */
    onPromotion?: boolean;
  }): Promise<Product[]> {
    const slugMap = await categorySlugMap();
    const q = qs({
      categoryId: params?.categoryId,
      sellerId: params?.sellerId,
      page: params?.page ?? 1,
      pageSize: clampApiPageSize(params?.pageSize, 48),
      isFeatured: params?.isFeatured,
      sortBy: normalizeProductSortBy(params?.sortBy),
      inStock: params?.inStock ?? true,
      onPromotion: params?.onPromotion,
    });
    const page = await request<PagedResultWire<ProductSummaryWire>>(`/products${q}`);
    return page.items.map((i) => mapSummaryToProduct(i, slugMap));
  },

  async byCategorySlug(categorySlug: string, page = 1, pageSize = 48): Promise<Product[]> {
    const tree = await loadCategoryTree();
    const flat = flattenCategories(tree);
    const row = flat.find((c) => c.slug === categorySlug);
    if (!row) return [];
    return productApi.list({ categoryId: row.id, page, pageSize });
  },

  async bySlug(slug: string): Promise<Product | undefined> {
    const key = decodeURIComponent(slug).trim();
    let slugMap = new Map<string, string>();
    try {
      slugMap = await categorySlugMap();
    } catch {
      /* Kateqoriya ağacı uğursuz olsa belə məhsul detalı yüklənə bilsin */
    }

    try {
      const d = await request<ProductDetailWire>(
        `/products/${encodeURIComponent(key)}`
      );
      return mapDetailToProduct(d, slugMap);
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) return undefined;
      throw e;
    }
  },

  async featured(limit = 12): Promise<Product[]> {
    return productApi.list({ isFeatured: true, pageSize: limit, sortBy: "newest" });
  },

  async bestSellers(limit = 8): Promise<Product[]> {
    return productApi.list({ sortBy: "rating", pageSize: limit });
  },

  async discounted(limit = 48): Promise<Product[]> {
    const slugMap = await categorySlugMap();
    const q = qs({
      page: 1,
      pageSize: clampApiPageSize(Math.max(limit, 12), 12, API_MAX_PAGE_SIZE),
      sortBy: "rating",
      inStock: true,
      onPromotion: true,
    });
    const page = await request<PagedResultWire<ProductSummaryWire>>(`/products${q}`);
    const mapped = page.items.map((i) => mapSummaryToProduct(i, slugMap));
    const onPromo = mapped.filter(
      (p) =>
        (p.discount != null && p.discount > 0) ||
        (p.sellerCampaign != null && p.sellerCampaign.percent > 0)
    );
    return onPromo.slice(0, limit);
  },

  async search(query: string, page = 1, pageSize = 48): Promise<Product[]> {
    const slugMap = await categorySlugMap();
    const q = qs({ query, page, pageSize: clampApiPageSize(pageSize, 48) });
    const res = await request<PagedResultWire<ProductSummaryWire>>(`/products/search${q}`);
    return res.items.map((i) => mapSummaryToProduct(i, slugMap));
  },
};

export type SellerShopPublicWire = {
  sellerUserId: string;
  storeName: string;
  storeSlug: string;
  description: string | null;
  logoUrl: string | null;
  bannerUrl: string | null;
  ownerDisplayName: string | null;
  activeProductCount: number;
  createdAt: string;
};

/** GET seller-shops/view/{slug}/campaigns — aktiv planlar (ictimai). */
export type SellerCampaignPlanPublicWire = {
  name: string;
  kind: string;
  percent: number;
  startsAt: string;
  endsAt: string;
  minCartAmount: number | null;
  scope: unknown;
};

const SELLER_SHOPS_VIEW_PRIMARY = "/seller-shops/view";
const SELLER_SHOPS_VIEW_LEGACY = "/SellerShops/view";
const SELLER_SHOPS_ACTIVE_CAMPAIGNS = "/seller-shops/active-campaigns";
const SELLER_SHOPS_ACTIVE_CAMPAIGNS_LEGACY = "/SellerShops/active-campaigns";

/** GET seller-shops/active-campaigns — bütün vitrin mağazaları üçün aktiv planlar. */
export type SellerCampaignPlanHomeWire = SellerCampaignPlanPublicWire & {
  storeSlug: string;
  storeName: string;
};

export const sellerShopApi = {
  async getBySlug(slug: string): Promise<SellerShopPublicWire> {
    const key = decodeURIComponent(slug).trim();
    try {
      return await request<SellerShopPublicWire>(
        `${SELLER_SHOPS_VIEW_PRIMARY}/${encodeURIComponent(key)}`
      );
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        return request<SellerShopPublicWire>(
          `${SELLER_SHOPS_VIEW_LEGACY}/${encodeURIComponent(key)}`
        );
      }
      throw e;
    }
  },

  async listActiveCampaignsBySlug(slug: string): Promise<SellerCampaignPlanPublicWire[]> {
    const key = decodeURIComponent(slug).trim();
    try {
      return await request<SellerCampaignPlanPublicWire[]>(
        `${SELLER_SHOPS_VIEW_PRIMARY}/${encodeURIComponent(key)}/campaigns`
      );
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        return request<SellerCampaignPlanPublicWire[]>(
          `${SELLER_SHOPS_VIEW_LEGACY}/${encodeURIComponent(key)}/campaigns`
        );
      }
      throw e;
    }
  },

  async listActiveCampaignsForHome(): Promise<SellerCampaignPlanHomeWire[]> {
    try {
      return await request<SellerCampaignPlanHomeWire[]>(SELLER_SHOPS_ACTIVE_CAMPAIGNS);
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        return request<SellerCampaignPlanHomeWire[]>(SELLER_SHOPS_ACTIVE_CAMPAIGNS_LEGACY);
      }
      throw e;
    }
  },
};

// ── Categories ───────────────────────────────────────────────────────────────

export const categoryApi = {
  async list(): Promise<Category[]> {
    const tree = await loadCategoryTree();
    return flattenCategories(tree).map(wireCategoryToDisplay);
  },

  async tree(): Promise<CategoryWire[]> {
    return loadCategoryTree();
  },
};

// ── Auth ─────────────────────────────────────────────────────────────────────

export const authApi = {
  async login(email: string, password: string): Promise<AuthResponseWire> {
    return request<AuthResponseWire>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
  },

  async register(data: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phoneNumber?: string | null;
  }): Promise<AuthResponseWire> {
    return request<AuthResponseWire>("/auth/register", {
      method: "POST",
      body: JSON.stringify(data),
    });
  },

  async logout(refreshToken: string): Promise<void> {
    await request<undefined>("/auth/logout", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    });
  },

  async currentUser(): Promise<CurrentUserWire> {
    return request<CurrentUserWire>("/auth/me");
  },

  async forgotPassword(email: string): Promise<void> {
    await request<undefined>("/auth/forgot-password", {
      method: "POST",
      body: JSON.stringify({ email }),
    });
  },

  async resetPassword(token: string, newPassword: string, confirmNewPassword: string): Promise<void> {
    await request<undefined>("/auth/reset-password", {
      method: "POST",
      body: JSON.stringify({ token, newPassword, confirmNewPassword }),
    });
  },

  async sendOtp(phoneNumber: string): Promise<OtpWire> {
    return request<OtpWire>("/auth/otp/send", {
      method: "POST",
      body: JSON.stringify({ phoneNumber }),
    });
  },

  async verifyOtp(phoneNumber: string, code: string): Promise<OtpWire> {
    return request<OtpWire>("/auth/otp/verify", {
      method: "POST",
      body: JSON.stringify({ phoneNumber, code }),
    });
  },
};

// ── İstifadəçi profili və ünvanlar ──────────────────────────────────────────

export const userApi = {
  async getProfile(): Promise<UserProfileWire> {
    return request<UserProfileWire>("/users/me");
  },

  async updateProfile(firstName: string, lastName: string, phoneNumber?: string | null): Promise<UserProfileWire> {
    return request<UserProfileWire>("/users/me", {
      method: "PUT",
      body: JSON.stringify({ firstName, lastName, phoneNumber }),
    });
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await request<undefined>("/users/me/change-password", {
      method: "POST",
      body: JSON.stringify({
        currentPassword,
        newPassword,
        confirmNewPassword: newPassword,
      }),
    });
  },

  async addAddress(body: {
    title: string;
    recipientName: string;
    phoneNumber: string;
    city: string;
    district: string;
    fullLine: string;
    postalCode?: string | null;
    countryCode?: string;
    isDefault?: boolean;
  }): Promise<string> {
    const raw = await request<string>("/users/me/addresses", {
      method: "POST",
      body: JSON.stringify({
        countryCode: "AZ",
        ...body,
      }),
    });
    if (typeof raw === "string") return raw.replace(/^"|"$/g, "");
    return String(raw);
  },

  async updateAddress(
    id: string,
    body: {
      title: string;
      recipientName: string;
      phoneNumber: string;
      city: string;
      district: string;
      fullLine: string;
      postalCode?: string | null;
    }
  ): Promise<UserProfileWire> {
    return request<UserProfileWire>(`/users/me/addresses/${encodeURIComponent(id)}`, {
      method: "PUT",
      body: JSON.stringify(body),
    });
  },

  async deleteAddress(id: string): Promise<void> {
    await request<undefined>(`/users/me/addresses/${encodeURIComponent(id)}`, {
      method: "DELETE",
    });
  },
};

interface OrderSummaryWire {
  id: string;
  orderNumber: string;
  status: string;
  statusLabel: string;
  deliveryType: string;
  paymentMethod: string;
  paymentStatus: string;
  totalAmount: number;
  itemCount: number;
  primaryProductName?: string | null;
  primaryImageUrl?: string | null;
  canBeCancelled: boolean;
  canBeReturned: boolean;
  createdAt: string;
}

// ── Sifarişlər ───────────────────────────────────────────────────────────────

export const orderApi = {
  async list(page = 1, pageSize = 50): Promise<OrderSummaryWire[]> {
    const q = qs({
      page,
      pageSize: clampApiPageSize(pageSize, 50, USER_ORDERS_MAX_PAGE_SIZE),
    });
    const res = await request<PagedResultWire<OrderSummaryWire>>(`/orders${q}`);
    return res.items;
  },

  /** Siyahı + hər sifariş üçün tam detal (hesab səhifələri üçün). */
  async listDetailed(page = 1, pageSize = 50): Promise<Order[]> {
    const items = await orderApi.list(page, pageSize);
    const details = await Promise.all(
      items.map((s) => orderApi.getById(s.id).catch(() => null))
    );
    const orders = details.filter((x): x is Order => x !== null);
    return enrichOrdersWithProductImages(orders);
  },

  async getById(id: string): Promise<Order> {
    const o = await request<OrderResponseWire>(`/orders/${encodeURIComponent(id)}`);
    return mapOrderResponseWire(o);
  },

  async create(body: {
    addressId: string;
    deliveryType: string;
    paymentMethod: string;
    items: { productId: string; quantity: number }[];
    couponCode?: string | null;
    giftCardMessage?: string | null;
    giftCardSenderName?: string | null;
    giftCardOccasion?: string | null;
  }): Promise<string> {
    const raw = await request<string>("/orders", {
      method: "POST",
      body: JSON.stringify(body),
    });
    if (typeof raw === "string") return raw.replace(/^"|"$/g, "");
    return String(raw);
  },

  async cancel(id: string, reason: string): Promise<void> {
    await request<undefined>(`/orders/${encodeURIComponent(id)}/cancel`, {
      method: "POST",
      body: JSON.stringify({ reason }),
    });
  },

  async requestReturn(id: string, reason: string): Promise<void> {
    await request<undefined>(`/orders/${encodeURIComponent(id)}/return`, {
      method: "POST",
      body: JSON.stringify({ reason }),
    });
  },
};

// ── Wishlist ─────────────────────────────────────────────────────────────────

export const wishlistApi = {
  async get(): Promise<WishlistWire> {
    return request<WishlistWire>("/wishlist");
  },

  async add(productId: string): Promise<void> {
    await request<undefined>("/wishlist", {
      method: "POST",
      body: JSON.stringify({ productId }),
    });
  },

  async remove(productId: string): Promise<void> {
    await request<undefined>(`/wishlist/${encodeURIComponent(productId)}`, {
      method: "DELETE",
    });
  },
};

// ── Kuponlar ─────────────────────────────────────────────────────────────────

export const couponApi = {
  async validate(code: string, orderSubTotal: number): Promise<ValidateCouponWire> {
    const q = qs({ code, orderSubTotal });
    return request<ValidateCouponWire>(`/coupons/validate${q}`);
  },
};

// ── Rəylər ───────────────────────────────────────────────────────────────────

export const reviewApi = {
  async list(productId: string, page = 1, pageSize = 20): Promise<ReviewWire[]> {
    const q = qs({
      page,
      pageSize: clampApiPageSize(pageSize, 20, PRODUCT_PUBLIC_REVIEWS_MAX_PAGE_SIZE),
    });
    const res = await request<PagedResultWire<ReviewWire>>(
      `/products/${encodeURIComponent(productId)}/reviews${q}`
    );
    return res.items;
  },

  async create(
    productId: string,
    body: { rating: number; title?: string | null; body?: string | null; orderId?: string | null }
  ): Promise<string> {
    const raw = await request<string>(`/products/${encodeURIComponent(productId)}/reviews`, {
      method: "POST",
      body: JSON.stringify(body),
    });
    if (typeof raw === "string") return raw.replace(/^"|"$/g, "");
    return String(raw);
  },
};

// ── Bildirişlər (hesab) ──────────────────────────────────────────────────────

export const notificationsApi = {
  async list(page = 1, pageSize = 20): Promise<PagedResultWire<NotificationWire>> {
    const q = qs({
      page,
      pageSize: clampApiPageSize(pageSize, 20, NOTIFICATIONS_MAX_PAGE_SIZE),
    });
    return request<PagedResultWire<NotificationWire>>(`/notifications${q}`);
  },

  async markRead(notificationIds?: string[]): Promise<void> {
    await request<undefined>("/notifications/read", {
      method: "PATCH",
      body: JSON.stringify({
        notificationIds: notificationIds ?? [],
      }),
    });
  },
};

// ── Korporativ / satıcı ───────────────────────────────────────────────────────

export const corporateApi = {
  async submitInquiry(data: {
    company: string;
    contactName: string;
    email: string;
    phone: string;
    occasion: string;
    quantity: string;
    budget: string;
    message: string;
  }): Promise<void> {
    await request<undefined>("/corporate/inquiries", {
      method: "POST",
      body: JSON.stringify({
        company: data.company,
        contactName: data.contactName,
        email: data.email,
        phone: data.phone,
        occasion: data.occasion,
        quantity: data.quantity,
        budget: data.budget,
        message: data.message,
      }),
    });
  },
};

export const sellerApi = {
  async apply(data: {
    fullName: string;
    company: string;
    taxNumber: string;
    email: string;
    phone: string;
    city: string;
    category: string;
    message: string;
  }): Promise<SellerApplicationResponseWire> {
    return request<SellerApplicationResponseWire>("/seller-applications", {
      method: "POST",
      body: JSON.stringify(data),
    });
  },
};
