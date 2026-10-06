namespace Gulora.Domain.Enums;

/// <summary>İndirim/kupon hesaplama türleri.</summary>
public enum DiscountType
{
    /// <summary>Yüzde bazlı indirim (ör. %10).</summary>
    Percent = 1,

    /// <summary>Sabit tutar indirimi (ör. 50 AZN).</summary>
    Fixed = 2
}
