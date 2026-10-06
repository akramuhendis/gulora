using Gulora.Contracts.SellerApplications;
using MediatR;

namespace Gulora.Application.SellerApplications.Commands.SubmitSellerApplication;

public sealed record SubmitSellerApplicationCommand(
    string FullName,
    string Company,
    string TaxNumber,
    string Email,
    string Phone,
    string City,
    string Category,
    string? Message) : IRequest<SellerApplicationResponse>;
