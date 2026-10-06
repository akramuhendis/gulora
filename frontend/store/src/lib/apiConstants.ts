/**
 * Backend ilə uyğun: Gulora.Contracts.Common.PagingConstraints, Products.ProductListSortBy.
 * Dəyərlər dəyişəndə backend contract ilə sinxron saxlayın.
 */

/** PagingConstraints.MaxPageSize */
export const API_MAX_PAGE_SIZE = 100;

/** PagingConstraints.DefaultPageSize */
export const API_DEFAULT_PAGE_SIZE = 20;

/** GetUserOrdersQueryHandler — effektiv üst sınır */
export const USER_ORDERS_MAX_PAGE_SIZE = 50;

/** GetProductReviewsQueryHandler */
export const PRODUCT_PUBLIC_REVIEWS_MAX_PAGE_SIZE = 50;

/** GetNotificationsQueryHandler */
export const NOTIFICATIONS_MAX_PAGE_SIZE = 50;

/** ProductListSortBy.Allowed (sıra backend ilə eyni) */
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

/** Boş və ya naməlum dəyərləri sorğudan çıxarmaq üçün */
/** ProductListSortBy.IsAllowed — OrdinalIgnoreCase */
export function normalizeProductSortBy(sortBy: string | undefined | null): string | undefined {
  const t = sortBy?.trim();
  if (!t) return undefined;
  const lower = t.toLowerCase();
  return PRODUCT_LIST_SORT_VALUES.find((v) => v === lower);
}
