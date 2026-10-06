using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class SellerApplicationRepository : Repository<SellerApplication>, ISellerApplicationRepository
{
    public SellerApplicationRepository(GuloraDbContext context) : base(context) { }

    public async Task<bool> HasPendingApplicationForTaxNumberAsync(string taxNumber, CancellationToken ct = default)
    {
        var key = taxNumber.Trim();
        return await Context.SellerApplications.AnyAsync(
            a => a.TaxNumber == key
                 && (a.Status == SellerApplicationStatus.Pending
                     || a.Status == SellerApplicationStatus.UnderReview),
            ct);
    }
}
