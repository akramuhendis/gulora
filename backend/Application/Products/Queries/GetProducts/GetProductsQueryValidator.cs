using FluentValidation;

namespace Gulora.Application.Products.Queries.GetProducts;

/// <summary>
/// <see cref="GetProductsQuery"/> gövdesi ve içindeki <see cref="Gulora.Contracts.Products.GetProductsRequest"/> için doğrulama.
/// </summary>
/// <remarks>
/// Sayfalama sınırları ve fiyat aralığı tutarlılığı, API ve depo katmanında anlamsız veya ağır sorguları engeller.
/// </remarks>
public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    /// <summary>
    /// İstek nesnesinin varlığını, sayfa/sayfa boyutu pozitifliğini ve min–max fiyat ilişkisini tanımlar.
    /// </summary>
    public GetProductsQueryValidator()
    {
        // İstek nesnesi zorunlu; null gövde ile handler'a düşmeyi engelle
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request!.Page).GreaterThan(0);
        // Çok büyük sayfa boyutları tek yanıtta aşırı veri taşır
        RuleFor(x => x.Request.PageSize).InclusiveBetween(1, 100);
        // İki sınır da verilmişse minimum fiyat maksimumu aşmamalı
        RuleFor(x => x.Request)
            .Must(r => !r.MinPrice.HasValue || !r.MaxPrice.HasValue || r.MinPrice <= r.MaxPrice)
            .WithMessage("MinPrice cannot exceed MaxPrice.");
    }
}
