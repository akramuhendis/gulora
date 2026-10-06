import { useEffect, useState, startTransition } from "react";
import { Link, useParams } from "react-router-dom";
import { Loader2, Package, Percent, Store, Truck } from "lucide-react";
import ProductCard from "../components/ProductCard";
import { usePageMeta } from "../hooks/usePageMeta";
import {
  productApi,
  sellerShopApi,
  ApiError,
  type SellerShopPublicWire,
  type SellerCampaignPlanPublicWire,
} from "../services/api";
import type { Product } from "../data/products";

function campaignKindLabel(kind: string): string {
  const k = kind.trim().toLowerCase();
  if (k === "shipping") return "Çatdırılma endirimi";
  return "Faiz endirimi";
}

function scopeSummary(scope: unknown): string {
  if (!scope || typeof scope !== "object") return "";
  const t = (scope as { type?: string }).type?.toLowerCase();
  if (t === "all") return "Bütün məhsullar";
  if (t === "category") return "Seçilmiş kateqoriyalar";
  if (t === "products") return "Seçilmiş məhsullar";
  return "";
}

function formatCampaignDate(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString("az-AZ", { day: "numeric", month: "short", year: "numeric" });
}

export default function SellerStorePage() {
  const { slug } = useParams<{ slug: string }>();
  const [shop, setShop] = useState<SellerShopPublicWire | null>(null);
  const [products, setProducts] = useState<Product[]>([]);
  const [campaigns, setCampaigns] = useState<SellerCampaignPlanPublicWire[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  usePageMeta(shop?.storeName ?? "Mağaza", shop?.description ?? undefined);

  useEffect(() => {
    if (!slug) {
      startTransition(() => {
        setLoading(false);
      });
      return;
    }
    let cancelled = false;
    startTransition(() => {
      setLoading(true);
      setError(null);
    });

    (async () => {
      try {
        const s = await sellerShopApi.getBySlug(slug);
        if (cancelled) return;
        setShop(s);
        const [list, campList] = await Promise.all([
          productApi.list({
            sellerId: s.sellerUserId,
            page: 1,
            pageSize: 48,
            sortBy: "newest",
          }),
          sellerShopApi.listActiveCampaignsBySlug(slug).catch(() => [] as SellerCampaignPlanPublicWire[]),
        ]);
        if (cancelled) return;
        setProducts(list);
        setCampaigns(campList);
      } catch (e) {
        if (cancelled) return;
        setShop(null);
        setProducts([]);
        setCampaigns([]);
        setError(
          e instanceof ApiError && e.status === 404
            ? "Mağaza tapılmadı."
            : e instanceof ApiError
              ? e.message
              : "Məlumat yüklənmədi."
        );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [slug]);

  if (loading) {
    return (
      <div className="flex min-h-[40vh] flex-col items-center justify-center gap-2 text-gray-500">
        <Loader2 className="h-8 w-8 animate-spin" aria-hidden />
        <p className="text-sm">Mağaza yüklənir…</p>
      </div>
    );
  }

  if (error || !shop) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center">
        <Store className="mx-auto h-14 w-14 text-gray-300 mb-4" aria-hidden />
        <h1 className="text-xl font-semibold text-gray-700 mb-2">Mağaza tapılmadı</h1>
        <p className="text-gray-500 mb-6">{error ?? "Bu ünvanda satıcı mövcud deyil."}</p>
        <Link
          to="/"
          className="inline-block rounded-xl bg-[#65a30d] px-5 py-2.5 text-sm font-semibold text-white hover:bg-[#4d7c0f]"
        >
          Ana səhifə
        </Link>
      </div>
    );
  }

  const banner =
    shop.bannerUrl ??
    "https://images.unsplash.com/photo-1490750967868-88df5691cc6e?w=1400&q=80";

  return (
    <div className="pb-12">
      <div className="relative h-44 sm:h-56 md:h-64 w-full overflow-hidden bg-gray-100">
        <img src={banner} alt="" className="h-full w-full object-cover" />
        <div className="absolute inset-0 bg-linear-to-t from-black/60 to-transparent" />
      </div>

      <div className="max-w-7xl mx-auto px-4 -mt-16 relative z-10">
        <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-lg sm:flex sm:items-start sm:gap-6">
          <div className="mx-auto flex h-24 w-24 shrink-0 items-center justify-center overflow-hidden rounded-xl border border-gray-100 bg-gray-50 sm:mx-0">
            {shop.logoUrl ? (
              <img src={shop.logoUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              <Store className="h-12 w-12 text-[#65a30d]" aria-hidden />
            )}
          </div>
          <div className="mt-4 flex-1 text-center sm:mt-0 sm:text-left">
            <h1 className="text-2xl font-bold text-gray-900">{shop.storeName}</h1>
            {shop.ownerDisplayName && (
              <p className="mt-1 text-sm text-gray-500">Satıcı: {shop.ownerDisplayName}</p>
            )}
            {shop.description && (
              <p className="mt-3 text-sm leading-relaxed text-gray-600">{shop.description}</p>
            )}
            <div className="mt-4 flex flex-wrap items-center justify-center gap-4 text-sm text-gray-600 sm:justify-start">
              <span className="inline-flex items-center gap-1.5 rounded-full bg-lime-50 px-3 py-1 text-lime-800">
                <Package className="h-4 w-4" aria-hidden />
                {shop.activeProductCount} məhsul
              </span>
            </div>
          </div>
        </div>

        {campaigns.length > 0 && (
          <section className="mt-10" aria-labelledby="store-campaigns-heading">
            <h2 id="store-campaigns-heading" className="text-lg font-semibold text-gray-800 mb-4">
              Aktiv kampaniyalar
            </h2>
            <ul className="grid gap-3 sm:grid-cols-2">
              {campaigns.map((c, idx) => {
                const Icon = c.kind.trim().toLowerCase() === "shipping" ? Truck : Percent;
                const scope = scopeSummary(c.scope);
                return (
                  <li
                    key={`${c.name}-${c.startsAt}-${idx}`}
                    className="flex gap-3 rounded-xl border border-lime-100 bg-lime-50/80 p-4 text-sm text-gray-800"
                  >
                    <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white text-[#65a30d] shadow-sm">
                      <Icon className="h-5 w-5" aria-hidden />
                    </span>
                    <div className="min-w-0 flex-1">
                      <p className="font-semibold text-gray-900">{c.name}</p>
                      <p className="mt-0.5 text-gray-600">
                        {campaignKindLabel(c.kind)} —{" "}
                        <span className="font-medium text-lime-900">{c.percent}%</span>
                      </p>
                      <p className="mt-1 text-xs text-gray-500">
                        {formatCampaignDate(c.startsAt)} — {formatCampaignDate(c.endsAt)}
                        {c.minCartAmount != null && c.minCartAmount > 0 && (
                          <> · Minimum səbət: {c.minCartAmount} ₼</>
                        )}
                      </p>
                      {scope && <p className="mt-1 text-xs text-gray-500">{scope}</p>}
                    </div>
                  </li>
                );
              })}
            </ul>
          </section>
        )}

        <section className="mt-10">
          <h2 className="text-lg font-semibold text-gray-800 mb-4">Mağazanın məhsulları</h2>
          {products.length === 0 ? (
            <p className="text-sm text-gray-500 py-8 text-center border border-dashed border-gray-200 rounded-xl">
              Bu mağazada hələ məhsul yoxdur.
            </p>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6">
              {products.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
