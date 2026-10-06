using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Reviews.Commands.UpdateReview;

public sealed class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateReviewCommand cmd, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdAsync(cmd.ReviewId, ct)
            ?? throw new NotFoundException("Yorum", cmd.ReviewId);

        if (review.UserId != cmd.UserId)
            throw new ForbiddenException("Bu yorumu düzenleme yetkiniz yok.");

        review.Update(cmd.Rating, cmd.Title, cmd.Body);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
