import { Link } from "react-router-dom";
import { ChevronRight } from "lucide-react";

export default function AboutPage() {
  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <nav className="flex items-center gap-2 text-sm text-gray-400 mb-8">
        <Link to="/" className="hover:text-[#65a30d]">
          Ana səhifə
        </Link>
        <ChevronRight size={14} className="text-gray-300" />
        <span className="text-gray-700 font-medium">Haqqımızda</span>
      </nav>

      <h1 className="text-3xl font-bold text-gray-900 mb-6">Haqqımızda</h1>
      <div className="prose prose-gray max-w-none space-y-4 text-gray-600 text-sm leading-relaxed">
        <p>
          <strong className="text-gray-800">Gülora</strong> — Azərbaycan üzrə təzə çiçək və
          hədiyyə çatdırılması üzrə ixtisaslaşmış onlayn platformadır. Məqsədimiz sevdiklərinizə
          bir kliklə keyfiyyətli məhsul və etibarlı xidmət təqdim etməkdir.
        </p>
        <p>
          Komandamız sifarişlərinizi diqqətlə hazırlayır, çatdırılma vaxtına riayət edir və
          müştəri dəstəyi ilə hər addımda yanınızdadır.
        </p>
        <p className="text-xs text-gray-400 pt-4">
          Bu məzmun nümunədir; şirkət təsviri və rekvizitlər hazır olanda yenilənəcək.
        </p>
      </div>
      <Link
        to="/mehsullar"
        className="inline-block mt-8 text-sm font-semibold text-[#65a30d] hover:underline"
      >
        Məhsullara bax →
      </Link>
    </div>
  );
}
