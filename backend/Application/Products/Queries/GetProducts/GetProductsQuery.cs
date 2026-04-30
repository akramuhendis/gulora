using Gulora.Contracts.Common;
using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProducts;

/// <summary>
/// Mağaza listeleme ve filtreleme ekranları için sayfalanmış ürün listesini getiren MediatR sorgusu.
/// </summary>
/// <remarks>
/// Filtreler (kategori slug, fiyat aralığı, rozet, aynı gün teslimat vb.) ve sıralama seçenekleri
/// <see cref="GetProductsRequest"/> üzerinden iletilir; yanıt <see cref="PagedResult{T}"/> ile toplam kayıt sayısını taşır.
/// </remarks>
/// <param name="Request">
/// Sayfa numarası, sayfa boyutu, kategori, fiyat sınırları, etiket/rozet ve sıralama dahil tüm liste parametreleri.
/// </param>
public sealed record GetProductsQuery(GetProductsRequest Request) : IRequest<PagedResult<ProductDto>>;
