using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

public interface ISellerApplicationRepository : IRepository<SellerApplication>
{
    Task<bool> HasPendingApplicationForTaxNumberAsync(string taxNumber, CancellationToken ct = default);
}
