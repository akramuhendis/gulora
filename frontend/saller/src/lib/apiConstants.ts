/**
 * Backend ilə uyğun: Gulora.Contracts.Common.PagingConstraints, Products.ProductListSortBy.
 */

export const API_MAX_PAGE_SIZE = 100;
export const API_DEFAULT_PAGE_SIZE = 20;

/** GetSellerProductReviewsQueryHandler */
export const SELLER_PRODUCT_REVIEWS_MAX_PAGE_SIZE = 50;

export const PRODUCT_LIST_SORT_VALUES = [
  "newest",
  "price_asc",
  "price_desc",
  "rating",
  "name_asc",
  "name_desc",
] as const;

export type ProductListSortValue = (typeof PRODUCT_LIST_SORT_VALUES)[number];

export function clampApiPageSize(
  pageSize: number | undefined,
  fallback: number,
  max: number = API_MAX_PAGE_SIZE
): number {
  const base = pageSize ?? fallback;
  if (!Number.isFinite(base)) return Math.min(Math.max(fallback, 1), max);
  return Math.min(Math.max(Math.trunc(base), 1), max);
}

export function normalizeProductSortBy(sortBy: string | undefined | null): string | undefined {
  const t = sortBy?.trim();
  if (!t) return undefined;
  const lower = t.toLowerCase();
  return PRODUCT_LIST_SORT_VALUES.find((v) => v === lower);
}
