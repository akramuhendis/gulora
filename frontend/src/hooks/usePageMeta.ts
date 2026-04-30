import { useEffect } from "react";

const SITE_NAME = "Gülora";
const DEFAULT_TITLE = "Gülora — Gül və Hədiyyə";

/**
 * Sets document.title and the <meta name="description"> for the current page.
 * Resets to defaults on unmount so navigating back doesn't leave stale titles.
 */
export function usePageMeta(title: string, description?: string) {
  useEffect(() => {
    document.title = `${title} | ${SITE_NAME}`;

    if (description) {
      let meta = document.querySelector<HTMLMetaElement>('meta[name="description"]');
      if (!meta) {
        meta = document.createElement("meta");
        meta.name = "description";
        document.head.appendChild(meta);
      }
      meta.content = description;
    }

    return () => {
      document.title = DEFAULT_TITLE;
    };
  }, [title, description]);
}
