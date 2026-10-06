/** `AppLayout` lazy yüklənərkən — tam ekran, giriş sonrası ilk təəssürat */
export default function AppShellFallback() {
  return (
    <div
      className="flex min-h-screen flex-col items-center justify-center gap-4 bg-[#f9f9f9] px-4"
      aria-busy="true"
      aria-live="polite"
    >
      <div className="text-center">
        <p className="text-lg font-bold uppercase tracking-tight text-[#006437]">GÜLORA</p>
        <p className="mt-1 text-sm text-slate-600">Satıcı paneli yüklənir…</p>
      </div>
      <div
        className="h-10 w-10 animate-spin rounded-full border-2 border-[#006437] border-t-transparent"
        aria-hidden
      />
    </div>
  )
}
