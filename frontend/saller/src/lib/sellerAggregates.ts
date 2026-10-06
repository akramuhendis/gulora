import type { OrderSummaryResponse, ProductSummaryResponse } from '../services/api'

export type OrderTrendDay = { dateKey: string; label: string; count: number; revenue: number }

const REALIZED_STATUSES = new Set(['Delivered', 'Shipped'])
const EXCLUDED_FROM_GROSS = new Set(['Cancelled', 'Returned', 'ReturnRequested'])

export function aggregateOrdersByStatus(orders: OrderSummaryResponse[]): Map<string, number> {
  const m = new Map<string, number>()
  for (const o of orders) {
    m.set(o.status, (m.get(o.status) ?? 0) + 1)
  }
  return m
}

/** Ləğv/iadə olmayan sifarişlərin ümumi məbləği (nümunə dəstlə). */
export function sumGrossMerchandiseValue(orders: OrderSummaryResponse[]): number {
  let s = 0
  for (const o of orders) {
    if (EXCLUDED_FROM_GROSS.has(o.status)) continue
    s += o.totalAmount
  }
  return s
}

/** Çatdırılması başlamış / tamamlanmış satış məbləği. */
export function sumRealizedRevenue(orders: OrderSummaryResponse[]): number {
  let s = 0
  for (const o of orders) {
    if (REALIZED_STATUSES.has(o.status)) s += o.totalAmount
  }
  return s
}

export function countLowStockProducts(
  products: ProductSummaryResponse[],
  threshold: number
): number {
  return products.filter((p) => p.stockQuantity > 0 && p.stockQuantity <= threshold).length
}

export function countOutOfStock(products: ProductSummaryResponse[]): number {
  return products.filter((p) => p.stockQuantity <= 0).length
}

function startOfDay(d: Date): Date {
  const x = new Date(d)
  x.setHours(0, 0, 0, 0)
  return x
}

function addDays(d: Date, n: number): Date {
  const x = new Date(d)
  x.setDate(x.getDate() + n)
  return x
}

function formatDateKey(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

/** Son `dayCount` gün üçün gündəlik sifariş sayı və məbləğ (yerli tarix). */
export function bucketOrdersByDay(
  orders: OrderSummaryResponse[],
  dayCount: number
): OrderTrendDay[] {
  const today = startOfDay(new Date())
  const keys: string[] = []
  for (let i = dayCount - 1; i >= 0; i--) {
    keys.push(formatDateKey(addDays(today, -i)))
  }
  const countMap = new Map<string, { count: number; revenue: number }>()
  for (const k of keys) countMap.set(k, { count: 0, revenue: 0 })

  for (const o of orders) {
    const d = startOfDay(new Date(o.createdAt))
    const k = formatDateKey(d)
    const bucket = countMap.get(k)
    if (!bucket) continue
    bucket.count += 1
    if (!EXCLUDED_FROM_GROSS.has(o.status)) bucket.revenue += o.totalAmount
  }

  return keys.map((dateKey) => {
    const b = countMap.get(dateKey)!
    const [y, m, day] = dateKey.split('-').map(Number)
    const label = new Date(y, m - 1, day).toLocaleDateString('az-AZ', {
      month: 'short',
      day: 'numeric',
    })
    return { dateKey, label, count: b.count, revenue: b.revenue }
  })
}
