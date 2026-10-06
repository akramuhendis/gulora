using Gulora.Contracts.Categories;
using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
        => _categoryRepository = categoryRepository;

    public async Task<IReadOnlyList<CategoryResponse>> Handle(
        GetCategoriesQuery query, CancellationToken ct)
    {
        var roots = await _categoryRepository.GetRootCategoriesAsync(ct);

        var result = new List<CategoryResponse>();
        foreach (var root in roots)
        {
            if (query.OnlyActive && !root.IsActive) continue;
            result.Add(await MapAsync(root, ct));
        }

        return result.AsReadOnly();
    }

    private async Task<CategoryResponse> MapAsync(Category category, CancellationToken ct)
    {
        var children = await _categoryRepository.GetChildrenAsync(category.Id, ct);
        var childResponses = new List<CategoryResponse>();

        foreach (var child in children)
            childResponses.Add(await MapAsync(child, ct));

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.ParentId,
            category.DisplayOrder,
            category.IsActive,
            childResponses.AsReadOnly());
    }
}
