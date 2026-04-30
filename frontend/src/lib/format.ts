/** Azərbaycan manatı (AZN) formatı */
export function formatMoney(value: number): string {
  return new Intl.NumberFormat("az-AZ", {
    style: "currency",
    currency: "AZN",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

export function formatReviewCount(count: number): string {
  if (count >= 99999) return "99.999+";
  return count.toLocaleString("az-AZ");
}
