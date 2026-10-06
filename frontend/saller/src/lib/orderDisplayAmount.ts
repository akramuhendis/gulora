import type { OrderSummaryResponse } from '../services/api'

/** Sifariş cədvəllərində satıcıya düşən məbləği göstərmək üçün. */
export function orderDisplayTotalForSeller(o: OrderSummaryResponse): number {
  if (o.sellerAttributedTotal != null && Number.isFinite(o.sellerAttributedTotal)) {
    return o.sellerAttributedTotal
  }
  return o.totalAmount
}
