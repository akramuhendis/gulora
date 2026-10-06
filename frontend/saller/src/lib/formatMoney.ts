/** Azərbaycan məkanı — manat göstərimi (satıcı paneli). */
export function formatMoney(n: number): string {
  return (
    new Intl.NumberFormat('az-AZ', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
      n
    ) + ' ₼'
  )
}
