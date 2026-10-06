using MediatR;

namespace Gulora.Application.Products.Commands.ActivateProduct;

/// <summary>Ürünü yeniden aktif eder (IsActive = true).</summary>
public sealed record ActivateProductCommand(Guid ProductId) : IRequest;
