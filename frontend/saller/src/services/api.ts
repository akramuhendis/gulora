/**
 * Satıcı paneli — bütün REST çağrıları bu moduldan keçir.
 * Backend: /api/v1 (Vite proxy → Gulora.API HTTP portu, launchSettings ilə uyğun — indi 5284).
 */

import type { CampaignScope } from '../lib/campaignDrafts'
import {
  API_DEFAULT_PAGE_SIZE,
  clampApiPageSize,
  normalizeProductSortBy,
  SELLER_PRODUCT_REVIEWS_MAX_PAGE_SIZE,
} from '../lib/apiConstants'

const BASE_URL = import.meta.env.VITE_API_URL ?? '/api/v1'

const ACCESS_KEY = 'gulora_seller_access_token'
const REFRESH_KEY = 'gulora_seller_refresh_token'

export class ApiError extends Error {
  status: number
  body?: unknown

  constructor(status: number, message: string, body?: unknown) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.body = body
  }
}

export type PagedResult<T> = {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages?: number
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}

export type AuthUserResponse = {
  id: string
  fullName: string
  email: string
  role: string
}

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  refreshTokenExpiry: string
  user: AuthUserResponse
}

export type CurrentUserResponse = {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  phoneNumber: string | null
  role: string
  isActive: boolean
  emailVerified: boolean
  phoneVerified: boolean
  lastLoginAt: string | null
  createdAt: string
}

/** Kateqoriya ağacı — GET /api/v1/categories */
export type CategoryResponse = {
  id: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  parentId: string | null
  displayOrder: number
  isActive: boolean
  children: CategoryResponse[]
}

/** POST /api/v1/products gövdəsi (camelCase). */
export type CreateProductBody = {
  name: string
  slug: string
  price: number
  stockQuantity: number
  categoryId: string
  description?: string | null
  shortDescription?: string | null
  sku?: string | null
  brand?: string | null
  discountedPrice?: number | null
  isFeatured?: boolean
}

export type UpdateProductBody = {
  name: string
  slug: string
  description?: string | null
  shortDescription?: string | null
  brand?: string | null
  categoryId: string
  isFeatured?: boolean
}

export type ProductDetailResponse = {
  id: string
  name: string
  slug: string
  description: string | null
  shortDescription: string | null
  price: number
  discountedPrice: number | null
  effectivePrice: number
  stockQuantity: number
  isAvailableForPurchase: boolean
  sku: string | null
  brand: string | null
  categoryId: string
  categoryName: string
  sellerId: string | null
  isActive: boolean
  isFeatured: boolean
  averageRating: number
  reviewCount: number
  primaryImageUrl: string | null
  createdAt: string
  updatedAt: string | null
}

export type ProductSummaryResponse = {
  id: string
  name: string
  slug: string
  shortDescription: string | null
  price: number
  discountedPrice: number | null
  effectivePrice: number
  stockQuantity: number
  isActive: boolean
  isAvailableForPurchase: boolean
  brand: string | null
  categoryId: string
  categoryName: string
  isFeatured: boolean
  averageRating: number
  reviewCount: number
  primaryImageUrl: string | null
  createdAt: string
}

export type OrderSummaryResponse = {
  id: string
  orderNumber: string
  status: string
  statusLabel: string
  deliveryType: string
  paymentMethod: string
  paymentStatus: string
  totalAmount: number
  itemCount: number
  primaryProductName: string | null
  primaryImageUrl: string | null
  canBeCancelled: boolean
  canBeReturned: boolean
  createdAt: string
  /** Satıcıya aid məhsul sətirlərinin cəmi (çox satıcılı səbət üçün). */
  sellerAttributedTotal?: number | null
}

export type SellerDashboardStatusCount = {
  status: string
  statusLabel: string
  count: number
}

export type SellerDashboardDayBucket = {
  date: string
  orderCount: number
  sellerAttributedTotal: number
}

export type SellerDashboardResponse = {
  totalProductCount: number
  lowStockCount: number
  outOfStockCount: number
  lowStockThreshold: number
  totalOrderCount: number
  pendingFulfillmentCount: number
  statusBreakdown: SellerDashboardStatusCount[]
  gmvSellerAttributed: number
  realizedSellerAttributed: number
  ordersLastDays: SellerDashboardDayBucket[]
  recentOrders: OrderSummaryResponse[]
}

export type OrderItemResponse = {
  id: string
  productId: string
  productName: string
  productSku: string | null
  unitPrice: number
  quantity: number
  lineTotal: number
}

export type AddressSnapshotResponse = {
  recipientName: string
  phoneNumber: string
  city: string
  district: string
  fullLine: string
  postalCode: string | null
  countryCode: string | null
}

export type OrderTrackingResponse = {
  id: string
  status: string
  statusLabel: string
  note: string | null
  cargoCompany: string | null
  trackingNumber: string | null
  changedByUserId: string | null
  createdAt: string
}

export type OrderResponse = {
  id: string
  orderNumber: string
  status: string
  statusLabel: string
  deliveryType: string
  paymentMethod: string
  paymentStatus: string
  subTotal: number
  deliveryFee: number
  discountAmount: number
  totalAmount: number
  couponCode: string | null
  trackingNumber: string | null
  cargoCompany: string | null
  cancellationReason: string | null
  deliveredAt: string | null
  canBeCancelled: boolean
  canBeReturned: boolean
  createdAt: string
  updatedAt: string | null
  address: AddressSnapshotResponse
  items: OrderItemResponse[]
  trackings: OrderTrackingResponse[]
  /** Müştərinin hədiyyə kartı mətni və meta — backend OrderResponse ilə eyni kablo. */
  giftCardMessage?: string | null
  giftCardSenderName?: string | null
  giftCardOccasion?: string | null
}

let refreshInFlight: Promise<boolean> | null = null

function parseJsonOrRawText<T>(text: string): T {
  if (!text) return undefined as T
  try {
    return JSON.parse(text) as T
  } catch {
    return text as T
  }
}

function clearSession(): void {
  localStorage.removeItem(ACCESS_KEY)
  localStorage.removeItem(REFRESH_KEY)
}

/** Yanlış şifrə kimi hallarda refresh döngüsünə girməmək üçün. */
function shouldAttemptTokenRefreshOn401(path: string): boolean {
  if (path.startsWith('/auth/login')) return false
  if (path.startsWith('/auth/register')) return false
  if (path.startsWith('/auth/refresh')) return false
  if (path.startsWith('/auth/forgot-password')) return false
  if (path.startsWith('/auth/reset-password')) return false
  return true
}

async function tryRefreshToken(): Promise<boolean> {
  if (refreshInFlight) return refreshInFlight
  const refresh = localStorage.getItem(REFRESH_KEY)
  if (!refresh) return false

  refreshInFlight = (async () => {
    try {
      const res = await fetch(`${BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: refresh }),
      })
      if (!res.ok) return false
      const data = (await res.json()) as AuthResponse
      localStorage.setItem(ACCESS_KEY, data.accessToken)
      localStorage.setItem(REFRESH_KEY, data.refreshToken)
      return true
    } catch {
      return false
    } finally {
      refreshInFlight = null
    }
  })()

  return refreshInFlight
}

export async function request<T>(
  path: string,
  options: RequestInit = {},
  skipAuth = false,
  allowTokenRefresh = true
): Promise<T> {
  const token = localStorage.getItem(ACCESS_KEY)
  const headers: HeadersInit = {
    ...(options.body ? { 'Content-Type': 'application/json' } : {}),
    ...(!skipAuth && token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  }

  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers })

  if (res.status === 401 && !skipAuth && allowTokenRefresh && shouldAttemptTokenRefreshOn401(path)) {
    const refreshed = await tryRefreshToken()
    if (!refreshed) {
      clearSession()
      throw new ApiError(401, 'Sessiya bitib.', undefined)
    }
    return request<T>(path, options, skipAuth, false)
  }

  if (!res.ok) {
    const text = await res.text()
    let message = res.statusText || 'Xəta baş verdi.'
    if (res.status === 502 || res.status === 504) {
      message =
        'API serverinə çata bilinmir. Gulora.API işə salınıb və vite.config proxy portu launchSettings.json HTTP portu ilə eynidir (5284).'
    }
    let body: unknown
    if (text) {
      try {
        body = JSON.parse(text) as object
        const j = body as {
          detail?: string
          title?: string
          errors?: Record<string, string[]>
        }
        if (j.detail) message = j.detail
        else if (j.title) message = j.title
        else if (j.errors) {
          const flat = Object.values(j.errors).flat()[0]
          if (flat) message = flat
        }
      } catch {
        message = text.length > 280 ? `${text.slice(0, 280)}…` : text
        body = undefined
      }
    }
    throw new ApiError(res.status, message, body)
  }

  if (res.status === 204) return undefined as T

  const text = await res.text()
  return parseJsonOrRawText<T>(text)
}

export type UploadProductCoverResponse = {
  url: string
}

export type AddProductImageBody = {
  url: string
  altText?: string | null
  displayOrder?: number
}

/** multipart/form-data — Content-Type təyin olunmur (boundary üçün). */
async function requestMultipartJson<T>(
  path: string,
  formData: FormData,
  skipAuth = false,
  allowTokenRefresh = true
): Promise<T> {
  const token = localStorage.getItem(ACCESS_KEY)
  const headers: HeadersInit = {
    ...(!skipAuth && token ? { Authorization: `Bearer ${token}` } : {}),
  }

  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers,
    body: formData,
  })

  if (res.status === 401 && !skipAuth && allowTokenRefresh && shouldAttemptTokenRefreshOn401(path)) {
    const refreshed = await tryRefreshToken()
    if (!refreshed) {
      clearSession()
      throw new ApiError(401, 'Sessiya bitib.', undefined)
    }
    return requestMultipartJson<T>(path, formData, skipAuth, false)
  }

  if (!res.ok) {
    const text = await res.text()
    let message = res.statusText || 'Xəta baş verdi.'
    if (res.status === 502 || res.status === 504) {
      message =
        'API serverinə çata bilinmir. Gulora.API işə salınıb və vite.config proxy portu launchSettings.json HTTP portu ilə eynidir (5284).'
    }
    let body: unknown
    if (text) {
      try {
        body = JSON.parse(text) as object
        const j = body as {
          detail?: string
          title?: string
          errors?: Record<string, string[]>
        }
        if (j.detail) message = j.detail
        else if (j.title) message = j.title
        else if (j.errors) {
          const flat = Object.values(j.errors).flat()[0]
          if (flat) message = flat
        }
      } catch {
        message = text.length > 280 ? `${text.slice(0, 280)}…` : text
        body = undefined
      }
    }
    throw new ApiError(res.status, message, body)
  }

  const text = await res.text()
  return parseJsonOrRawText<T>(text)
}

export const authApi = {
  login: (email: string, password: string) =>
    request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  me: () => request<CurrentUserResponse>('/auth/me'),

  logout: async () => {
    const refresh = localStorage.getItem(REFRESH_KEY)
    if (refresh) {
      try {
        await request<void>('/auth/logout', {
          method: 'POST',
          body: JSON.stringify({ refreshToken: refresh }),
        })
      } catch {
        /* şəbəkə xətası olsa belə yerli sessiyanı təmizləyirik */
      }
    }
    clearSession()
  },

  persistSession: (data: AuthResponse) => {
    localStorage.setItem(ACCESS_KEY, data.accessToken)
    localStorage.setItem(REFRESH_KEY, data.refreshToken)
  },
}

function toQuery(params: Record<string, string | number | boolean | undefined | null>): string {
  const sp = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null || v === '') continue
    sp.set(k, String(v))
  }
  const s = sp.toString()
  return s ? `?${s}` : ''
}

/** Məhsul rəyi (ictimai və ya satıcı paneli cavabı). */
export type ProductReviewResponse = {
  id: string
  productId: string
  userId: string
  orderId: string | null
  reviewerName: string | null
  rating: number
  title: string | null
  body: string | null
  status: string
  rejectionReason: string | null
  createdAt: string
  updatedAt: string | null
}

/** İctimai kataloq — satıcı formunda kateqoriya seçimi. */
export const catalogApi = {
  listCategories: (onlyActive = true) =>
    request<CategoryResponse[]>(`/categories${toQuery({ onlyActive })}`),
}

export const sellerPortalApi = {
  listProducts: (opts?: {
    page?: number
    pageSize?: number
    query?: string
    isActive?: boolean
    inStock?: boolean
    sortBy?: string
  }) =>
    request<PagedResult<ProductSummaryResponse>>(
      `/seller/products${toQuery({
        page: opts?.page ?? 1,
        pageSize: clampApiPageSize(opts?.pageSize, API_DEFAULT_PAGE_SIZE),
        query: opts?.query?.trim() || undefined,
        isActive: opts?.isActive,
        inStock: opts?.inStock,
        sortBy: normalizeProductSortBy(opts?.sortBy),
      })}`
    ),

  /** Öz məhsulunuzun bütün rəyləri (Pending / Approved / Rejected) — JWT satıcı tələb olunur. */
  listMyProductReviews: (productId: string, page = 1, pageSize = 50) =>
    request<PagedResult<ProductReviewResponse>>(
      `/seller/products/${encodeURIComponent(productId)}/reviews${toQuery({
        page,
        pageSize: clampApiPageSize(pageSize, 50, SELLER_PRODUCT_REVIEWS_MAX_PAGE_SIZE),
      })}`
    ),

  createProduct: (body: CreateProductBody) =>
    request<string>('/products', {
      method: 'POST',
      body: JSON.stringify(body),
    }),

  getProductBySlug: (slug: string) =>
    request<ProductDetailResponse>(`/products/${encodeURIComponent(slug)}`),

  updateProduct: (id: string, body: UpdateProductBody) =>
    request<void>(`/products/${encodeURIComponent(id)}`, {
      method: 'PUT',
      body: JSON.stringify({
        name: body.name,
        slug: body.slug,
        description: body.description ?? null,
        shortDescription: body.shortDescription ?? null,
        brand: body.brand ?? null,
        categoryId: body.categoryId,
        isFeatured: body.isFeatured ?? false,
      }),
    }),

  updateProductPrice: (id: string, price: number, discountedPrice?: number | null) =>
    request<void>(`/products/${encodeURIComponent(id)}/price`, {
      method: 'PATCH',
      body: JSON.stringify({
        price,
        discountedPrice: discountedPrice ?? null,
      }),
    }),

  updateProductStock: (id: string, stockQuantity: number) =>
    request<void>(`/products/${encodeURIComponent(id)}/stock`, {
      method: 'PATCH',
      body: JSON.stringify({ stockQuantity }),
    }),

  deleteProduct: (id: string) =>
    request<void>(`/products/${encodeURIComponent(id)}`, {
      method: 'DELETE',
    }),

  activateProduct: (id: string) =>
    request<void>(`/products/${encodeURIComponent(id)}/activate`, {
      method: 'PATCH',
    }),

  uploadProductCover: (file: File) => {
    const fd = new FormData()
    fd.append('file', file)
    return requestMultipartJson<UploadProductCoverResponse>(
      '/seller/uploads/product-cover',
      fd
    )
  },

  addProductImage: (productId: string, body: AddProductImageBody) =>
    request<string>(`/products/${encodeURIComponent(productId)}/images`, {
      method: 'POST',
      body: JSON.stringify({
        url: body.url,
        altText: body.altText ?? null,
        displayOrder: body.displayOrder ?? 0,
      }),
    }),

  listOrders: (opts: {
    page?: number
    pageSize?: number
    status?: string
    orderNumber?: string
    dateFrom?: string
    dateTo?: string
  }) =>
    request<PagedResult<OrderSummaryResponse>>(
      `/seller/orders${toQuery({
        page: opts.page ?? 1,
        pageSize: clampApiPageSize(opts.pageSize, 20),
        status: opts.status,
        orderNumber: opts.orderNumber || undefined,
        dateFrom: opts.dateFrom,
        dateTo: opts.dateTo,
      })}`
    ),

  getDashboard: (opts?: { recentOrders?: number; trendDays?: number; lowStockThreshold?: number }) =>
    request<SellerDashboardResponse>(
      `/seller/dashboard${toQuery({
        recentOrders: opts?.recentOrders,
        trendDays: opts?.trendDays,
        lowStockThreshold: opts?.lowStockThreshold,
      })}`
    ),

  getOrder: (id: string) => request<OrderResponse>(`/seller/orders/${id}`),

  updateOrderStatus: (id: string, newStatus: string, note?: string) =>
    request<void>(`/seller/orders/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ newStatus, note: note ?? null }),
    }),

  shipOrder: (id: string, cargoCompany: string, trackingNumber: string) =>
    request<void>(`/seller/orders/${id}/ship`, {
      method: 'POST',
      body: JSON.stringify({ cargoCompany, trackingNumber }),
    }),
}

export type SellerShopMyResponse = {
  sellerUserId: string
  storeName: string
  storeSlug: string
  description: string | null
  logoUrl: string | null
  bannerUrl: string | null
  createdAt: string
  updatedAt: string | null
}

export type UpsertSellerShopBody = {
  storeName: string
  storeSlug: string
  description?: string | null
  logoUrl?: string | null
  bannerUrl?: string | null
}

const SELLER_SHOPS_ME_PRIMARY = '/seller-shops/me'
const SELLER_SHOPS_ME_LEGACY = '/SellerShops/me'

export const sellerShopApi = {
  async uploadShopLogo(file: File): Promise<UploadProductCoverResponse> {
    const toUploadResponse = (payload: UploadProductCoverResponse | string): UploadProductCoverResponse =>
      typeof payload === 'string' ? { url: payload } : payload

    const fd = new FormData()
    fd.append('file', file)
    try {
      const res = await requestMultipartJson<UploadProductCoverResponse | string>(
        '/seller/uploads/shop-logo',
        fd
      )
      return toUploadResponse(res)
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        try {
          const res = await requestMultipartJson<UploadProductCoverResponse | string>(
            '/seller/uploads/product-cover',
            fd
          )
          return toUploadResponse(res)
        } catch (legacyE) {
          if (legacyE instanceof ApiError && legacyE.status === 404) {
            try {
              const res = await requestMultipartJson<UploadProductCoverResponse | string>(
                '/SellerUploads/shop-logo',
                fd
              )
              return toUploadResponse(res)
            } catch (legacyControllerE) {
              if (legacyControllerE instanceof ApiError && legacyControllerE.status === 404) {
                const res = await requestMultipartJson<UploadProductCoverResponse | string>(
                  '/SellerUploads/product-cover',
                  fd
                )
                return toUploadResponse(res)
              }
              throw legacyControllerE
            }
          }
          throw legacyE
        }
      }
      throw e
    }
  },

  async getMine(): Promise<SellerShopMyResponse | null> {
    try {
      return await request<SellerShopMyResponse>(SELLER_SHOPS_ME_PRIMARY)
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        try {
          return await request<SellerShopMyResponse>(SELLER_SHOPS_ME_LEGACY)
        } catch (legacyErr) {
          if (legacyErr instanceof ApiError && legacyErr.status === 404) return null
          throw legacyErr
        }
      }
      throw e
    }
  },

  async upsert(body: UpsertSellerShopBody): Promise<SellerShopMyResponse> {
    try {
      return await request<SellerShopMyResponse>(SELLER_SHOPS_ME_PRIMARY, {
        method: 'PUT',
        body: JSON.stringify(body),
      })
    } catch (e) {
      if (e instanceof ApiError && e.status === 404) {
        return request<SellerShopMyResponse>(SELLER_SHOPS_ME_LEGACY, {
          method: 'PUT',
          body: JSON.stringify(body),
        })
      }
      throw e
    }
  },
}

export type SellerCampaignPlanWire = {
  id: string
  name: string
  kind: 'discount' | 'shipping'
  percent: number
  startsAt: string
  endsAt: string
  minCartAmount: number | null
  scope: CampaignScope
  notes: string | null
  createdAt: string
  updatedAt: string | null
}

export type UpsertSellerCampaignPlanBody = {
  name: string
  kind: 'discount' | 'shipping'
  percent: number
  startsAt: string
  endsAt: string
  minCartAmount: number | null
  scope: CampaignScope
  notes: string | null
}

export const sellerCampaignApi = {
  list: () => request<SellerCampaignPlanWire[]>('/seller/campaigns'),

  create: (body: UpsertSellerCampaignPlanBody) =>
    request<SellerCampaignPlanWire>('/seller/campaigns', {
      method: 'POST',
      body: JSON.stringify(body),
    }),

  update: (id: string, body: UpsertSellerCampaignPlanBody) =>
    request<SellerCampaignPlanWire>(`/seller/campaigns/${encodeURIComponent(id)}`, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),

  remove: (id: string) =>
    request<void>(`/seller/campaigns/${encodeURIComponent(id)}`, {
      method: 'DELETE',
    }),
}

export const ORDER_STATUS_VALUES = [
  'Pending',
  'Confirmed',
  'Processing',
  'Shipped',
  'Delivered',
  'Cancelled',
  'ReturnRequested',
  'Returned',
] as const
