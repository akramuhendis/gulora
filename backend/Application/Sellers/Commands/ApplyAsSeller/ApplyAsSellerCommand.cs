using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Sellers.Commands.ApplyAsSeller;

public sealed record ApplyAsSellerCommand(
    string FullName,
    string Company,
    string TaxNumber,
    string Email,
    string Phone,
    string City,
    string Category,
    string Message) : IRequest<Guid>, ICommand;
