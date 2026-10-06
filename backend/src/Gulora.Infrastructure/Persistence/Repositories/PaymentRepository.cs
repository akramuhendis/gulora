using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(GuloraDbContext context) : base(context) { }

    public async Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
        => await Context.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == key, ct);

    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken ct = default)
        => await Context.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
}
