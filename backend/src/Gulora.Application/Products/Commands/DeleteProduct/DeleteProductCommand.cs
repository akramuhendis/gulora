using MediatR;

namespace Gulora.Application.Products.Commands.DeleteProduct;

/// <summary>Ürünü soft-delete ile devre dışı bırakır (IsActive = false).</summary>
public sealed record DeleteProductCommand(Guid ProductId) : IRequest;
