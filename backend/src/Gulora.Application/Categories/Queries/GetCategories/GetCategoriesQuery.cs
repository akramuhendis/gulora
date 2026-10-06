using Gulora.Contracts.Categories;
using MediatR;

namespace Gulora.Application.Categories.Queries.GetCategories;

/// <summary>Tüm kök kategorileri alt kategorileriyle birlikte getirir.</summary>
public sealed record GetCategoriesQuery(bool OnlyActive = true) : IRequest<IReadOnlyList<CategoryResponse>>;
