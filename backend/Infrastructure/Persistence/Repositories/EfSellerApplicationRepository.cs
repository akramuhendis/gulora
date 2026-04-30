using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Gulora.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

public sealed class EfSellerApplicationRepository(GuloraDbContext db) : ISellerApplicationRepository
{
    public Task<bool> HasPendingApplicationAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.SellerApplications.AnyAsync(
            a => a.Email == normalized
                 && (a.Status == SellerApplicationStatus.Pending || a.Status == SellerApplicationStatus.UnderReview),
            ct);
    }

    public async Task AddAsync(SellerApplication application, CancellationToken ct = default) =>
        await db.SellerApplications.AddAsync(application, ct).ConfigureAwait(false);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
