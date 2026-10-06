using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;

namespace Gulora.Application.Reviews.Commands.ModerateReview;

public sealed class ModerateReviewCommandHandler : IRequestHandler<ModerateReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModerateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ModerateReviewCommand cmd, CancellationToken ct)
    {
        var review = await _reviewRepository.GetByIdAsync(cmd.ReviewId, ct)
            ?? throw new NotFoundException("Yorum", cmd.ReviewId);

        if (cmd.Approve)
            review.Approve();
        else
        {
            if (string.IsNullOrWhiteSpace(cmd.RejectionReason))
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["RejectionReason"] = ["Red gerekçesi zorunludur."]
                });

            review.Reject(cmd.RejectionReason);
        }

        // Ürün rating istatistiklerini güncelle
        var (avg, count) = await _reviewRepository.GetRatingStatsAsync(review.ProductId, ct);
        var product = await _productRepository.GetByIdAsync(review.ProductId, ct);
        product?.RecalculateRating(avg, count);

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
