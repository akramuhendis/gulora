/**
 * Ana səhifə üfüqi “Trendyol” tipli kateqoriya qısayolları.
 * `data/products.ts` vitrin slaq dəyərləri ilə uyğunlaşdırın; mobil `src/lib/homeQuickChips.ts` ilə eyni sıra.
 */
export type HomeQuickChipKind = "category" | "search";

export interface HomeQuickChipDef {
  id: string;
  kind: HomeQuickChipKind;
  /** kind === "category" — vitrin / API kateqoriya slaqı */
  slug?: string;
  /** kind === "search" — veb: /search?q=… */
  searchQuery?: string;
  icon: string;
  /** Yaşıl “Populyar” nişanı */
  popular?: boolean;
}

export const HOME_QUICK_CHIPS: HomeQuickChipDef[] = [
  {
    id: "mothers-day",
    kind: "search",
    searchQuery: "analar günü",
    icon: "🌷",
    popular: true,
  },
  { id: "flowers", kind: "category", slug: "cicekler", icon: "🌸" },
  { id: "gourmet", kind: "category", slug: "gurme-lezzet", icon: "🍓" },
  { id: "birthday", kind: "category", slug: "kek-pasta", icon: "🎂" },
  { id: "plush", kind: "category", slug: "pelus-oyuncak", icon: "🧸" },
];

/** Mağaza UI (Vite) — mobil üçün eyni mətnlər `mobile/src/i18n/az.ts`-də saxlanılır */
export const HOME_QUICK_CHIP_LABELS_AZ: Record<string, string> = {
  "mothers-day": "Analar günü",
  flowers: "Çiçək",
  gourmet: "Yeməli",
  birthday: "Ad günü",
  plush: "Oyuncaq",
};

export const HOME_QUICK_POPULAR_BADGE_AZ = "Populyar";

function normSlug(s: string): string {
  return s.trim().toLowerCase().replace(/_/g, "-");
}

export function findCategoryIdByStoreSlug(
  rows: { id: string; slug: string }[],
  storeSlug: string | undefined
): string | undefined {
  if (!storeSlug) return undefined;
  const want = normSlug(storeSlug);
  return rows.find((r) => normSlug(r.slug) === want)?.id;
}

export function getHomeQuickChipHref(chip: HomeQuickChipDef): string {
  if (chip.kind === "search" && chip.searchQuery?.trim()) {
    return `/search?q=${encodeURIComponent(chip.searchQuery.trim())}`;
  }
  if (chip.kind === "category" && chip.slug) {
    return `/kategori/${encodeURIComponent(chip.slug)}`;
  }
  return "/mehsullar";
}

export function homeQuickChipLabel(chip: HomeQuickChipDef): string {
  return HOME_QUICK_CHIP_LABELS_AZ[chip.id] ?? chip.id;
}
