import type { Order } from "../models/order";

export function orderDisplayImage(order: Order): string {
  return order.items[0]?.image ?? "";
}

export function orderDisplayTitle(order: Order): string {
  const first = order.items[0];
  if (!first?.name) return "Sifariş";
  const extra = order.items.length - 1;
  if (extra <= 0) return first.name;
  return `${first.name} və ${extra} digər`;
}
