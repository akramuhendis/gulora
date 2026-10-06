import { useMemo, useState } from "react";
import { FALLBACK_IMG } from "../lib/catalogMap";
import { resolveMediaUrlOrFallback } from "../lib/mediaUrl";

type ProductCoverImageProps = {
  src: string | null | undefined;
  alt: string;
  className?: string;
  fallbackEmoji?: string;
};

type InnerProps = {
  resolvedSrc: string;
  alt: string;
  className?: string;
  fallbackEmoji: string;
};

/** `key={resolvedSrc}` ilə src dəyişəndə daxili state sıfırlanır — effectdə setState ehtiyacı yoxdur. */
function ProductCoverImageInner({
  resolvedSrc,
  alt,
  className,
  fallbackEmoji,
}: InnerProps) {
  const [currentSrc, setCurrentSrc] = useState(resolvedSrc);
  const [failed, setFailed] = useState(false);

  if (failed) {
    return (
      <div
        className={`flex items-center justify-center bg-lime-50 text-2xl ${className ?? ""}`}
        aria-hidden
      >
        {fallbackEmoji}
      </div>
    );
  }

  return (
    <img
      src={currentSrc}
      alt={alt}
      className={className}
      loading="lazy"
      decoding="async"
      onError={() => {
        if (currentSrc !== FALLBACK_IMG) {
          setCurrentSrc(FALLBACK_IMG);
          return;
        }
        setFailed(true);
      }}
    />
  );
}

/** Məhsul kapak şəkli — URL normallaşdırma + yüklənmə xətasında ehtiyat. */
export default function ProductCoverImage({
  src,
  alt,
  className,
  fallbackEmoji = "🌸",
}: ProductCoverImageProps) {
  const resolvedSrc = useMemo(() => resolveMediaUrlOrFallback(src, FALLBACK_IMG), [src]);

  return (
    <ProductCoverImageInner
      key={resolvedSrc}
      resolvedSrc={resolvedSrc}
      alt={alt}
      className={className}
      fallbackEmoji={fallbackEmoji}
    />
  );
}
