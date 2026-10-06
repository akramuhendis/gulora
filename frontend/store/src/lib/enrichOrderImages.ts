import type { Order } from "../models/order";
import { productApi } from "../services/api";
import { FALLBACK_IMG } from "./catalogMap";
import { resolveMediaUrlOrFallback } from "./mediaUrl";

/**
 * Sifariş sətirlərinə kataloqdan kapak şəkli əlavə edir (API/localStorage boş olanda).
 * Bütün URL-lər resolveMediaUrl ilə normallaşdırılır.
 */
export async function enrichOrdersWithProductImages(orders: Order[]): Promise<Order[]> {
  if (orders.length === 0) return orders;

  const productIds = [...new Set(orders.flatMap((o) => o.items.map((i) => i.productId)))];
  const imageByProductId = new Map<string, string>();

  try {
    const products = await productApi.list({ pageSize: 100 });
    for (const p of products) {
      if (productIds.includes(p.id) && p.image) {
        imageByProductId.set(p.id, p.image);
      }
    }
  } catch {
    /* kataloq əlçatan deyilsə mövcud URL ilə davam */
  }

  return orders.map((order) => ({
    ...order,
    items: order.items.map((item) => {
      const catalogImage = imageByProductId.get(item.productId);
      const raw = catalogImage ?? item.image;
      return {
        ...item,
        image: resolveMediaUrlOrFallback(raw, FALLBACK_IMG),
      };
    }),
  }));
}
