import { createContext } from "react";
import type { Order } from "../models/order";

export interface OrderContextType {
  orders: Order[];
  ordersLoadError: string | null;
  refreshOrders: () => Promise<void>;
  addOrder: (order: Order) => void;
  getOrder: (id: string) => Order | undefined;
  requestReturn: (orderId: string, reason: string) => void;
  cancelOrder: (orderId: string) => void;
  markReviewed: (orderId: string) => void;
}

export const OrderContext = createContext<OrderContextType | undefined>(undefined);
