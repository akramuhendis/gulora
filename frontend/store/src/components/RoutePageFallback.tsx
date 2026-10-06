/** Marşrut `lazy()` chunk-u yüklənənə qədər — Navbar/Footer qalır */
export default function RoutePageFallback() {
  return (
    <div
      className="flex min-h-[40vh] w-full flex-col items-center justify-center gap-3 text-gray-500"
      aria-busy="true"
      aria-live="polite"
    >
      <div
        className="h-9 w-9 shrink-0 animate-spin rounded-full border-2 border-[#65a30d] border-t-transparent"
        aria-hidden
      />
      <p className="text-sm font-medium text-gray-600">Səhifə yüklənir…</p>
    </div>
  );
}
