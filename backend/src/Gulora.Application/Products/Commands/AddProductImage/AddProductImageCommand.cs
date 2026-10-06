using MediatR;

namespace Gulora.Application.Products.Commands.AddProductImage;

public sealed record AddProductImageCommand(
    Guid ProductId,
    string Url,
    string? AltText,
    int DisplayOrder) : IRequest<Guid>;
