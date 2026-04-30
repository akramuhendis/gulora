using Gulora.Contracts.Products;
using MediatR;

namespace Gulora.Application.Products.Queries.GetProductBySlug;

/// <summary>
/// Ürün detay sayfası veya paylaşılabilir URL için kullanılan benzersiz slug (SEO yolu) ile tek ürün getiren sorgu.
/// </summary>
/// <remarks>
/// Slug genelde küçük harf ve tire ile normalize edilmiş benzersiz bir yol parçasıdır; bulunamazsa
/// işleyici <see cref="Gulora.Domain.Exceptions.NotFoundException"/> fırlatır.
/// </remarks>
/// <param name="Slug">
/// Ürünün SEO dostu tanımlayıcısı; veritabanında benzersiz kabul edilir ve istemci tarafından URL'de kullanılır.
/// </param>
public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductDto>;
