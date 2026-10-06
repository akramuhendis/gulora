using MediatR;

namespace Gulora.Application.Categories.Commands.DeleteCategory;

/// <summary>Kategoriyi devre dışı bırakır (soft delete). Alt kategoriler ve ürünler etkilenmez.</summary>
public sealed record DeleteCategoryCommand(Guid CategoryId) : IRequest;
