import { useState, useEffect, useCallback, startTransition } from "react";
import type { ReactNode } from "react";
import { useAuth } from "../hooks/useAuth";
import { ApiError, orderApi } from "../services/api";
import { enrichOrdersWithProductImages } from "../lib/enrichOrderImages";
import type { Order } from "../models/order";
import { OrderContext } from "./order-context";

function storageKey(userId?: string) {
  return userId ? `gulora_orders_${userId}` : "gulora_orders_guest";
}

function loadOrders(userId?: string): Order[] {
  try {
    const raw = localStorage.getItem(storageKey(userId));
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveOrders(orders: Order[], userId?: string) {
  localStorage.setItem(storageKey(userId), JSON.stringify(orders));
}

export function OrderProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [ordersLoadError, setOrdersLoadError] = useState<string | null>(null);

  const fetchOrdersForUser = useCallback(
    async (userId: string, signal?: { cancelled: boolean }) => {
      setOrdersLoadError(null);
      try {
        const rows = await orderApi.listDetailed(1, 50);
        if (signal?.cancelled) return;
        setOrders(rows);
        saveOrders(rows, userId);
      } catch (e) {
        if (signal?.cancelled) return;
        const stale = loadOrders(userId);
        const enriched = await enrichOrdersWithProductImages(stale);
        if (signal?.cancelled) return;
        setOrders(enriched);
        setOrdersLoadError(
          e instanceof ApiError
            ? e.message
            : "Sifarişlər serverdən yüklənə bilmədi. Bağlantını və ya girişi yoxlayın."
        );
      }
    },
    []
  );

  const refreshOrders = useCallback(async () => {
    if (!user?.id) return;
    await fetchOrdersForUser(user.id);
  }, [user, fetchOrdersForUser]);

  useEffect(() => {
    if (!user?.id) {
      startTransition(() => {
        setOrders(loadOrders(undefined));
        setOrdersLoadError(null);
      });
      return;
    }
    const signal = { cancelled: false };
    queueMicrotask(() => {
      void fetchOrdersForUser(user.id, signal);
    });
    return () => {
      signal.cancelled = true;
    };
  }, [user, fetchOrdersForUser]);

  useEffect(() => {
    if (!user) saveOrders(orders, undefined);
  }, [orders, user]);

  const addOrder = useCallback(
    (order: Order) => {
      setOrders((prev) => {
        const next = [order, ...prev];
        if (user) saveOrders(next, user.id);
        return next;
      });
    },
    [user]
  );

  const getOrder = useCallback(
    (id: string) => {
      return orders.find((o) => o.id === id);
    },
    [orders]
  );

  const requestReturn = useCallback(
    (orderId: string, reason: string) => {
      if (user) {
        void orderApi.requestReturn(orderId, reason).then(() =>
          orderApi
            .listDetailed(1, 50)
            .then((rows) => {
              setOrders(rows);
              saveOrders(rows, user.id);
            })
            .catch(() => {})
        );
        return;
      }
      setOrders((prev) =>
        prev.map((o) =>
          o.id === orderId
            ? {
                ...o,
                status: "return_requested",
                returnReason: reason,
                returnRequestedAt: new Date().toISOString(),
                tracking: [
                  ...o.tracking,
                  {
                    status: "return_requested" as const,
                    timestamp: new Date().toISOString(),
                    note: `İadə sorğusu: ${reason}`,
                  },
                ],
              }
            : o
        )
      );
    },
    [user]
  );

  const cancelOrder = useCallback(
    (orderId: string) => {
      if (user) {
        void orderApi
          .cancel(orderId, "Müştəri tərəfindən ləğv edildi.")
          .then(() =>
            orderApi
              .listDetailed(1, 50)
              .then((rows) => {
                setOrders(rows);
                saveOrders(rows, user.id);
              })
              .catch(() => {})
          );
        return;
      }
      setOrders((prev) =>
        prev.map((o) =>
          o.id === orderId && o.status === "pending"
            ? {
                ...o,
                status: "cancelled",
                tracking: [
                  ...o.tracking,
                  {
                    status: "cancelled" as const,
                    timestamp: new Date().toISOString(),
                    note: "Müştəri tərəfindən ləğv edildi.",
                  },
                ],
              }
            : o
        )
      );
    },
    [user]
  );

  const markReviewed = useCallback((orderId: string) => {
    setOrders((prev) => prev.map((o) => (o.id === orderId ? { ...o, reviewLeft: true } : o)));
  }, []);

  return (
    <OrderContext.Provider
      value={{
        orders,
        ordersLoadError,
        refreshOrders,
        addOrder,
        getOrder,
        requestReturn,
        cancelOrder,
        markReviewed,
      }}
    >
      {children}
    </OrderContext.Provider>
  );
}
