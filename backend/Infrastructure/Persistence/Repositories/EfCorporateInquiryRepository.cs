using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfCorporateInquiryRepository(GuloraDbContext db) : ICorporateInquiryRepository
{
    public async Task AddAsync(CorporateInquiry inquiry, CancellationToken ct = default) =>
        await db.CorporateInquiries.AddAsync(inquiry, ct).ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
