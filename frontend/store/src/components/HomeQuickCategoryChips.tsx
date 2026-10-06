import { Link } from "react-router-dom";
import {
  getHomeQuickChipHref,
  HOME_QUICK_CHIPS,
  HOME_QUICK_POPULAR_BADGE_AZ,
  homeQuickChipLabel,
} from "../lib/homeQuickChips";

/**
 * Ana səhifə üfüqi kateqoriya qısayolları (Trendyol tipli dairəvi ikonlar).
 */
export function HomeQuickCategoryChips() {
  return (
    <section aria-label="Tez kateqoriyalar" className="mb-8">
      <div className="flex gap-4 overflow-x-auto pb-2 pt-1 [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden snap-x snap-mandatory">
        {HOME_QUICK_CHIPS.map((chip) => {
          const href = getHomeQuickChipHref(chip);
          const label = homeQuickChipLabel(chip);
          const ringClass = chip.popular
            ? "border-[#0B7A45] ring-1 ring-[#0B7A45]/25"
            : "border-gray-200 hover:border-[#65a30d]/80";
          return (
            <Link
              key={chip.id}
              to={href}
              className="snap-start shrink-0 flex flex-col items-center w-[76px] text-center group"
            >
              <div className="relative">
                {chip.popular ? (
                  <span className="absolute -left-0.5 -top-1 z-10 rounded-md bg-[#0B7A45] px-1.5 py-0.5 text-[9px] font-bold uppercase tracking-wide text-white leading-none">
                    {HOME_QUICK_POPULAR_BADGE_AZ}
                  </span>
                ) : null}
                <div
                  className={`flex h-16 w-16 items-center justify-center rounded-full border-2 bg-white transition-colors ${ringClass}`}
                >
                  <span className="text-[26px] leading-none select-none" aria-hidden>
                    {chip.icon}
                  </span>
                </div>
              </div>
              <span className="mt-2 text-[11px] font-semibold leading-tight text-gray-800 group-hover:text-[#3f6212] line-clamp-2">
                {label}
              </span>
            </Link>
          );
        })}
      </div>
    </section>
  );
}
