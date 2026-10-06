import type { CartItem } from "./cart";

export type PaymentMethod = "card" | "cash" | "transfer";

export interface CompletedOrder {
  orderNumber: string;
  placedAt: string;
  recipientName: string;
  recipientPhone: string;
  recipientEmail: string;
  deliveryAddress: string;
  deliveryDate: string;
  deliveryTimeSlot: string;
  notes: string;
  paymentMethod: PaymentMethod;
  items: CartItem[];
  totalPrice: number;
  discount: number;
  shipping: number;
  finalPrice: number;
}

export const LAST_ORDER_STORAGE_KEY = "gulora_last_order";
