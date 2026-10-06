import type { CartItem } from "../types/cart";

export type OrderStatus =
  | "pending"
  | "confirmed"
  | "preparing"
  | "shipped"
  | "delivered"
  | "cancelled"
  | "return_requested"
  | "returned";

export type DeliveryType = "same_day" | "courier" | "cargo" | "pickup";

export type PaymentMethod = "card" | "cash" | "transfer";

export interface OrderItem {
  productId: string;
  productSlug: string;
  name: string;
  image: string;
  price: number;
  quantity: number;
  variantLabel?: string;
  addons?: { name: string; price: number }[];
}

export interface OrderAddress {
  recipientName: string;
  recipientPhone: string;
  city: string;
  district: string;
  line: string;
  postalCode?: string;
}

export interface OrderTracking {
  status: OrderStatus;
  timestamp: string;
  note?: string;
}

export interface Order {
  id: string;
  orderNumber?: string;
  userId?: string;
  guestEmail?: string;
  placedAt: string;
  status: OrderStatus;
  statusLabel?: string;
  canBeCancelled?: boolean;
  canBeReturned?: boolean;
  cancellationReason?: string;
  items: OrderItem[];
  address: OrderAddress;
  deliveryType: DeliveryType;
  deliveryDate: string;
  deliveryTimeSlot: string;
  notes: string;
  giftCardMessage?: string;
  giftCardSenderName?: string;
  giftCardOccasion?: string;
  paymentMethod: PaymentMethod;
  cardLast4?: string;
  installment?: number;
  couponCode?: string;
  couponDiscount: number;
  subtotal: number;
  deliveryFee: number;
  discount: number;
  totalPrice: number;
  invoiceRequested?: boolean;
  invoiceCompanyName?: string;
  invoiceTaxId?: string;
  tracking: OrderTracking[];
  returnReason?: string;
  returnRequestedAt?: string;
  reviewLeft?: boolean;
}

export function cartItemsToOrderItems(items: CartItem[]): OrderItem[] {
  return items.map((i) => ({
    productId: i.product.id,
    productSlug: i.product.slug,
    name: i.product.name,
    image: i.product.image,
    price: i.product.price,
    quantity: i.quantity,
  }));
}

export function orderStatusLabel(status: OrderStatus): string {
  const map: Record<OrderStatus, string> = {
    pending: "Gözləmədə",
    confirmed: "Təsdiqləndi",
    preparing: "Hazırlanır",
    shipped: "Yoldadır",
    delivered: "Çatdırılıb",
    cancelled: "Ləğv edilib",
    return_requested: "İadə sorğusu",
    returned: "İadə edilib",
  };
  return map[status] ?? status;
}

export function orderStatusTone(status: OrderStatus): string {
  const map: Record<OrderStatus, string> = {
    pending: "bg-amber-50 text-amber-800 border-amber-200",
    confirmed: "bg-blue-50 text-blue-700 border-blue-200",
    preparing: "bg-blue-50 text-blue-800 border-blue-200",
    shipped: "bg-violet-50 text-violet-800 border-violet-200",
    delivered: "bg-green-50 text-green-800 border-green-200",
    cancelled: "bg-gray-100 text-gray-600 border-gray-200",
    return_requested: "bg-lime-50 text-lime-900 border-lime-200",
    returned: "bg-teal-50 text-teal-700 border-teal-200",
  };
  return map[status] ?? "bg-gray-100 text-gray-600 border-gray-200";
}

export function deliveryTypeLabel(type: DeliveryType): string {
  const map: Record<DeliveryType, string> = {
    same_day: "Eyni gün kurye",
    courier: "Standart kurye",
    cargo: "Kargo (2-3 gün)",
    pickup: "Mağazadan götür",
  };
  return map[type];
}
