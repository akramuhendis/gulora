using MediatR;

namespace Gulora.Application.Categories.Commands.UpdateCategory;

/// <summary>Kategori bilgilerini günceller.</summary>
public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int DisplayOrder
) : IRequest;
