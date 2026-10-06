using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(GuloraDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Context.Users
            .IgnoreQueryFilters()   // soft delete filtresi devre dışı (deactive kullanıcı için de bakılır)
            .Include(u => u.RefreshTokens)
            // EF.Functions.ILike → PostgreSQL ILIKE; ix_users_email indeksiyle uyumlu
            // olmak için UserConfiguration'da LOWER() fonksiyon indeksi eklenebilir.
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email), ct);

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => EF.Functions.ILike(u.Email, email), ct);

    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token), ct);

    public async Task<User?> GetByEmailVerificationTokenAsync(string token, CancellationToken ct = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .AsSplitQuery()
            .Include(u => u.RefreshTokens)
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token, ct);

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Context.Users
            .IgnoreQueryFilters()
            .AsSplitQuery()
            .Include(u => u.RefreshTokens)
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}
