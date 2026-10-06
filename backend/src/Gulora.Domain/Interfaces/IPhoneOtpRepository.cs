using Gulora.Domain.Entities;

namespace Gulora.Domain.Interfaces;

public interface IPhoneOtpRepository : IRepository<PhoneOtpChallenge>
{
    Task<PhoneOtpChallenge?> GetLatestByPhoneAsync(string normalizedPhone, CancellationToken ct = default);

    Task DeleteByPhoneAsync(string normalizedPhone, CancellationToken ct = default);
}
