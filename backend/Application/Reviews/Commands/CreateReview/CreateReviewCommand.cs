using Gulora.Application.Common.Abstractions;
using Gulora.Contracts.Reviews;
using MediatR;

namespace Gulora.Application.Reviews.Commands.CreateReview;

/// <summary>
/// Oturum açmış kullanıcının belirli bir ürün için yıldız puanı ve metin yorumu göndermesini temsil eden yazma komutu.
/// </summary>
/// <remarks>
/// Komut <see cref="ICommand"/> ile işaretlenebilir (ör. audit, transactional davranış); yanıt oluşturulan
/// yorumun <see cref="ReviewDto"/> temsilidir.
/// </remarks>
/// <param name="ProductId">Yorumlanacak ürünün benzersiz tanımlayıcısı.</param>
/// <param name="UserId">Yorumu yazan kullanıcının benzersiz tanımlayıcısı (genelde mevcut kullanıcı id'si).</param>
/// <param name="Rating">1 ile 5 arası inclusive puan değeri.</param>
/// <param name="Comment">Kullanıcının serbest metin yorum içeriği.</param>
public sealed record CreateReviewCommand(
    Guid ProductId,
    Guid UserId,
    int Rating,
    string Comment) : IRequest<ReviewDto>, ICommand;
