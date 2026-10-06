using FluentValidation;
using Gulora.Contracts.Common;
using Gulora.Contracts.Products;

namespace Gulora.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PagingConstraints.MaxPageSize);
        RuleFor(x => x.SortBy)
            .Must(s => string.IsNullOrWhiteSpace(s) || ProductListSortBy.IsAllowed(s))
            .WithMessage(
                "SortBy geçersiz. Geçerli değerler: newest, price_asc, price_desc, rating, name_asc, name_desc.");        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(x => x.MinPrice!.Value)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("Maksimum fiyat minimum fiyattan küçük olamaz.");
    }
}
