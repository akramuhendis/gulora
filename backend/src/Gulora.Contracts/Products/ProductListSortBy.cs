namespace Gulora.Contracts.Products;

/// <summary>
/// GET api/v1/products SortBy parametri — GetProductsQueryValidator ilə eyni dəst.
/// Frontend <c>apiConstants.ts</c> içində təkrarlanır.
/// </summary>
public static class ProductListSortBy
{
    public static readonly string[] Allowed =
    [
        "newest",
        "price_asc",
        "price_desc",
        "rating",
        "name_asc",
        "name_desc"
    ];

    public static bool IsAllowed(string? sortBy) =>
        !string.IsNullOrWhiteSpace(sortBy)
        && Allowed.Contains(sortBy.Trim(), StringComparer.OrdinalIgnoreCase);
}
