import { useContext } from "react";
import { OrderContext } from "../context/order-context";

export function useOrders() {
  const ctx = useContext(OrderContext);
  if (!ctx) throw new Error("useOrders must be used within OrderProvider");
  return ctx;
}
