import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { BookOpen, HelpCircle, LifeBuoy, MessageSquare, Search, Truck } from 'lucide-react'
import { usePageMeta } from '../hooks/usePageMeta'

type FaqItem = { q: string; a: string; tags: string[] }

const FAQ: FaqItem[] = [
  {
    q: 'Çox satıcılı səbətdə məbləğ necə hesablanır?',
    a:
      'İcmal və maliyyə göstəriciləri sizə aid məhsul sətirlərinin cəmini göstərir; tam sifariş ümumi məbləği fərqli ola bilər.',
    tags: ['maliyyə', 'sifariş'],
  },
  {
    q: 'Kargoya verəndən sonra izləmə dəyişə bilərmi?',
    a:
      'Bəli — sifariş təfsilatında izləmə tarixçəsi saxlanılır; yanlış daxil olmuş kod üçün dəstəklə əlaqə saxlayın və ya yenidən təsdiq edin.',
    tags: ['kargo', 'sifariş'],
  },
  {
    q: 'Məhsul şəkilləri haradan yüklənir?',
    a:
      'Məhsul yaradıb redaktə edərkən üz qabığı yükləmə endpoint-i vasitəsilə CDN URL əldə olunur; vitrinə avtomatik düşür.',
    tags: ['məhsul', 'media'],
  },
  {
    q: 'Endirim kampaniyası nə vaxt aktiv olacaq?',
    a:
      'Kampaniya motoru backend tarafında aktiv olanda yaradılan kampaniyalar vitrin qiymətlərinə tətbiq olunacaq; indi UI təklif/taslak rejimindədir.',
    tags: ['kampaniya', 'qiymət'],
  },
]

export default function HelpCenterPage() {
  usePageMeta('Yardım mərkəzi')

  const [q, setQ] = useState('')

  const filtered = useMemo(() => {
    const s = q.trim().toLowerCase()
    if (!s) return FAQ
    return FAQ.filter(
      (f) =>
        f.q.toLowerCase().includes(s) ||
        f.a.toLowerCase().includes(s) ||
        f.tags.some((t) => t.includes(s))
    )
  }, [q])

  return (
    <div>
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
            Yardım mərkəzi
          </h1>
          <p className="mt-2 max-w-3xl text-slate-600">
            Trendyol Partner tipli pazaryeri axını üçün tez-tez verilən suallar, əməliyyat xətləri və
            dəstək kanalları (UI). Əməliyyat müqaviləsi və ödəniş qaydaları üçün əməliyyat komandası ilə
            təsdiqləyin.
          </p>
        </div>
        <Link
          to="/messages"
          className="inline-flex shrink-0 items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-800 shadow-sm hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] focus-visible:ring-offset-2"
        >
          <MessageSquare className="h-4 w-4 text-[#006437]" aria-hidden />
          Mesajlar
        </Link>
      </div>

      <section className="mt-10 grid gap-4 lg:grid-cols-3" aria-labelledby="help-cards">
        <h2 id="help-cards" className="sr-only">
          Tez keçidlər
        </h2>
        <article className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-[#006437]/10 text-[#006437]">
            <Truck className="h-6 w-6" aria-hidden />
          </div>
          <h3 className="mt-4 font-semibold text-slate-900">Kargo və SLA</h3>
          <p className="mt-2 text-sm text-slate-600">
            Hazırlıq → kargoya vermə → çatdırılma axını üçün sifariş təfsilatından istifadə edin.
          </p>
          <Link
            to="/orders"
            className="mt-4 inline-flex text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
          >
            Sifarişlər
          </Link>
        </article>
        <article className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-emerald-100 text-emerald-900">
            <BookOpen className="h-6 w-6" aria-hidden />
          </div>
          <h3 className="mt-4 font-semibold text-slate-900">Məhsul və inventar</h3>
          <p className="mt-2 text-sm text-slate-600">
            Stok xəbərdarlıqları inventar ekranında; qiymət və endirimlər məhsul redaktəsində.
          </p>
          <div className="mt-4 flex flex-wrap gap-3">
            <Link
              to="/products"
              className="inline-flex text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
            >
              Məhsullar
            </Link>
            <Link
              to="/inventory"
              className="inline-flex text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
            >
              İnventar
            </Link>
          </div>
        </article>
        <article className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-indigo-100 text-indigo-900">
            <LifeBuoy className="h-6 w-6" aria-hidden />
          </div>
          <h3 className="mt-4 font-semibold text-slate-900">Maliyyə və hesabat</h3>
          <p className="mt-2 text-sm text-slate-600">
            GMV, gözləyən ödəniş və nümunə əməliyyatlar üçün maliyyə bölməsinə baxın.
          </p>
          <Link
            to="/finance"
            className="mt-4 inline-flex text-sm font-semibold text-[#006437] hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[#006437] rounded"
          >
            Maliyyə
          </Link>
        </article>
      </section>

      <section className="mt-10 rounded-xl border border-slate-200 bg-white p-6 shadow-sm" aria-labelledby="faq-heading">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div className="flex items-start gap-3">
            <HelpCircle className="mt-0.5 h-8 w-8 text-[#006437]" aria-hidden />
            <div>
              <h2 id="faq-heading" className="text-lg font-semibold text-slate-900">
                Tez-tez verilən suallar
              </h2>
              <p className="mt-1 text-sm text-slate-600">
                Axtarış mətni sual və cavabda, həmçinin teqlərdə işləyir.
              </p>
            </div>
          </div>
          <div className="relative w-full sm:max-w-md">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" aria-hidden />
            <input
              type="search"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="Məs: kargo, ödəniş…"
              className="w-full rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm"
              aria-label="FAQ axtarışı"
            />
          </div>
        </div>

        <ul className="mt-6 divide-y divide-slate-100">
          {filtered.map((f) => (
            <li key={f.q} className="py-5 first:pt-0">
              <p className="font-semibold text-slate-900">{f.q}</p>
              <p className="mt-2 text-sm text-slate-700">{f.a}</p>
              <p className="mt-2 text-xs text-slate-500">{f.tags.map((t) => `#${t}`).join(' ')}</p>
            </li>
          ))}
        </ul>
        {filtered.length === 0 && (
          <p className="py-10 text-center text-sm text-slate-600">Uyğun sual tapılmadı.</p>
        )}
      </section>

      <section className="mt-10 rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6">
        <h2 className="text-base font-semibold text-slate-900">Dəstək kanalları</h2>
        <ul className="mt-3 list-inside list-disc space-y-2 text-sm text-slate-700">
          <li>
            Satıcı mesajları: <Link className="font-semibold text-[#006437] hover:underline" to="/messages">Mesajlar</Link>{' '}
            (API qoşulanda canlı mövzular).
          </li>
          <li>Əməliyyat ofisinə e-poçt və ya telefon — müqavilədə göstərilən kanallar üzrə.</li>
          <li>Təcili kargo düzəlişi üçün sifariş nömrəsi ilə müraciət edin.</li>
        </ul>
      </section>
    </div>
  )
}
