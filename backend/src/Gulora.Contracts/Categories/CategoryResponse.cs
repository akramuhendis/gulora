namespace Gulora.Contracts.Categories;

/// <summary>
/// Kategori yanıtı — öz-referans hiyerarşi destekli.
/// Alt kategoriler (Children) iç içe geçmiş CategoryResponse listesi olarak gelir.
/// </summary>
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<CategoryResponse> Children);
