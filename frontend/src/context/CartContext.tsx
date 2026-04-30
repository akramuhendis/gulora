import { createContext, useContext, useState } from "react";
import type { ReactNode } from "react";
import type { Product } from "../data/products";
import { applyCoupon } from "../lib/cartPricing";
import { useToast } from "./ToastContext";

export interface CartItem {
  product: Product;
  quantity: number;
  variantLabel?: string;
  addons?: { name: string; price: number }[];
}

interface CartContextType {
  items: CartItem[];
  addToCart: (product: Product, quantity?: number, variantLabel?: string, addons?: { name: string; price: number }[]) => void;
  removeFromCart: (productId: number) => void;
  updateQuantity: (productId: number, quantity: number) => void;
  clearCart: () => void;
  totalItems: number;
  totalPrice: number;
  couponCode: string;
  couponLabel: string;
  couponDiscount: number;
  setCoupon: (code: string) => { ok: boolean; error?: string; label?: string; discountAmount?: number };
  removeCoupon: () => void;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export function CartProvider({ children }: { children: ReactNode }) {
  const { showToast } = useToast();
  const [items, setItems] = useState<CartItem[]>([]);
  const [couponCode, setCouponCode] = useState("");
  const [couponLabel, setCouponLabel] = useState("");
  const [couponDiscount, setCouponDiscountAmt] = useState(0);

  const addToCart = (product: Product, quantity = 1, variantLabel?: string, addons?: { name: string; price: number }[]) => {
    setItems((prev) => {
      const existing = prev.find((item) => item.product.id === product.id && item.variantLabel === variantLabel);
      if (existing) {
        return prev.map((item) =>
          item.product.id === product.id && item.variantLabel === variantLabel
            ? { ...item, quantity: item.quantity + quantity }
            : item
        );
      }
      return [...prev, { product, quantity, variantLabel, addons }];
    });
    showToast(`${product.name} səbətə əlavə edildi`, "success");
  };

  const removeFromCart = (productId: number) => {
    setItems((prev) => prev.filter((item) => item.product.id !== productId));
  };

  const updateQuantity = (productId: number, quantity: number) => {
    if (quantity <= 0) {
      removeFromCart(productId);
      return;
    }
    setItems((prev) =>
      prev.map((item) =>
        item.product.id === productId ? { ...item, quantity } : item
      )
    );
  };

  const clearCart = () => {
    setItems([]);
    setCouponCode("");
    setCouponLabel("");
    setCouponDiscountAmt(0);
  };

  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);
  const totalPrice = items.reduce((sum, item) => {
    const addonTotal = (item.addons ?? []).reduce((a, b) => a + b.price, 0);
    return sum + (item.product.price + addonTotal) * item.quantity;
  }, 0);

  const setCoupon = (code: string) => {
    const result = applyCoupon(code, totalPrice);
    if (result.ok && result.discountAmount !== undefined && result.label) {
      setCouponCode(code.toUpperCase().trim());
      setCouponLabel(result.label);
      setCouponDiscountAmt(result.discountAmount);
    }
    return result;
  };

  const removeCoupon = () => {
    setCouponCode("");
    setCouponLabel("");
    setCouponDiscountAmt(0);
  };

  return (
    <CartContext.Provider
      value={{
        items, addToCart, removeFromCart, updateQuantity, clearCart,
        totalItems, totalPrice,
        couponCode, couponLabel, couponDiscount,
        setCoupon, removeCoupon,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error("useCart must be used within CartProvider");
  return ctx;
}
