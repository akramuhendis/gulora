export interface District {
  name: string;
  deliveryFee: number;
  sameDayAvailable: boolean;
  minDeliveryDays: number;
}

export interface City {
  name: string;
  districts: District[];
  sameDayCutoff: string;
}

export const cities: City[] = [
  {
    name: "Bakı",
    sameDayCutoff: "14:00",
    districts: [
      { name: "Binəqədi", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Nərimanov", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Nəsimi", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Nizami", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Pirallahı", deliveryFee: 5, sameDayAvailable: false, minDeliveryDays: 1 },
      { name: "Qaradağ", deliveryFee: 8, sameDayAvailable: false, minDeliveryDays: 1 },
      { name: "Sabail", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Sabunçu", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Suraxanı", deliveryFee: 3, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Xətai", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Xəzər", deliveryFee: 5, sameDayAvailable: false, minDeliveryDays: 1 },
      { name: "Yasamal", deliveryFee: 0, sameDayAvailable: true, minDeliveryDays: 0 },
    ],
  },
  {
    name: "Sumqayıt",
    sameDayCutoff: "12:00",
    districts: [
      { name: "Sumqayıt şəhər", deliveryFee: 10, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Corat", deliveryFee: 12, sameDayAvailable: false, minDeliveryDays: 1 },
    ],
  },
  {
    name: "Gəncə",
    sameDayCutoff: "11:00",
    districts: [
      { name: "Gəncə şəhər", deliveryFee: 15, sameDayAvailable: true, minDeliveryDays: 0 },
      { name: "Kəpəz", deliveryFee: 17, sameDayAvailable: false, minDeliveryDays: 1 },
      { name: "Nizami", deliveryFee: 15, sameDayAvailable: true, minDeliveryDays: 0 },
    ],
  },
  {
    name: "Mingəçevir",
    sameDayCutoff: "11:00",
    districts: [
      { name: "Mingəçevir şəhər", deliveryFee: 18, sameDayAvailable: false, minDeliveryDays: 1 },
    ],
  },
  {
    name: "Lənkəran",
    sameDayCutoff: "10:00",
    districts: [
      { name: "Lənkəran şəhər", deliveryFee: 20, sameDayAvailable: false, minDeliveryDays: 1 },
      { name: "Astara", deliveryFee: 25, sameDayAvailable: false, minDeliveryDays: 2 },
    ],
  },
  {
    name: "Quba",
    sameDayCutoff: "10:00",
    districts: [
      { name: "Quba şəhər", deliveryFee: 22, sameDayAvailable: false, minDeliveryDays: 1 },
    ],
  },
  {
    name: "Şəki",
    sameDayCutoff: "10:00",
    districts: [
      { name: "Şəki şəhər", deliveryFee: 25, sameDayAvailable: false, minDeliveryDays: 2 },
    ],
  },
  {
    name: "Naxçıvan",
    sameDayCutoff: "10:00",
    districts: [
      { name: "Naxçıvan şəhər", deliveryFee: 30, sameDayAvailable: false, minDeliveryDays: 2 },
      { name: "Ordubad", deliveryFee: 35, sameDayAvailable: false, minDeliveryDays: 3 },
    ],
  },
];

export function getCityByName(name: string): City | undefined {
  return cities.find((c) => c.name === name);
}

export function getDistrictByName(city: string, district: string): District | undefined {
  return getCityByName(city)?.districts.find((d) => d.name === district);
}

export const DELIVERY_TYPES = [
  {
    value: "same_day" as const,
    label: "Eyni gün kurye",
    description: "Saat 14:00-a qədər sifariş — bu gün çatdırılma",
    icon: "⚡",
    extraFee: 0,
    requiresSameDay: true,
  },
  {
    value: "courier" as const,
    label: "Standart kurye",
    description: "Seçilmiş tarix və vaxtda çatdırılma",
    icon: "🚗",
    extraFee: 0,
    requiresSameDay: false,
  },
  {
    value: "cargo" as const,
    label: "Kargo (2–3 iş günü)",
    description: "Ünvana kargo şirkəti vasitəsilə çatdırılma",
    icon: "📦",
    extraFee: -5,
    requiresSameDay: false,
  },
  {
    value: "pickup" as const,
    label: "Mağazadan götür",
    description: "Nizami küç. 203, Bakı — pulsuz",
    icon: "🏪",
    extraFee: -10,
    requiresSameDay: false,
  },
];

export const INSTALLMENT_OPTIONS = [
  { months: 1, label: "Tək ödəniş", interestRate: 0 },
  { months: 3, label: "3 taksit", interestRate: 0 },
  { months: 6, label: "6 taksit", interestRate: 0 },
  { months: 9, label: "9 taksit", interestRate: 2.5 },
  { months: 12, label: "12 taksit", interestRate: 5 },
];

export const VALID_COUPONS: Record<string, { type: "percent" | "fixed"; value: number; minOrder: number; label: string }> = {
  "GULORA10": { type: "percent", value: 10, minOrder: 50, label: "%10 endirim" },
  "GULORA20": { type: "percent", value: 20, minOrder: 150, label: "%20 endirim" },
  "YAZ2026": { type: "fixed", value: 15, minOrder: 80, label: "15 AZN endirim" },
  "ILKSIFARISH": { type: "percent", value: 15, minOrder: 0, label: "İlk sifariş %15 endirim" },
  "ANALARGUNU": { type: "fixed", value: 25, minOrder: 100, label: "Analar günü 25 AZN endirim" },
};
