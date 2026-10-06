using Gulora.Domain.Enums;

namespace Gulora.Application.Common.Extensions;

/// <summary>Domain enum'larını insan okunabilir etiketlere dönüştüren yardımcı metodlar.</summary>
public static class EnumExtensions
{
    public static string ToDisplayString(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Beklemede",
        OrderStatus.Confirmed => "Onaylandı",
        OrderStatus.Processing => "Hazırlanıyor",
        OrderStatus.Shipped => "Kargoda",
        OrderStatus.Delivered => "Teslim Edildi",
        OrderStatus.Cancelled => "İptal Edildi",
        OrderStatus.ReturnRequested => "İade Talebi",
        OrderStatus.Returned => "İade Edildi",
        _ => status.ToString()
    };

    public static string ToDisplayString(this PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Ödeme Bekleniyor",
        PaymentStatus.Completed => "Ödendi",
        PaymentStatus.Failed => "Ödeme Başarısız",
        PaymentStatus.Refunded => "İade Edildi",
        PaymentStatus.PartiallyRefunded => "Kısmi İade",
        _ => status.ToString()
    };

    public static string ToDisplayString(this DeliveryType type) => type switch
    {
        DeliveryType.Standard => "Standart Teslimat",
        DeliveryType.Express => "Ekspres Teslimat",
        DeliveryType.SameDay => "Aynı Gün Teslimat",
        _ => type.ToString()
    };
}
