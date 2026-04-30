using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using MediatR;

namespace Gulora.Application.Corporate.Commands.SubmitInquiry;

public sealed class SubmitInquiryCommandHandler(
    ICorporateInquiryRepository inquiries) : IRequestHandler<SubmitInquiryCommand, Guid>
{
    public async Task<Guid> Handle(SubmitInquiryCommand request, CancellationToken cancellationToken)
    {
        var inquiry = CorporateInquiry.Create(
            request.Company,
            request.ContactName,
            request.Email,
            request.Phone,
            request.Message,
            request.Occasion,
            request.Quantity,
            request.Budget);

        await inquiries.AddAsync(inquiry, cancellationToken).ConfigureAwait(false);
        await inquiries.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return inquiry.Id;
    }
}
