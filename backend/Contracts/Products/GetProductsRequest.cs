namespace Gulora.Contracts.Products;

public record GetProductsRequest(
    string? CategorySlug = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Tag = null,
    string? Badge = null,
    bool? IsSameDay = null,
    bool? IsFreeDelivery = null,
    string? SortBy = null,
    int Page = 1,
    int PageSize = 20);
