import { Link } from "react-router-dom";
import { ChevronRight, Calendar } from "lucide-react";

const posts = [
  {
    slug: "gul-secimi",
    title: "Buket seçərkən nələrə diqqət yetirməli?",
    excerpt: "Təzəlik, mövsüm və çatdırılma üçün qısa tövsiyələr.",
    date: "2026-04-10",
  },
  {
    slug: "eyni-gun-catdirilma",
    title: "Eyni gün çatdırılma necə işləyir?",
    excerpt: "Sifariş saatları və əhatə dairəsi haqqında.",
    date: "2026-03-28",
  },
  {
    slug: "hediye-ideyalari",
    title: "Xüsusi günlər üçün hədiyyə ideyaları",
    excerpt: "Ad günü, 8 Mart və digər tədbirlər üçün ilham.",
    date: "2026-03-15",
  },
];

export default function BlogPage() {
  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-8">
        <Link to="/" className="hover:text-[#65a30d]">
          Ana səhifə
        </Link>
        <ChevronRight size={14} className="text-gray-300" />
        <span className="text-gray-700 font-medium">Bloq</span>
      </nav>

      <h1 className="text-3xl font-bold text-gray-900 mb-2">Bloq</h1>
      <p className="text-sm text-gray-500 mb-10">
        Çiçək, hədiyyə və çatdırılma haqqında məqalələr.
      </p>

      <ul className="space-y-6">
        {posts.map((post) => (
          <li
            key={post.slug}
            className="bg-white rounded-2xl border border-gray-100 p-5 hover:border-lime-200 transition-colors"
          >
            <div className="flex items-center gap-2 text-xs text-gray-400 mb-2">
              <Calendar size={12} />
              {new Date(post.date).toLocaleDateString("az-AZ", {
                day: "numeric",
                month: "long",
                year: "numeric",
              })}
            </div>
            <h2 className="text-lg font-bold text-gray-800 mb-2">{post.title}</h2>
            <p className="text-sm text-gray-600 mb-3">{post.excerpt}</p>
            <span className="text-xs text-gray-400">Tam mətn tezliklə əlavə olunacaq.</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
