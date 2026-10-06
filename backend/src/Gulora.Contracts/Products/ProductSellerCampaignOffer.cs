namespace Gulora.Contracts.Products;

/// <summary>
/// Satıcı kampaniya planından vitrinə ötürülən təklif.
/// <c>discount</c> — məhsul qiymətinə tətbiq; <c>shipping</c> — yalnız məlumat (qiymət dəyişmir).
/// </summary>
public sealed record ProductSellerCampaignOffer(
    string Name,
    int Percent,
    decimal PriceAfterDiscount,
    decimal? MinimumCartAmount,
    string Kind = "discount");
