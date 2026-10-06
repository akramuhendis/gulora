import { createContext } from "react";
import type { CartItem } from "../types/cart";

export interface CartContextType {
  items: CartItem[];
  addToCart: (
    product: CartItem["product"],
    quantity?: number,
    variantLabel?: string,
    addons?: { name: string; price: number }[]
  ) => void;
  removeFromCart: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  totalItems: number;
  totalPrice: number;
  couponCode: string;
  couponLabel: string;
  couponDiscount: number;
  setCoupon: (code: string) => { ok: boolean; error?: string; label?: string; discountAmount?: number };
  removeCoupon: () => void;
}

export const CartContext = createContext<CartContextType | undefined>(undefined);
