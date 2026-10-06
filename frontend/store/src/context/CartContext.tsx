import { useState, useCallback, useEffect } from "react";
import type { ReactNode } from "react";
import type { Product } from "../data/products";
import { useToast } from "../hooks/useToast";
import { CartContext } from "./cart-context";

const CART_STORAGE_KEY = "gulora_store_cart_v1";

type StoredCartItem = {
  product: Product;
  quantity: number;
  variantLabel?: string;
  addons?: { name: string; price: number }[];
};

function loadCart(): StoredCartItem[] {
  try {
    const raw = localStorage.getItem(CART_STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as StoredCartItem[];
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function CartProvider({ children }: { children: ReactNode }) {
  const { showToast } = useToast();
  const [items, setItems] = useState<StoredCartItem[]>(() => loadCart());
  const [couponCode, setCouponCode] = useState("");
  const [couponLabel, setCouponLabel] = useState("");
  const [couponDiscount, setCouponDiscountAmt] = useState(0);

  useEffect(() => {
    localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
  }, [items]);

  const addToCart = (
    product: Product,
    quantity = 1,
    variantLabel?: string,
    addons?: { name: string; price: number }[]
  ) => {
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

  const removeFromCart = (productId: string) => {
    setItems((prev) => prev.filter((item) => item.product.id !== productId));
  };

  const updateQuantity = (productId: string, quantity: number) => {
    if (quantity <= 0) {
      removeFromCart(productId);
      return;
    }
    setItems((prev) =>
      prev.map((item) => (item.product.id === productId ? { ...item, quantity } : item))
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

  const setCoupon = useCallback(
    (_code: string) => ({
      ok: false as const,
      error: "Kupon ödəniş səhifəsində API ilə təsdiqlənir.",
    }),
    []
  );

  const removeCoupon = () => {
    setCouponCode("");
    setCouponLabel("");
    setCouponDiscountAmt(0);
  };

  return (
    <CartContext.Provider
      value={{
        items,
        addToCart,
        removeFromCart,
        updateQuantity,
        clearCart,
        totalItems,
        totalPrice,
        couponCode,
        couponLabel,
        couponDiscount,
        setCoupon,
        removeCoupon,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}
