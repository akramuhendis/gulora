using Gulora.Application.Common.Abstractions;
using MediatR;

namespace Gulora.Application.Corporate.Commands.SubmitInquiry;

public sealed record SubmitInquiryCommand(
    string Company,
    string ContactName,
    string Email,
    string Phone,
    string Message,
    string? Occasion,
    int? Quantity,
    decimal? Budget) : IRequest<Guid>, ICommand;
