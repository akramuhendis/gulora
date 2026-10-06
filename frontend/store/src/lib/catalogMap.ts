import type { Category, Product } from "../data/products";
import type { DeliveryType, Order, OrderItem, OrderStatus, PaymentMethod } from "../models/order";
import { resolveMediaUrlOrFallback } from "./mediaUrl";

/** Backend CategoriesController JSON — camelCase. */
export interface CategoryWire {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  imageUrl?: string | null;
  parentId?: string | null;
  displayOrder: number;
  isActive: boolean;
  children: CategoryWire[];
}

export interface ProductSummaryWire {
  id: string;
  name: string;
  slug: string;
  shortDescription?: string | null;
  price: number;
  discountedPrice?: number | null;
  effectivePrice: number;
  stockQuantity: number;
  /** Əksər cavablarda mövcuddur; köhnə backend uyğunluğu üçün opsional. */
  isActive?: boolean;
  isAvailableForPurchase: boolean;
  brand?: string | null;
  categoryId: string;
  categoryName: string;
  isFeatured: boolean;
  averageRating: number;
  reviewCount: number;
  primaryImageUrl?: string | null;
  createdAt: string;
  /** GET /products siyahısı — aktiv satıcı faiz kampaniyası (opsional). */
  sellerCampaign?: {
    name: string;
    percent: number;
    priceAfterDiscount: number;
    minimumCartAmount?: number | null;
    kind?: string | null;
  } | null;
}

export interface ProductImageWire {
  url: string;
  altText?: string | null;
  displayOrder: number;
}

export interface ProductDetailWire extends Omit<ProductSummaryWire, "shortDescription"> {
  description?: string | null;
  shortDescription?: string | null;
  sku?: string | null;
  sellerId?: string | null;
  isActive: boolean;
  primaryImageUrl?: string | null;
  images: ProductImageWire[];
  updatedAt?: string | null;
  sellerShop?: {
    sellerUserId: string;
    storeName: string;
    storeSlug: string;
    logoUrl?: string | null;
  } | null;
  /** Satıcı panelindəki aktiv faiz kampaniyası (backend). */
  sellerCampaign?: {
    name: string;
    percent: number;
    priceAfterDiscount: number;
    minimumCartAmount?: number | null;
    kind?: string | null;
  } | null;
}

export const FALLBACK_IMG =
  "https://images.unsplash.com/photo-1490750967868-88df5691cc6e?w=400&q=80";

const CAT_PALETTE = [
  "#FF6B9D",
  "#9B59B6",
  "#E74C3C",
  "#27AE60",
  "#8B4513",
  "#D35400",
  "#F39C12",
  "#E67E22",
  "#1ABC9C",
  "#2C3E50",
];

export function colorFromKey(key: string): string {
  let h = 0;
  for (let i = 0; i < key.length; i++) h = (h * 31 + key.charCodeAt(i)) >>> 0;
  return CAT_PALETTE[h % CAT_PALETTE.length];
}

const SLUG_EMOJI: Record<string, string> = {
  cicekler: "🌸",
  orkide: "🌺",
  "gul-buketi": "🌹",
  "saksi-cicegi": "🪴",
  "gurme-lezzet": "🍫",
  cikolata: "🍬",
  "kek-pasta": "🎂",
  "pelus-oyuncak": "🧸",
  "kisiye-ozel": "🎁",
  teknoloji: "📱",
};

export function iconForSlug(slug: string): string {
  return SLUG_EMOJI[slug] ?? "🛍️";
}

export function flattenCategories(nodes: CategoryWire[]): CategoryWire[] {
  const out: CategoryWire[] = [];
  const walk = (n: CategoryWire) => {
    out.push(n);
    for (const c of n.children ?? []) walk(c);
  };
  for (const r of nodes) walk(r);
  return out;
}

export function wireCategoryToDisplay(wire: CategoryWire): Category {
  return {
    id: wire.id,
    name: wire.name,
    slug: wire.slug,
    icon: iconForSlug(wire.slug),
    color: colorFromKey(wire.slug),
    image: resolveMediaUrlOrFallback(wire.imageUrl, FALLBACK_IMG),
  };
}

function pickFiniteNumber(v: unknown): number {
  if (typeof v === "number" && Number.isFinite(v)) return v;
  if (typeof v === "string") {
    const n = Number(v.replace(",", "."));
    return Number.isFinite(n) ? n : NaN;
  }
  return NaN;
}

/** Siyahı JSON-u — camelCase/PascalCase və string rəqəmlər üçün. */
function parseSellerCampaignWire(raw: unknown): ProductSummaryWire["sellerCampaign"] {
  if (!raw || typeof raw !== "object") return undefined;
  const o = raw as Record<string, unknown>;
  const name = o.name ?? o.Name;
  const percent = pickFiniteNumber(o.percent ?? o.Percent);
  const priceAfterDiscount = pickFiniteNumber(o.priceAfterDiscount ?? o.PriceAfterDiscount);
  const minRaw = o.minimumCartAmount ?? o.MinimumCartAmount;
  const kindRaw = o.kind ?? o.Kind;
  const kindStr = typeof kindRaw === "string" ? kindRaw.trim().toLowerCase() : "";
  const kind: "discount" | "shipping" = kindStr === "shipping" ? "shipping" : "discount";
  if (typeof name !== "string" || !name.trim()) return undefined;
  if (!Number.isFinite(percent) || !Number.isFinite(priceAfterDiscount)) return undefined;
  let minimumCartAmount: number | null = null;
  if (minRaw != null) {
    const m = pickFiniteNumber(minRaw);
    if (Number.isFinite(m)) minimumCartAmount = m;
  }
  return {
    name: name.trim(),
    percent: Math.round(percent),
    priceAfterDiscount,
    minimumCartAmount,
    kind,
  };
}

export function mapSummaryToProduct(
  s: ProductSummaryWire,
  categorySlugById: Map<string, string>
): Product {
  const priceNum = pickFiniteNumber(s.price);
  const eff = pickFiniteNumber(s.effectivePrice);
  const discPrice =
    s.discountedPrice != null ? pickFiniteNumber(s.discountedPrice) : null;

  let discount: number | undefined;
  let sellingPrice = eff;
  let originalPrice: number | undefined;
  if (discPrice != null && Number.isFinite(discPrice) && priceNum > discPrice) {
    originalPrice = priceNum;
    sellingPrice = discPrice;
    discount = Math.round((1 - discPrice / priceNum) * 100);
  }

  const img = resolveMediaUrlOrFallback(s.primaryImageUrl, FALLBACK_IMG);
  const categorySlug =
    categorySlugById.get(s.categoryId) ?? s.categoryName.toLowerCase().replace(/\s+/g, "-");

  const base: Product = {
    id: s.id,
    backendCategoryId: s.categoryId,
    name: s.name,
    slug: s.slug,
    price: sellingPrice,
    originalPrice,
    discount,
    rating: s.averageRating,
    reviewCount: s.reviewCount,
    image: img,
    images: [img],
    category: s.categoryName,
    categorySlug,
    tags: s.brand ? [s.brand] : [],
    badge: s.isFeatured ? "Öne çıxan" : undefined,
    isSameDay: false,
    isFreeDelivery: s.isAvailableForPurchase,
    description: s.shortDescription ?? "",
    stock: s.stockQuantity,
  };

  const raw = s as unknown as Record<string, unknown>;
  const sc =
    parseSellerCampaignWire(raw.sellerCampaign ?? raw.SellerCampaign) ?? s.sellerCampaign;

  return {
    ...base,
    ...applySellerCampaignPricing(pickFiniteNumber(s.effectivePrice), sc),
  };
}

export function mapDetailToProduct(
  d: ProductDetailWire,
  categorySlugById: Map<string, string>
): Product {
  const base = mapSummaryToProduct(d, categorySlugById);
  const imgs =
    d.images?.length > 0
      ? [...d.images].sort((a, b) => a.displayOrder - b.displayOrder).map((i) => i.url)
      : base.images;

  return {
    ...base,
    images: imgs.length > 0 ? imgs : base.images,
    image: imgs[0] ?? base.image,
    description: d.description ?? d.shortDescription ?? base.description,
    stock: d.stockQuantity,
    badge: d.isFeatured ? "Öne çıxan" : base.badge,
    sellerShop: d.sellerShop
      ? {
          sellerUserId: d.sellerShop.sellerUserId,
          storeName: d.sellerShop.storeName,
          storeSlug: d.sellerShop.storeSlug,
          logoUrl: d.sellerShop.logoUrl,
        }
      : undefined,
  };
}

/** Kataloq qiyməti (effectivePrice) üzərindən satıcı kampaniyası — siyahı və detal üçün eyni məntiq. */
function applySellerCampaignPricing(
  effectivePrice: number,
  sc:
    | {
        name: string;
        percent: number;
        priceAfterDiscount: number;
        minimumCartAmount?: number | null;
        kind?: string | null;
      }
    | null
    | undefined
): Partial<Product> {
  if (!sc) return {};

  const line = effectivePrice;
  const after = pickFiniteNumber(sc.priceAfterDiscount);
  const campaignKind: "discount" | "shipping" = sc.kind === "shipping" ? "shipping" : "discount";
  const sellerCampaign = {
    name: sc.name,
    percent: sc.percent,
    priceAfterDiscount: after,
    minimumCartAmount: sc.minimumCartAmount ?? null,
    kind: campaignKind,
  };

  if (campaignKind === "shipping") {
    return { sellerCampaign };
  }

  if (!Number.isFinite(line) || !Number.isFinite(after) || after >= line - 1e-9) {
    return { sellerCampaign };
  }

  return {
    price: after,
    originalPrice: line,
    discount: sc.percent,
    sellerCampaign,
  };
}

export interface OrderResponseWire {
  id: string;
  orderNumber: string;
  status: string;
  statusLabel: string;
  deliveryType: string;
  paymentMethod: string;
  paymentStatus: string;
  subTotal: number;
  deliveryFee: number;
  discountAmount: number;
  totalAmount: number;
  couponCode?: string | null;
  trackingNumber?: string | null;
  cargoCompany?: string | null;
  cancellationReason?: string | null;
  deliveredAt?: string | null;
  canBeCancelled: boolean;
  canBeReturned: boolean;
  createdAt: string;
  updatedAt?: string | null;
  giftCardMessage?: string | null;
  giftCardSenderName?: string | null;
  giftCardOccasion?: string | null;
  address: {
    recipientName: string;
    phoneNumber: string;
    city: string;
    district: string;
    fullLine: string;
    postalCode?: string | null;
    countryCode: string;
  };
  items: Array<{
    id: string;
    productId: string;
    productName: string;
    productSku?: string | null;
    productImageUrl?: string | null;
    unitPrice: number;
    quantity: number;
    lineTotal: number;
  }>;
  trackings: Array<{
    id: string;
    status: string;
    statusLabel: string;
    note?: string | null;
    cargoCompany?: string | null;
    trackingNumber?: string | null;
    changedByUserId?: string | null;
    createdAt: string;
  }>;
}

export function mapApiDeliveryToFrontend(raw: string): DeliveryType {
  switch (normalizeWireEnumKey(raw)) {
    case "sameDay":
      return "same_day";
    case "express":
      return "cargo";
    case "standard":
    default:
      return "courier";
  }
}

export function mapFrontendDeliveryToApi(t: DeliveryType): string {
  switch (t) {
    case "same_day":
      return "sameDay";
    case "cargo":
      return "express";
    case "pickup":
      return "standard";
    case "courier":
    default:
      return "standard";
  }
}

export function mapApiPaymentToFrontend(raw: string): PaymentMethod {
  switch (normalizeWireEnumKey(raw)) {
    case "cashOnDelivery":
      return "cash";
    case "bankTransfer":
      return "transfer";
    case "creditCard":
    case "debitCard":
    default:
      return "card";
  }
}

export function mapFrontendPaymentToApi(m: PaymentMethod): string {
  switch (m) {
    case "cash":
      return "cashOnDelivery";
    case "transfer":
      return "bankTransfer";
    case "card":
    default:
      return "creditCard";
  }
}

/**
 * Backend record sahələri çox vaxt enum üçün `.ToString()` ilə **PascalCase** (məs. Pending,
 * ReturnRequested) qaytarır; JSON enum konvertoru isə camelCase verir. UI xəritəsi üçün
 * açarı vahid camelCase formata salırıq.
 */
function normalizeWireEnumKey(raw: string): string {
  const s = raw.trim();
  if (!s) return "";
  return s.charAt(0).toLowerCase() + s.slice(1);
}

export function mapApiStatusToFrontend(raw: string): OrderStatus {
  const key = normalizeWireEnumKey(raw);
  const map: Record<string, OrderStatus> = {
    pending: "pending",
    confirmed: "confirmed",
    processing: "preparing",
    shipped: "shipped",
    delivered: "delivered",
    cancelled: "cancelled",
    returnRequested: "return_requested",
    returned: "returned",
  };
  return map[key] ?? "pending";
}

export function mapOrderResponseWire(o: OrderResponseWire): Order {
  const placedDate = o.createdAt.slice(0, 10);
  const items: OrderItem[] = o.items.map((i) => ({
    productId: i.productId,
    productSlug: "",
    name: i.productName,
    image: resolveMediaUrlOrFallback(i.productImageUrl, FALLBACK_IMG),
    price: Number(i.unitPrice),
    quantity: i.quantity,
  }));

  const tracking = o.trackings.map((t) => ({
    status: mapApiStatusToFrontend(t.status),
    timestamp: t.createdAt,
    note: t.note ?? undefined,
  }));

  return {
    id: o.id,
    orderNumber: o.orderNumber,
    placedAt: o.createdAt,
    status: mapApiStatusToFrontend(o.status),
    statusLabel: o.statusLabel,
    canBeCancelled: o.canBeCancelled,
    canBeReturned: o.canBeReturned,
    cancellationReason: o.cancellationReason ?? undefined,
    items,
    address: {
      recipientName: o.address.recipientName,
      recipientPhone: o.address.phoneNumber,
      city: o.address.city,
      district: o.address.district,
      line: o.address.fullLine,
      postalCode: o.address.postalCode ?? undefined,
    },
    deliveryType: mapApiDeliveryToFrontend(o.deliveryType),
    deliveryDate: placedDate,
    deliveryTimeSlot: "",
    notes: "",
    paymentMethod: mapApiPaymentToFrontend(o.paymentMethod),
    couponCode: o.couponCode ?? undefined,
    couponDiscount: Number(o.discountAmount),
    subtotal: Number(o.subTotal),
    deliveryFee: Number(o.deliveryFee),
    discount: Number(o.discountAmount),
    totalPrice: Number(o.totalAmount),
    tracking: tracking.length > 0 ? tracking : [{ status: mapApiStatusToFrontend(o.status), timestamp: o.createdAt }],
    returnReason: undefined,
    giftCardMessage: o.giftCardMessage ?? undefined,
    giftCardSenderName: o.giftCardSenderName ?? undefined,
    giftCardOccasion: o.giftCardOccasion ?? undefined,
  };
}

export interface WishlistItemWire {
  productId: string;
  productName: string;
  productSlug: string;
  effectivePrice: number;
  primaryImageUrl?: string | null;
  isAvailable: boolean;
}

export function wishlistItemToProduct(row: WishlistItemWire): Product {
  const img = row.primaryImageUrl ?? FALLBACK_IMG;
  return {
    id: row.productId,
    name: row.productName,
    slug: row.productSlug,
    price: Number(row.effectivePrice),
    rating: 0,
    reviewCount: 0,
    image: img,
    images: [img],
    category: "",
    categorySlug: "mehsullar",
    tags: [],
    isSameDay: false,
    isFreeDelivery: row.isAvailable,
    description: "",
    stock: row.isAvailable ? 1 : 0,
  };
}
