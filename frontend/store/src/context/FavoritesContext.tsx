import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  startTransition,
  type ReactNode,
} from "react";
import type { Product } from "../data/products";
import { products } from "../data/products";
import { wishlistApi } from "../services/api";
import { wishlistItemToProduct } from "../lib/catalogMap";
import { useAuth } from "../hooks/useAuth";
import { useToast } from "../hooks/useToast";
import { FavoritesContext } from "./favorites-context";

const STORAGE_KEY = "gulora_favorites";

function normalizeFavoriteIds(raw: unknown): string[] {
  if (!Array.isArray(raw)) return [];
  return raw
    .map((x) => (typeof x === "number" ? String(x) : typeof x === "string" ? x : ""))
    .filter((x) => x.length > 0);
}

function loadFavoriteIds(): string[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    return normalizeFavoriteIds(JSON.parse(raw));
  } catch {
    return [];
  }
}

export function FavoritesProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const { showToast } = useToast();
  const [favoriteIds, setFavoriteIds] = useState<string[]>([]);
  const [wishlistProducts, setWishlistProducts] = useState<Product[]>([]);

  useEffect(() => {
    if (!user) {
      startTransition(() => {
        setFavoriteIds(loadFavoriteIds());
        setWishlistProducts([]);
      });
      return;
    }
    let cancelled = false;
    wishlistApi
      .get()
      .then((w) => {
        if (cancelled) return;
        setFavoriteIds(w.items.map((i) => i.productId));
        setWishlistProducts(w.items.map((i) => wishlistItemToProduct(i)));
      })
      .catch(() => {
        if (!cancelled) {
          setFavoriteIds([]);
          setWishlistProducts([]);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [user]);

  useEffect(() => {
    if (user) return;
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(favoriteIds));
    } catch {
      /* ignore */
    }
  }, [favoriteIds, user]);

  const isFavorite = useCallback(
    (productId: string) => favoriteIds.includes(productId),
    [favoriteIds]
  );

  const toggleFavorite = useCallback(
    async (product: Product) => {
      if (user) {
        try {
          if (favoriteIds.includes(product.id)) {
            await wishlistApi.remove(product.id);
            showToast(`${product.name} seçilmişlərdən çıxarıldı`, "info");
            setFavoriteIds((prev) => prev.filter((id) => id !== product.id));
            setWishlistProducts((prev) => prev.filter((p) => p.id !== product.id));
          } else {
            await wishlistApi.add(product.id);
            showToast(`${product.name} seçilmişlərə əlavə edildi`, "success");
            setFavoriteIds((prev) => [product.id, ...prev.filter((id) => id !== product.id)]);
            setWishlistProducts((prev) => [
              wishlistItemToProduct({
                productId: product.id,
                productName: product.name,
                productSlug: product.slug,
                effectivePrice: product.price,
                primaryImageUrl: product.image,
                isAvailable: product.stock > 0,
              }),
              ...prev.filter((p) => p.id !== product.id),
            ]);
          }
        } catch {
          showToast("Seçilmişlər yenilənmədi — yenidən cəhd edin.", "error");
        }
        return;
      }

      setFavoriteIds((prev) => {
        if (prev.includes(product.id)) {
          showToast(`${product.name} seçilmişlərdən çıxarıldı`, "info");
          return prev.filter((id) => id !== product.id);
        }
        showToast(`${product.name} seçilmişlərə əlavə edildi`, "success");
        return [product.id, ...prev];
      });
    },
    [user, favoriteIds, showToast]
  );

  const favoriteProducts = useMemo(() => {
    if (user) return wishlistProducts;
    const byId = new Map(products.map((p) => [p.id, p]));
    return favoriteIds.map((id) => byId.get(id)).filter((p): p is Product => p !== undefined);
  }, [user, favoriteIds, wishlistProducts]);

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

  return <FavoritesContext.Provider value={value}>{children}</FavoritesContext.Provider>;
}
