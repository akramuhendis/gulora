import { createContext, useContext, useState, useEffect, useCallback } from "react";
import type { ReactNode } from "react";
import type { CartItem } from "./CartContext";
import { useAuth } from "./AuthContext";

export type OrderStatus = "pending" | "confirmed" | "preparing" | "shipped" | "delivered" | "cancelled" | "return_requested" | "returned";

export type DeliveryType = "same_day" | "courier" | "cargo" | "pickup";

export type PaymentMethod = "card" | "cash" | "transfer";

export interface OrderItem {
  productId: number;
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
  userId?: string;
  guestEmail?: string;
  placedAt: string;
  status: OrderStatus;
  items: OrderItem[];
  address: OrderAddress;
  deliveryType: DeliveryType;
  deliveryDate: string;
  deliveryTimeSlot: string;
  notes: string;
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

interface OrderContextType {
  orders: Order[];
  addOrder: (order: Order) => void;
  getOrder: (id: string) => Order | undefined;
  requestReturn: (orderId: string, reason: string) => void;
  cancelOrder: (orderId: string) => void;
  markReviewed: (orderId: string) => void;
}

const OrderContext = createContext<OrderContextType | undefined>(undefined);

function storageKey(userId?: string) {
  return userId ? `gulora_orders_${userId}` : "gulora_orders_guest";
}

function loadOrders(userId?: string): Order[] {
  try {
    const raw = localStorage.getItem(storageKey(userId));
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveOrders(orders: Order[], userId?: string) {
  localStorage.setItem(storageKey(userId), JSON.stringify(orders));
}

function cartItemsToOrderItems(items: CartItem[]): OrderItem[] {
  return items.map((i) => ({
    productId: i.product.id,
    productSlug: i.product.slug,
    name: i.product.name,
    image: i.product.image,
    price: i.product.price,
    quantity: i.quantity,
  }));
}

export { cartItemsToOrderItems };

export function OrderProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [orders, setOrders] = useState<Order[]>(() => loadOrders(user?.id));

  useEffect(() => {
    setOrders(loadOrders(user?.id));
  }, [user?.id]);

  useEffect(() => {
    saveOrders(orders, user?.id);
  }, [orders, user?.id]);

  const addOrder = useCallback((order: Order) => {
    setOrders((prev) => [order, ...prev]);
  }, []);

  const getOrder = useCallback((id: string) => {
    return orders.find((o) => o.id === id);
  }, [orders]);

  const requestReturn = useCallback((orderId: string, reason: string) => {
    setOrders((prev) =>
      prev.map((o) =>
        o.id === orderId
          ? {
              ...o,
              status: "return_requested",
              returnReason: reason,
              returnRequestedAt: new Date().toISOString(),
              tracking: [
                ...o.tracking,
                { status: "return_requested" as OrderStatus, timestamp: new Date().toISOString(), note: `İadə sorğusu: ${reason}` },
              ],
            }
          : o
      )
    );
  }, []);

  const cancelOrder = useCallback((orderId: string) => {
    setOrders((prev) =>
      prev.map((o) =>
        o.id === orderId && (o.status === "pending" || o.status === "confirmed")
          ? {
              ...o,
              status: "cancelled",
              tracking: [
                ...o.tracking,
                { status: "cancelled" as OrderStatus, timestamp: new Date().toISOString(), note: "Müştəri tərəfindən ləğv edildi." },
              ],
            }
          : o
      )
    );
  }, []);

  const markReviewed = useCallback((orderId: string) => {
    setOrders((prev) => prev.map((o) => (o.id === orderId ? { ...o, reviewLeft: true } : o)));
  }, []);

  return (
    <OrderContext.Provider value={{ orders, addOrder, getOrder, requestReturn, cancelOrder, markReviewed }}>
      {children}
    </OrderContext.Provider>
  );
}

export function useOrders() {
  const ctx = useContext(OrderContext);
  if (!ctx) throw new Error("useOrders must be used within OrderProvider");
  return ctx;
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
    return_requested: "bg-orange-50 text-orange-700 border-orange-200",
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
