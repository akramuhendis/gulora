using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class PhoneOtpRepository : Repository<PhoneOtpChallenge>, IPhoneOtpRepository
{
    public PhoneOtpRepository(GuloraDbContext context) : base(context) { }

    public async Task<PhoneOtpChallenge?> GetLatestByPhoneAsync(string normalizedPhone, CancellationToken ct = default)
        => await Context.PhoneOtpChallenges
            .Where(c => c.PhoneNumber == normalizedPhone)
            .OrderByDescending(c => c.SentAt)
            .FirstOrDefaultAsync(ct);

    public async Task DeleteByPhoneAsync(string normalizedPhone, CancellationToken ct = default)
        => await Context.PhoneOtpChallenges
            .Where(c => c.PhoneNumber == normalizedPhone)
            .ExecuteDeleteAsync(ct);
}
