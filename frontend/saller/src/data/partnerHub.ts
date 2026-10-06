/** Pazaryeri satıcı paneli — statik məzmun (Trendyol Partner tipli duyuru/yönləndirmə). */

export type PartnerAnnouncement = {
  id: string
  title: string
  body: string
  severity: 'info' | 'warning' | 'success'
  publishedAt: string
}

export const PARTNER_ANNOUNCEMENTS: PartnerAnnouncement[] = [
  {
    id: 'ann-1',
    title: 'Yeni: sifariş tarixçəsi və kargo təsdiqi',
    body:
      'Sifariş təfsilatında izləmə nömrəsi əlavə edərkən daşıyıcı seçimi üçün təklif siyahısı göstərilir; SLA üçün status tarixçəsini izləyin.',
    severity: 'success',
    publishedAt: '2026-05-01',
  },
  {
    id: 'ann-2',
    title: 'Maliyyə: həftəlik ödəniş hesabatları (beta)',
    body:
      'Maliyyə bölməsində köçürmə təqvimi və nümunə cədvəllər göstərilir; PDF çıxışı backend hazır olanda aktivləşəcək.',
    severity: 'info',
    publishedAt: '2026-04-22',
  },
  {
    id: 'ann-3',
    title: 'Performans göstəriciləri',
    body:
      'Çatdırılma və məmnuniyyət üçün hədəf SLA-ları Performans səhifəsində izləyin; real hesab backend ilə sinxron olacaq.',
    severity: 'warning',
    publishedAt: '2026-04-10',
  },
]

/** Kargo təsdiqi modalında təklif olunan daşıyıcılar (UI üçün). */
export const CARGO_COMPANY_PRESETS: string[] = [
  'Azərpoçt',
  'FedEx',
  'DHL Express',
  'UPS',
  'Aras Kargo',
  'Yurtiçi Kargo',
  'PTT Kargo',
  'MNG Kargo',
]
