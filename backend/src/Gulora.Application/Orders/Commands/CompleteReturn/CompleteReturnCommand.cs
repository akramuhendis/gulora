using MediatR;

namespace Gulora.Application.Orders.Commands.CompleteReturn;

public sealed record CompleteReturnCommand(
    Guid OrderId,
    Guid AdminUserId,
    string? Note,
    Guid? SellerScopeUserId = null) : IRequest;
