namespace Gulora.Contracts.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    int SortOrder,
    string? Icon,
    string? Color,
    string? ImageUrl);
