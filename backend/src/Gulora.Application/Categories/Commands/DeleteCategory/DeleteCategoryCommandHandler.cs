using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCategoryCommand cmd, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(cmd.CategoryId, ct)
            ?? throw new NotFoundException("Kategori", cmd.CategoryId);

        category.Deactivate();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
