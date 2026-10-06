import { createContext } from "react";
import type { Product } from "../data/products";

export interface FavoritesContextType {
  favoriteIds: readonly string[];
  favoriteProducts: Product[];
  favoriteCount: number;
  isFavorite: (productId: string) => boolean;
  toggleFavorite: (product: Product) => Promise<void>;
}

export const FavoritesContext = createContext<FavoritesContextType | undefined>(undefined);
