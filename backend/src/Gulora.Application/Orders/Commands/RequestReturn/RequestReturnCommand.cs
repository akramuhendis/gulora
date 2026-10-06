using MediatR;

namespace Gulora.Application.Orders.Commands.RequestReturn;

/// <summary>Teslim edilmiş sipariş için iade talebi açar.</summary>
public sealed record RequestReturnCommand(
    Guid OrderId,
    Guid UserId,
    string Reason
) : IRequest;
