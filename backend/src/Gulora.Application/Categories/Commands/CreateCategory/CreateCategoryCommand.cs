using MediatR;

namespace Gulora.Application.Categories.Commands.CreateCategory;

/// <summary>Yeni kategori oluşturur. Başarıda Guid döner.</summary>
public sealed record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Description = null,
    string? ImageUrl = null,
    Guid? ParentId = null,
    int DisplayOrder = 0
) : IRequest<Guid>;
