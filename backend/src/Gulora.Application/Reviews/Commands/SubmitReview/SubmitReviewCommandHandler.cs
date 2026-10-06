using Gulora.Application.Common.Configuration;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using Gulora.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Gulora.Application.Reviews.Commands.SubmitReview;

public sealed class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, Guid>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IOptions<ReviewPublishOptions> _reviewPublishOptions;

    public SubmitReviewCommandHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IHostEnvironment hostEnvironment,
        IOptions<ReviewPublishOptions> reviewPublishOptions)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
        _reviewPublishOptions = reviewPublishOptions;
    }

    public async Task<Guid> Handle(SubmitReviewCommand cmd, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new NotFoundException("Ürün", cmd.ProductId);

        if (await _reviewRepository.HasUserReviewedProductAsync(cmd.UserId, cmd.ProductId, ct))
            throw new ConflictException("Bu ürün için zaten bir yorumunuz bulunuyor.");

        var review = Review.Create(
            cmd.ProductId, cmd.UserId, cmd.Rating, cmd.Title, cmd.Body, cmd.OrderId);

        // Production xaricində (Development, Staging, boş mühit) avtomatik dərc — yalnız Production + false seçimdə moderasiya
        var autoPublish = !_hostEnvironment.IsProduction()
                          || _reviewPublishOptions.Value.AutoPublishOnSubmit;

        if (autoPublish)
            review.Approve();

        _reviewRepository.Add(review);
        await _unitOfWork.SaveChangesAsync(ct);

        if (autoPublish)
        {
            var (avg, count) = await _reviewRepository.GetRatingStatsAsync(cmd.ProductId, ct);
            product.RecalculateRating(avg, count);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return review.Id;
    }
}
