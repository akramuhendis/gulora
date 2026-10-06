const UPLOADS_PREFIX = "/uploads/";

function apiOrigin(): string {
  const explicit = import.meta.env.VITE_API_ORIGIN;
  if (typeof explicit === "string" && explicit.trim()) {
    return explicit.trim().replace(/\/$/, "");
  }
  const apiUrl = import.meta.env.VITE_API_URL ?? "/api/v1";
  if (apiUrl.startsWith("http://") || apiUrl.startsWith("https://")) {
    return new URL(apiUrl).origin;
  }
  return "http://localhost:5284";
}

function devRelativeUploads(): boolean {
  return import.meta.env.DEV && !import.meta.env.VITE_API_ORIGIN;
}

/** localhost:7253/5284 və ya nisbi yoldan /uploads/... çıxarır. */
export function extractUploadsPath(url: string): string | null {
  const t = url.trim();
  if (!t) return null;
  if (t.startsWith(UPLOADS_PREFIX)) return t.split("?")[0] || t;
  try {
    const u = new URL(t);
    if (u.pathname.startsWith(UPLOADS_PREFIX)) {
      return u.pathname + (u.search || "");
    }
  } catch {
    /* nisbi */
  }
  if (t.startsWith("uploads/")) return `/${t}`;
  return null;
}

/** Brauzerdə işləyən məhsul şəkli URL-i. */
export function resolveMediaUrl(url: string | null | undefined): string | null {
  if (url == null) return null;
  const trimmed = url.trim();
  if (!trimmed) return null;

  const uploadsPath = extractUploadsPath(trimmed);
  if (uploadsPath) {
    if (devRelativeUploads()) return uploadsPath;
    return `${apiOrigin()}${uploadsPath}`;
  }

  if (/^https?:\/\//i.test(trimmed)) return trimmed;
  if (trimmed.startsWith("//")) return `https:${trimmed}`;
  if (trimmed.startsWith("/")) {
    if (devRelativeUploads()) return trimmed;
    return `${apiOrigin()}${trimmed}`;
  }
  return trimmed;
}

export function resolveMediaUrlOrFallback(url: string | null | undefined, fallback: string): string {
  return resolveMediaUrl(url) ?? fallback;
}

export function isPlaceholderProductImage(url: string | null | undefined): boolean {
  if (!url?.trim()) return true;
  return url.includes("unsplash.com");
}
