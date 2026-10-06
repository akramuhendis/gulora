namespace Gulora.Contracts.Categories;

/// <summary>POST api/v1/categories gövdesi (Admin).</summary>
public sealed record CreateCategoryRequest(
    string Name,
    string Slug,
    string? Description = null,
    string? ImageUrl = null,
    Guid? ParentId = null,
    int DisplayOrder = 0);
