using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence.Repositories;

internal sealed class WishlistRepository : Repository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(GuloraDbContext context) : base(context) { }

    public async Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await Context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

    public async Task<Wishlist> GetOrCreateAsync(Guid userId, CancellationToken ct = default)
    {
        var wishlist = await Context.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

        if (wishlist is not null)
            return wishlist;

        // Yeni wishlist oluşturulur ve context'e eklenir.
        // Kaydetme sorumluluğu çağırana aittir (IUnitOfWork.SaveChangesAsync).
        // Repository içinde SaveChangesAsync çağırmak UoW desenini ihlal eder ve
        // açık transaction bağlamında veri tutarsızlığına yol açabilir.
        wishlist = Wishlist.CreateForUser(userId);
        Context.Wishlists.Add(wishlist);
        return wishlist;
    }
}
