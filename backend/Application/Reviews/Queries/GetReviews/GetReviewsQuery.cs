using Gulora.Contracts.Common;
using Gulora.Contracts.Reviews;
using MediatR;

namespace Gulora.Application.Reviews.Queries.GetReviews;

/// <summary>
/// Belirli bir ürüne ait müşteri yorumlarını sayfalı olarak listelemek için MediatR sorgusu.
/// </summary>
/// <remarks>
/// Yanıt <see cref="PagedResult{T}"/> içinde toplam kayıt sayısını taşır; işleyici yorum sahibi
/// kullanıcı adlarını toplu kullanıcı yüklemesi ile doldurur.
/// </remarks>
/// <param name="ProductId">Yorumları listelenecek ürünün benzersiz tanımlayıcısı (GUID).</param>
/// <param name="Page">1 tabanlı sayfa numarası; ilk sayfa için 1 kullanılır.</param>
/// <param name="PageSize">Sayfa başına döndürülecek yorum kaydı sayısı (varsayılan 20).</param>
public sealed record GetReviewsQuery(Guid ProductId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ReviewDto>>;
