import { useEffect } from "react";

const SITE_NAME = "Gülora";
const DEFAULT_TITLE = "Gülora — Gül və Hədiyyə";

/**
 * SPA-da route dəyişəndə document.title və istəyə görə meta description yenilənir;
 * unmount-da əvvəlki dəyərlər bərpa olunur (stacked route / geri düymə üçün).
 */
export function usePageMeta(title: string, description?: string) {
  useEffect(() => {
    const prevTitle = document.title;
    const full = title.includes("Gülora") ? title : `${title} | ${SITE_NAME}`;
    document.title = full;

    let meta: HTMLMetaElement | null = null;
    let insertedMeta = false;
    let prevMetaContent: string | null = null;

    if (description) {
      meta = document.querySelector<HTMLMetaElement>('meta[name="description"]');
      if (!meta) {
        meta = document.createElement("meta");
        meta.setAttribute("name", "description");
        document.head.appendChild(meta);
        insertedMeta = true;
      } else {
        prevMetaContent = meta.getAttribute("content");
      }
      meta.setAttribute("content", description);
    }

    return () => {
      document.title = prevTitle || DEFAULT_TITLE;
      if (!meta) return;
      if (insertedMeta) {
        meta.remove();
      } else if (prevMetaContent !== null) {
        meta.setAttribute("content", prevMetaContent);
      } else {
        meta.removeAttribute("content");
      }
    };
  }, [title, description]);
}
