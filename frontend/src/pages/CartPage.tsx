import { Link } from "react-router-dom";
import { Trash2, Plus, Minus, ShoppingBag, ArrowRight, Tag, Truck } from "lucide-react";
import { useCart } from "../context/CartContext";
import { formatMoney } from "../lib/format";
import { CART_DISCOUNT_THRESHOLD, computeCartTotals } from "../lib/cartPricing";

export default function CartPage() {
  const { items, removeFromCart, updateQuantity, totalPrice, totalItems, clearCart } = useCart();

  const { discount, finalPrice } = computeCartTotals(totalPrice);

  if (items.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-20 text-center">
        <div className="max-w-sm mx-auto">
          <div className="text-8xl mb-6">🛒</div>
          <h2 className="text-2xl font-bold text-gray-700 mb-3">
            Səbətiniz boşdur
          </h2>
          <p className="text-gray-400 mb-8">
            Gözəl çiçəklər və hədiyyələri kəşf etmək üçün ana səhifəyə keçin.
          </p>
          <Link
            to="/"
            className="inline-flex items-center gap-2 bg-[#65a30d] text-white px-6 py-3 rounded-xl font-semibold hover:bg-[#4d7c0f] transition-colors"
          >
            <ShoppingBag size={18} />
            Alış-verişə başla
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">
          Səbətim{" "}
          <span className="text-gray-400 font-normal text-lg">
            ({totalItems} məhsul)
          </span>
        </h1>
        <button
          type="button"
          onClick={clearCart}
          className="text-sm text-gray-400 hover:text-red-500 transition-colors flex items-center gap-1"
        >
          <Trash2 size={14} />
          Səbəti təmizlə
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-3">
          {items.map(({ product, quantity }) => (
            <div
              key={product.id}
              className="bg-white rounded-2xl border border-gray-100 p-4 flex gap-4"
            >
              <Link to={`/urun/${product.slug}`} className="shrink-0">
                <div className="w-20 h-20 rounded-xl overflow-hidden bg-gray-50">
                  <img
                    src={product.image}
                    alt={product.name}
                    className="w-full h-full object-cover"
                  />
                </div>
              </Link>
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <Link
                    to={`/urun/${product.slug}`}
                    className="text-sm font-medium text-gray-800 hover:text-[#65a30d] transition-colors line-clamp-2"
                  >
                    {product.name}
                  </Link>
                  <button
                    type="button"
                    onClick={() => removeFromCart(product.id)}
                    className="shrink-0 text-gray-300 hover:text-red-500 transition-colors p-1"
                  >
                    <Trash2 size={15} />
                  </button>
                </div>

                <div className="flex items-center gap-2 mt-1">
                  {product.isSameDay && (
                    <span className="text-[10px] text-green-700 bg-green-50 border border-green-200 px-1.5 py-0.5 rounded-full">
                      Eyni gün
                    </span>
                  )}
                  {product.isFreeDelivery && (
                    <span className="text-[10px] text-blue-700 bg-blue-50 border border-blue-200 px-1.5 py-0.5 rounded-full">
                      Pulsuz çatdırılma
                    </span>
                  )}
                </div>

                <div className="flex items-center justify-between mt-3">
                  <div className="flex items-center border border-gray-200 rounded-lg overflow-hidden">
                    <button
                      type="button"
                      onClick={() => updateQuantity(product.id, quantity - 1)}
                      className="w-8 h-8 flex items-center justify-center hover:bg-gray-50 text-gray-600 transition-colors"
                    >
                      <Minus size={13} />
                    </button>
                    <span className="w-8 text-center text-sm font-semibold">
                      {quantity}
                    </span>
                    <button
                      type="button"
                      onClick={() => updateQuantity(product.id, quantity + 1)}
                      className="w-8 h-8 flex items-center justify-center hover:bg-gray-50 text-gray-600 transition-colors"
                    >
                      <Plus size={13} />
                    </button>
                  </div>
                  <div className="text-right">
                    <div className="text-base font-bold text-gray-900">
                      {formatMoney(product.price * quantity)}
                    </div>
                    {quantity > 1 && (
                      <div className="text-xs text-gray-400">
                        {formatMoney(product.price)} / ədəd
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))}

          <div className="bg-white rounded-2xl border border-gray-100 p-4">
            <div className="flex items-center gap-2 mb-3">
              <Tag size={16} className="text-[#65a30d]" />
              <span className="font-semibold text-gray-700 text-sm">
                Endirim kodu
              </span>
            </div>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Kupon kodunuzu daxil edin"
                className="flex-1 border border-gray-200 rounded-xl px-3 py-2 text-sm outline-none focus:border-[#65a30d] transition-colors bg-gray-50"
              />
              <button
                type="button"
                className="px-4 py-2 bg-gray-800 text-white text-sm rounded-xl hover:bg-gray-700 transition-colors font-medium"
              >
                Tətbiq et
              </button>
            </div>
          </div>
        </div>

        <div className="lg:col-span-1">
          <div className="bg-white rounded-2xl border border-gray-100 p-5 sticky top-24">
            <h2 className="font-bold text-gray-800 text-lg mb-4">
              Sifariş xülasəsi
            </h2>

            <div className="space-y-3 text-sm">
              <div className="flex justify-between text-gray-600">
                <span>Aralı cəm ({totalItems} məhsul)</span>
                <span>{formatMoney(totalPrice)}</span>
              </div>
              <div className="flex justify-between text-gray-600">
                <span className="flex items-center gap-1">
                  <Truck size={13} className="text-blue-500" />
                  Çatdırılma
                </span>
                <span className="text-green-600 font-medium">Pulsuz</span>
              </div>
              {discount > 0 && (
                <div className="flex justify-between text-green-600">
                  <span>Endirim ({formatMoney(CART_DISCOUNT_THRESHOLD)} və üzəri %5)</span>
                  <span>-{formatMoney(discount)}</span>
                </div>
              )}
              <div className="border-t border-gray-100 pt-3 flex justify-between font-bold text-gray-900 text-base">
                <span>Cəm</span>
                <span>{formatMoney(finalPrice)}</span>
              </div>
            </div>

            {totalPrice < CART_DISCOUNT_THRESHOLD && (
              <div className="mt-3 bg-orange-50 border border-orange-200 rounded-xl px-3 py-2 text-xs text-orange-700">
                <strong>{formatMoney(CART_DISCOUNT_THRESHOLD - totalPrice)}</strong> daha xərcləyin və %5 endirim qazanın!
              </div>
            )}

            <Link
              to="/odeme"
              className="w-full mt-5 bg-[#65a30d] text-white py-3.5 rounded-xl font-semibold flex items-center justify-center gap-2 hover:bg-[#4d7c0f] transition-colors text-sm"
            >
              Ödənişə keç
              <ArrowRight size={16} />
            </Link>

            <Link
              to="/"
              className="block text-center mt-3 text-sm text-gray-500 hover:text-gray-700 transition-colors"
            >
              Alış-verişə davam et
            </Link>

            <div className="mt-4 pt-4 border-t border-gray-100 space-y-2">
              {[
                { icon: "🔒", text: "Təhlükəsiz ödəniş" },
                { icon: "✓", text: "Təzə çiçək zəmanəti" },
                { icon: "↩️", text: "7 gün geri qaytarma hüququ" },
              ].map((b, i) => (
                <div key={i} className="flex items-center gap-2 text-xs text-gray-500">
                  <span>{b.icon}</span>
                  {b.text}
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
