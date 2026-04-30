export type OrderStatus =
  | "pending"
  | "preparing"
  | "shipped"
  | "delivered"
  | "cancelled";

export interface MockOrderItem {
  name: string;
  qty: number;
  price: number;
  productSlug?: string;
}

export interface MockOrder {
  id: string;
  date: string;
  status: OrderStatus;
  total: number;
  items: MockOrderItem[];
}

export interface MockAddress {
  id: string;
  title: string;
  recipient: string;
  phone: string;
  city: string;
  district: string;
  line: string;
  isDefault: boolean;
}

export const mockUser = {
  firstName: "Leyla",
  lastName: "Məmmədova",
  email: "leyla.m@email.az",
  phone: "+994 50 123 45 67",
  memberSince: "2024-09-12",
};

export const mockOrders: MockOrder[] = [
  {
    id: "GL-2026-0842",
    date: "2026-04-22",
    status: "delivered",
    total: 189.98,
    items: [
      { name: "11 qırmızı güldən buket", qty: 1, price: 89.99, productSlug: "11li-kirmizi-gul-buketi" },
      { name: "Lotus Biscoff® qourmet qutusu", qty: 1, price: 74.99, productSlug: "lotus-biscoff-gurme-kutusu" },
    ],
  },
  {
    id: "GL-2026-0791",
    date: "2026-04-18",
    status: "shipped",
    total: 109.99,
    items: [{ name: "2 budaqlı ağ orxideya", qty: 1, price: 109.99, productSlug: "2-dal-beyaz-orkide" }],
  },
  {
    id: "GL-2026-0655",
    date: "2026-03-30",
    status: "delivered",
    total: 67.99,
    items: [{ name: "7 qırmızı güldən buket", qty: 1, price: 67.99, productSlug: "7li-kirmizi-gul-buketi" }],
  },
  {
    id: "GL-2026-0512",
    date: "2026-03-05",
    status: "cancelled",
    total: 59.99,
    items: [{ name: "5 qırmızı güldən mini buket", qty: 1, price: 59.99, productSlug: "5li-kirmizi-gul-buketi" }],
  },
];

export const mockAddresses: MockAddress[] = [
  {
    id: "1",
    title: "Ev",
    recipient: "Leyla Məmmədova",
    phone: "+994 50 123 45 67",
    city: "Bakı",
    district: "Nizami",
    line: "Nizami küç. 203, m. 4, mənzil 12",
    isDefault: true,
  },
  {
    id: "2",
    title: "Ofis",
    recipient: "Leyla Məmmədova",
    phone: "+994 12 555 00 11",
    city: "Bakı",
    district: "Nəsimi",
    line: "Rəşid Behbudov küç. 45, 3-cü mərtəbə",
    isDefault: false,
  },
];

export function orderStatusLabel(status: OrderStatus): string {
  const map: Record<OrderStatus, string> = {
    pending: "Gözləmədə",
    preparing: "Hazırlanır",
    shipped: "Yoldadır",
    delivered: "Çatdırılıb",
    cancelled: "Ləğv edilib",
  };
  return map[status];
}

export function orderStatusTone(status: OrderStatus): string {
  const map: Record<OrderStatus, string> = {
    pending: "bg-amber-50 text-amber-800 border-amber-200",
    preparing: "bg-blue-50 text-blue-800 border-blue-200",
    shipped: "bg-violet-50 text-violet-800 border-violet-200",
    delivered: "bg-green-50 text-green-800 border-green-200",
    cancelled: "bg-gray-100 text-gray-600 border-gray-200",
  };
  return map[status];
}
