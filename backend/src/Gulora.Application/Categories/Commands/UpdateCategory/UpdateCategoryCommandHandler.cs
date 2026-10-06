using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCategoryCommand cmd, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(cmd.CategoryId, ct)
            ?? throw new NotFoundException("Kategori", cmd.CategoryId);

        var slugConflict = await _categoryRepository.GetBySlugAsync(cmd.Slug, ct);
        if (slugConflict is not null && slugConflict.Id != cmd.CategoryId)
            throw new ConflictException($"'{cmd.Slug}' slug'ı başka bir kategoride kullanılıyor.");

        category.Update(cmd.Name, cmd.Slug, cmd.Description, cmd.ImageUrl, cmd.DisplayOrder);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
