/**
 * API service layer.
 *
 * All data operations go through this module.  Currently backed by static
 * data and localStorage — swap each stub with a real fetch() / axios call
 * when the backend is ready without touching any UI component.
 *
 * Convention: every exported function returns a Promise so the calling code
 * is already async-ready.
 */

import {
  products,
  categories,
  getProductsByCategory,
  getProductBySlug,
  getFeaturedProducts,
  getBestSellers,
  getDiscountedProducts,
} from "../data/products";
import type { Product, Category } from "../data/products";

// ── Products ────────────────────────────────────────────────────────────────

export const productApi = {
  list: (): Promise<Product[]> => Promise.resolve([...products]),

  byCategory: (slug: string): Promise<Product[]> =>
    Promise.resolve(getProductsByCategory(slug)),

  bySlug: (slug: string): Promise<Product | undefined> =>
    Promise.resolve(getProductBySlug(slug)),

  featured: (): Promise<Product[]> => Promise.resolve(getFeaturedProducts()),

  bestSellers: (): Promise<Product[]> => Promise.resolve(getBestSellers()),

  discounted: (): Promise<Product[]> => Promise.resolve(getDiscountedProducts()),

  search: (query: string): Promise<Product[]> => {
    const q = query.toLowerCase().trim();
    return Promise.resolve(
      products.filter(
        (p) =>
          p.name.toLowerCase().includes(q) ||
          p.tags.some((t) => t.toLowerCase().includes(q)) ||
          p.category.toLowerCase().includes(q)
      )
    );
  },
};

// ── Categories ───────────────────────────────────────────────────────────────

export const categoryApi = {
  list: (): Promise<Category[]> => Promise.resolve([...categories]),
};

// ── Auth ─────────────────────────────────────────────────────────────────────
// TODO: replace stubs with POST /api/auth/login and POST /api/auth/register

export const authApi = {
  login: (
    _email: string,
    _password: string
  ): Promise<{ success: boolean; error?: string }> =>
    Promise.resolve({ success: false, error: "Backend inteqrasiyası tamamlanmamışdır." }),

  register: (_data: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
  }): Promise<{ success: boolean; error?: string }> =>
    Promise.resolve({ success: false, error: "Backend inteqrasiyası tamamlanmamışdır." }),

  sendOtp: (_phone: string): Promise<{ success: boolean }> =>
    Promise.resolve({ success: true }),

  verifyOtp: (_phone: string, _code: string): Promise<{ success: boolean }> =>
    Promise.resolve({ success: true }),
};

// ── Orders ───────────────────────────────────────────────────────────────────
// TODO: replace stubs with POST /api/orders and GET /api/orders

export const orderApi = {
  create: (_payload: unknown): Promise<{ id: string }> =>
    Promise.resolve({ id: `ORD-${Date.now()}` }),

  list: (_userId: string): Promise<unknown[]> => Promise.resolve([]),

  getById: (_orderId: string): Promise<unknown | null> =>
    Promise.resolve(null),
};

// ── Addresses ────────────────────────────────────────────────────────────────
// TODO: replace stubs with /api/addresses

export const addressApi = {
  list: (_userId: string): Promise<unknown[]> => Promise.resolve([]),

  create: (_data: unknown): Promise<{ id: string }> =>
    Promise.resolve({ id: `ADDR-${Date.now()}` }),

  update: (_id: string, _data: unknown): Promise<void> =>
    Promise.resolve(),

  delete: (_id: string): Promise<void> => Promise.resolve(),
};

// ── Notifications ─────────────────────────────────────────────────────────
// TODO: wire to real SMS/email gateway (e.g. Twilio, SendGrid)

export const notificationApi = {
  sendOrderConfirmation: (_orderId: string, _email: string): Promise<void> =>
    Promise.resolve(),

  sendShipmentUpdate: (_orderId: string, _phone: string): Promise<void> =>
    Promise.resolve(),
};

// ── Reviews ──────────────────────────────────────────────────────────────────
// Currently stored in localStorage; TODO: move to server-side + verified purchase check

export const reviewApi = {
  list: (productId: number): Promise<unknown[]> => {
    try {
      const raw = localStorage.getItem(`gulora_reviews_${productId}`);
      return Promise.resolve(raw ? (JSON.parse(raw) as unknown[]) : []);
    } catch {
      return Promise.resolve([]);
    }
  },

  create: (productId: number, review: unknown): Promise<void> => {
    try {
      const raw = localStorage.getItem(`gulora_reviews_${productId}`);
      const existing: unknown[] = raw ? (JSON.parse(raw) as unknown[]) : [];
      localStorage.setItem(
        `gulora_reviews_${productId}`,
        JSON.stringify([review, ...existing])
      );
    } catch {
      /* ignore */
    }
    return Promise.resolve();
  },
};

// ── Corporate / B2B ──────────────────────────────────────────────────────────
// TODO: wire to CRM / sales pipeline

export const corporateApi = {
  submitInquiry: (data: {
    company: string;
    contactName: string;
    email: string;
    phone: string;
    occasion: string;
    quantity: string;
    budget: string;
    message: string;
  }): Promise<{ success: boolean }> => {
    console.info("[corporateApi.submitInquiry] — TODO send to CRM:", data);
    return new Promise((resolve) => setTimeout(() => resolve({ success: true }), 800));
  },
};

// ── Seller onboarding ────────────────────────────────────────────────────────
// TODO: wire to seller management system

export const sellerApi = {
  apply: (data: {
    fullName: string;
    company: string;
    taxNumber: string;
    email: string;
    phone: string;
    city: string;
    category: string;
    message: string;
  }): Promise<{ success: boolean }> => {
    console.info("[sellerApi.apply] — TODO send to seller portal:", data);
    return new Promise((resolve) => setTimeout(() => resolve({ success: true }), 800));
  },
};
