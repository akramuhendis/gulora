namespace Gulora.Contracts.Categories;

/// <summary>PUT api/v1/categories/{id} gövdesi (Admin).</summary>
public sealed record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int DisplayOrder);
