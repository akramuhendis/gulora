import type { DeliveryType } from "../models/order";
import { DELIVERY_TYPES, VALID_COUPONS, getDistrictByName } from "../data/cities";

export const CART_DISCOUNT_THRESHOLD = 100;

export function computeCartTotals(
  totalPrice: number,
  options?: {
    city?: string;
    district?: string;
    deliveryType?: DeliveryType;
    couponCode?: string;
    installmentMonths?: number;
  }
) {
  const city = options?.city ?? "Bakı";
  const district = options?.district;
  const deliveryType = options?.deliveryType ?? "courier";
  const couponCode = options?.couponCode?.toUpperCase();
  const installmentMonths = options?.installmentMonths ?? 1;

  const districtInfo = district ? getDistrictByName(city, district) : undefined;
  const baseFee = districtInfo ? districtInfo.deliveryFee : 0;
  const deliveryTypeInfo = DELIVERY_TYPES.find((d) => d.value === deliveryType);
  const extraFee = deliveryTypeInfo ? deliveryTypeInfo.extraFee : 0;
  const shipping = Math.max(0, baseFee + extraFee);

  const loyaltyDiscount = totalPrice >= CART_DISCOUNT_THRESHOLD ? totalPrice * 0.05 : 0;

  let couponDiscount = 0;
  if (couponCode && VALID_COUPONS[couponCode]) {
    const c = VALID_COUPONS[couponCode];
    if (totalPrice >= c.minOrder) {
      couponDiscount = c.type === "percent" ? totalPrice * (c.value / 100) : c.value;
    }
  }

  const discount = loyaltyDiscount + couponDiscount;

  let installmentInterest = 0;
  if (installmentMonths > 1) {
    const opt = [3, 6, 9, 12].includes(installmentMonths)
      ? installmentMonths >= 9 ? installmentMonths >= 12 ? 5 : 2.5 : 0
      : 0;
    installmentInterest = (totalPrice + shipping - discount) * (opt / 100);
  }

  const finalPrice = Math.max(0, totalPrice - discount + shipping + installmentInterest);
  const monthlyPayment = installmentMonths > 1 ? finalPrice / installmentMonths : 0;

  return {
    shipping,
    discount,
    loyaltyDiscount,
    couponDiscount,
    installmentInterest,
    finalPrice,
    monthlyPayment,
    installmentMonths,
  };
}

export function applyCoupon(couponCode: string, totalPrice: number) {
  const code = couponCode.toUpperCase().trim();
  const coupon = VALID_COUPONS[code];
  if (!coupon) return { ok: false, error: "Kupon kodu tapılmadı." };
  if (totalPrice < coupon.minOrder)
    return { ok: false, error: `Bu kupon üçün minimum sifariş ${coupon.minOrder} AZN olmalıdır.` };
  const discountAmount = coupon.type === "percent" ? totalPrice * (coupon.value / 100) : coupon.value;
  return { ok: true, label: coupon.label, discountAmount };
}
