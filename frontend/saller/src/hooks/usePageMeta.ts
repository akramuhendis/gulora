import { useEffect } from 'react'

export type PageMetaOptions = {
  suffix?: string
  /** Sənəd `<meta name="description">` — SEO / a11y üçün qısa mətn */
  description?: string | null
}

const DEFAULT_SUFFIX = 'Gülora — Satıcı paneli'

function normalizeOpts(suffixOrOpts: string | PageMetaOptions | undefined): Required<Pick<PageMetaOptions, 'suffix'>> & {
  description: string | null | undefined
} {
  if (suffixOrOpts === undefined) return { suffix: DEFAULT_SUFFIX, description: undefined }
  if (typeof suffixOrOpts === 'string') return { suffix: suffixOrOpts, description: undefined }
  return {
    suffix: suffixOrOpts.suffix ?? DEFAULT_SUFFIX,
    description: suffixOrOpts.description,
  }
}

/**
 * SPA-da route dəyişəndə sənəd başlığını (və istəyə görə meta təsvirini) yeniləyir.
 */
export function usePageMeta(title: string, suffixOrOpts: string | PageMetaOptions = DEFAULT_SUFFIX) {
  const { suffix, description } = normalizeOpts(suffixOrOpts)

  useEffect(() => {
    const prev = document.title
    document.title = title.includes('Gülora') ? title : `${title} | ${suffix}`
    return () => {
      document.title = prev
    }
  }, [title, suffix])

  useEffect(() => {
    if (!description) return

    let el = document.querySelector('meta[name="description"]') as HTMLMetaElement | null
    const inserted = !el
    if (!el) {
      el = document.createElement('meta')
      el.setAttribute('name', 'description')
      document.head.appendChild(el)
    }
    const prevContent = el.getAttribute('content') ?? ''
    el.setAttribute('content', description)
    return () => {
      if (inserted) el?.remove()
      else if (prevContent) el.setAttribute('content', prevContent)
      else el.removeAttribute('content')
    }
  }, [description])
}
