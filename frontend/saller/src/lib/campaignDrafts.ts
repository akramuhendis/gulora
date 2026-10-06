/**
 * Satıcı kampaniya planları — API əlçatan deyəndə ehtiyat üçün localStorage (v2) və köhnə sessiya (v1) köçürməsi.
 * Əsas mənbə: `sellerCampaignApi` — `/api/v1/seller/campaigns`.
 */

export type CampaignScope =
  | { type: 'all' }
  | { type: 'category'; categoryId: string; categoryName: string }
  | { type: 'products'; products: { id: string; name: string }[] }

export type CampaignPlan = {
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
  updatedAt: string
}

const V2_KEY = 'gulora_seller_campaign_plans_v2'
const V1_SESSION_KEY = 'gulora_seller_campaign_drafts_v1'

function isRecord(x: unknown): x is Record<string, unknown> {
  return typeof x === 'object' && x !== null
}

function parseScope(raw: unknown): CampaignScope | null {
  if (!isRecord(raw) || typeof raw.type !== 'string') return null
  if (raw.type === 'all') return { type: 'all' }
  if (raw.type === 'category') {
    const categoryId = raw.categoryId
    const categoryName = raw.categoryName
    if (typeof categoryId === 'string' && typeof categoryName === 'string') {
      return { type: 'category', categoryId, categoryName }
    }
    return null
  }
  if (raw.type === 'products') {
    const products = raw.products
    if (!Array.isArray(products)) return null
    const list: { id: string; name: string }[] = []
    for (const p of products) {
      if (!isRecord(p)) continue
      if (typeof p.id === 'string' && typeof p.name === 'string') list.push({ id: p.id, name: p.name })
    }
    return { type: 'products', products: list }
  }
  return null
}

function parsePlan(raw: unknown): CampaignPlan | null {
  if (!isRecord(raw)) return null
  const id = raw.id
  const name = raw.name
  const kind = raw.kind
  const percent = raw.percent
  const startsAt = raw.startsAt
  const endsAt = raw.endsAt
  const createdAt = raw.createdAt
  const updatedAt = raw.updatedAt
  if (typeof id !== 'string' || typeof name !== 'string') return null
  if (kind !== 'discount' && kind !== 'shipping') return null
  if (typeof percent !== 'number' || !Number.isFinite(percent)) return null
  if (typeof startsAt !== 'string' || typeof endsAt !== 'string') return null
  if (typeof createdAt !== 'string' || typeof updatedAt !== 'string') return null
  const scope = parseScope(raw.scope)
  if (!scope) return null
  let minCartAmount: number | null = null
  if (raw.minCartAmount != null) {
    if (typeof raw.minCartAmount === 'number' && Number.isFinite(raw.minCartAmount) && raw.minCartAmount >= 0) {
      minCartAmount = raw.minCartAmount
    } else return null
  }
  let notes: string | null = null
  if (raw.notes != null && typeof raw.notes === 'string') notes = raw.notes.slice(0, 2000)

  return {
    id,
    name: name.slice(0, 200),
    kind,
    percent: Math.round(percent),
    startsAt,
    endsAt,
    minCartAmount,
    scope,
    notes,
    createdAt,
    updatedAt,
  }
}

export function loadPlans(): CampaignPlan[] {
  try {
    const raw = localStorage.getItem(V2_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw) as unknown
    if (!Array.isArray(parsed)) return []
    const out: CampaignPlan[] = []
    for (const row of parsed) {
      const p = parsePlan(row)
      if (p) out.push(p)
    }
    return out
  } catch {
    return []
  }
}

export function clearPlansLocalStorage(): void {
  try {
    localStorage.removeItem(V2_KEY)
  } catch {
    /* */
  }
}

export function savePlans(rows: CampaignPlan[]): void {
  try {
    localStorage.setItem(V2_KEY, JSON.stringify(rows))
  } catch {
    /* quota / private mode */
  }
}

function defaultEndIso(start: Date): string {
  const d = new Date(start)
  d.setDate(d.getDate() + 30)
  return d.toISOString()
}

export function migrateV1SessionToV2(): void {
  try {
    const raw = sessionStorage.getItem(V1_SESSION_KEY)
    if (!raw) return
    const parsed = JSON.parse(raw) as unknown
    if (!Array.isArray(parsed) || parsed.length === 0) return

    const existing = loadPlans()
    const migrated: CampaignPlan[] = []

    for (const row of parsed) {
      if (!isRecord(row)) continue
      const id = row.id
      const name = row.name
      const percent = row.percent
      const kind = row.kind
      const createdAt = row.createdAt
      if (typeof id !== 'string' || typeof name !== 'string') continue
      if (typeof percent !== 'number' || !Number.isFinite(percent)) continue
      if (kind !== 'discount' && kind !== 'shipping') continue
      if (typeof createdAt !== 'string') continue

      const start = new Date(createdAt)
      const startsAt = Number.isNaN(start.getTime()) ? new Date().toISOString() : start.toISOString()
      const endsAt = defaultEndIso(Number.isNaN(start.getTime()) ? new Date() : start)

      migrated.push({
        id: `mig-${id}`,
        name: name.slice(0, 200),
        kind,
        percent: Math.min(100, Math.max(1, Math.round(percent))),
        startsAt,
        endsAt,
        minCartAmount: null,
        scope: { type: 'all' },
        notes: 'Köçürülmüş taslak (sessiya v1)',
        createdAt,
        updatedAt: new Date().toISOString(),
      })
    }

    if (migrated.length > 0) {
      savePlans([...migrated, ...existing])
    }
    sessionStorage.removeItem(V1_SESSION_KEY)
  } catch {
    /* */
  }
}

export function scopeSummary(scope: CampaignScope): string {
  if (scope.type === 'all') return 'Bütün məhsullar'
  if (scope.type === 'category') return scope.categoryName
  if (scope.products.length === 1) {
    const n = scope.products[0]?.name?.trim()
    return n ? `1 məhsul: ${n}` : '1 məhsul'
  }
  return `${scope.products.length} məhsul`
}

export function parseImportedPlans(json: string): { ok: true; plans: CampaignPlan[] } | { ok: false; error: string } {
  try {
    const data = JSON.parse(json) as unknown
    if (!Array.isArray(data)) return { ok: false, error: 'JSON massiv olmalıdır.' }
    const out: CampaignPlan[] = []
    for (const row of data) {
      const p = parsePlan(row)
      if (p) out.push(p)
    }
    if (out.length === 0) return { ok: false, error: 'Uyğun kampaniya qeydi tapılmadı.' }
    return { ok: true, plans: out }
  } catch {
    return { ok: false, error: 'JSON oxuna bilmədi.' }
  }
}
