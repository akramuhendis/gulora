import type { Product } from "../data/products";

export interface CartItem {
  product: Product;
  quantity: number;
  variantLabel?: string;
  addons?: { name: string; price: number }[];
}
