import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import type { Product } from "../data/products";
import { products } from "../data/products";
import { useToast } from "./ToastContext";

const STORAGE_KEY = "gulora_favorites";

function loadFavoriteIds(): number[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as unknown;
    if (!Array.isArray(parsed)) return [];
    return parsed.filter((x): x is number => typeof x === "number" && Number.isFinite(x));
  } catch {
    return [];
  }
}

interface FavoritesContextType {
  favoriteIds: readonly number[];
  favoriteProducts: Product[];
  favoriteCount: number;
  isFavorite: (productId: number) => boolean;
  toggleFavorite: (product: Product) => void;
}

const FavoritesContext = createContext<FavoritesContextType | undefined>(undefined);

export function FavoritesProvider({ children }: { children: ReactNode }) {
  const { showToast } = useToast();
  const [favoriteIds, setFavoriteIds] = useState<number[]>(loadFavoriteIds);

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(favoriteIds));
    } catch {
      /* ignore */
    }
  }, [favoriteIds]);

  const isFavorite = useCallback(
    (productId: number) => favoriteIds.includes(productId),
    [favoriteIds]
  );

  const toggleFavorite = useCallback((product: Product) => {
    setFavoriteIds((prev) => {
      if (prev.includes(product.id)) {
        showToast(`${product.name} seçilmişlərdən çıxarıldı`, "info");
        return prev.filter((id) => id !== product.id);
      }
      showToast(`${product.name} seçilmişlərə əlavə edildi`, "success");
      return [product.id, ...prev];
    });
  }, [showToast]);

  const favoriteProducts = useMemo(() => {
    const byId = new Map(products.map((p) => [p.id, p]));
    return favoriteIds
      .map((id) => byId.get(id))
      .filter((p): p is Product => p !== undefined);
  }, [favoriteIds]);

  const value = useMemo(
    () => ({
      favoriteIds,
      favoriteProducts,
      favoriteCount: favoriteProducts.length,
      isFavorite,
      toggleFavorite,
    }),
    [favoriteIds, favoriteProducts, isFavorite, toggleFavorite]
  );

  return (
    <FavoritesContext.Provider value={value}>{children}</FavoritesContext.Provider>
  );
}

export function useFavorites() {
  const ctx = useContext(FavoritesContext);
  if (!ctx) throw new Error("useFavorites must be used within FavoritesProvider");
  return ctx;
}
