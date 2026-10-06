using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var existing = await _categoryRepository.GetBySlugAsync(cmd.Slug, ct);
        if (existing is not null)
            throw new ConflictException($"'{cmd.Slug}' slug'ı zaten kullanılıyor.");

        if (cmd.ParentId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(cmd.ParentId.Value, ct)
                ?? throw new NotFoundException("Üst kategori", cmd.ParentId.Value);
        }

        var category = Category.Create(
            cmd.Name, cmd.Slug, cmd.Description, cmd.ImageUrl, cmd.ParentId, cmd.DisplayOrder);

        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return category.Id;
    }
}
