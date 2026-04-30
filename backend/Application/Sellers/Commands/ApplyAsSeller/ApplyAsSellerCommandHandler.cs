using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Exceptions;
using MediatR;

namespace Gulora.Application.Sellers.Commands.ApplyAsSeller;

public sealed class ApplyAsSellerCommandHandler(
    ISellerApplicationRepository applications) : IRequestHandler<ApplyAsSellerCommand, Guid>
{
    public async Task<Guid> Handle(ApplyAsSellerCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await applications.HasPendingApplicationAsync(email, cancellationToken).ConfigureAwait(false))
            throw new DomainException("An active application for this email address already exists.");

        var application = SellerApplication.Create(
            request.FullName,
            request.Company,
            request.TaxNumber,
            email,
            request.Phone,
            request.City,
            request.Category,
            request.Message);

        await applications.AddAsync(application, cancellationToken).ConfigureAwait(false);
        await applications.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return application.Id;
    }
}
