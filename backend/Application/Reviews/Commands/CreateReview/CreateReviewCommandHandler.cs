using Gulora.Application.Common.Interfaces;
using Gulora.Contracts.Reviews;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Reviews.Commands.CreateReview;

/// <summary>
/// Yeni ürün yorumu oluşturur: ürün ve kullanıcı varlığını doğrular, aynı kullanıcı–ürün için ikinci yorumu engeller, kalıcı kaydeder.
/// </summary>
/// <remarks>
/// İş kuralı ihlalleri <see cref="DomainException"/> veya <see cref="NotFoundException"/> ile bildirilir; başarıda
/// dönüş değeri oluşturulan yorumun API sözleşmesine uygun DTO'sudur.
/// </remarks>
public sealed class CreateReviewCommandHandler(
    IReviewRepository reviews,
    IProductRepository products,
    IUserRepository users) : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    /// <summary>
    /// Ürün varlığını doğrular, mükerrer yorum kontrolü yapar, kullanıcıyı yükler, <see cref="Review"/> oluşturur ve kaydeder.
    /// </summary>
    /// <param name="request">Ürün, kullanıcı, puan ve yorum metni.</param>
    /// <param name="cancellationToken">İptal belirteci.</param>
    /// <returns>Oluşturulan yorumun DTO temsili (görünen ad dahil).</returns>
    /// <exception cref="NotFoundException">Ürün veya kullanıcı bulunamadığında.</exception>
    /// <exception cref="DomainException">Aynı kullanıcı aynı ürüne daha önce yorum yazmışsa.</exception>
    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(request.ProductId, cancellationToken).ConfigureAwait(false);
        if (product is null)
            throw new NotFoundException("Product", request.ProductId);

        // Aynı kullanıcı aynı ürüne ikinci kez yorum yazamaz (iş kuralı)
        if (await reviews.HasReviewedAsync(request.UserId, request.ProductId, cancellationToken).ConfigureAwait(false))
            throw new DomainException("You have already submitted a review for this product.");

        var user = await users.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Alan kuralları varlık fabrikasında; ardından depo ve birim işi kalıcılık
        var review = Review.Create(request.ProductId, request.UserId, request.Rating, request.Comment);
        await reviews.AddAsync(review, cancellationToken).ConfigureAwait(false);
        await reviews.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // API yanıtında gösterim adı olarak ad ve soyad birleştirilir
        var fullName = $"{user.FirstName} {user.LastName}";
        return new ReviewDto(review.Id, fullName, review.Rating, review.Comment, review.IsVerified, review.CreatedAt);
    }
}
