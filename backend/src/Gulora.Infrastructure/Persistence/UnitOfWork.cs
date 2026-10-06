using Gulora.Domain.Entities;
using Gulora.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gulora.Infrastructure.Persistence;

/// <summary>
/// Unit of Work uygulaması — tek işlem kapsamında tüm repository değişikliklerini
/// atomik olarak commit eder ve domain olaylarını dağıtır (interceptor üzerinden).
/// gulora-infrastructure-contracts.mdc: IUnitOfWork uygulaması.
///
/// NOT: GuloraDbContext DI tarafından yönetildiğinden bu sınıf içinde
/// context.Dispose() çağrılmaz; yalnızca açık transaction kaynakları temizlenir.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly GuloraDbContext _context;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public UnitOfWork(GuloraDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await SaveChangesCoreAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("Zaten açık bir transaction var.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("Commit edilecek açık transaction bulunamadı.");

        try
        {
            await SaveChangesCoreAsync(ct);
            await _currentTransaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    // IDisposable — yalnızca transaction kaynağı temizlenir;
    // GuloraDbContext DI scope'u tarafından yönetilir, burada dispose edilmez.
    public void Dispose()
    {
        if (_disposed) return;
        _currentTransaction?.Dispose();
        _currentTransaction = null;
        _disposed = true;
    }

    // IAsyncDisposable — async kullanım için tercih edilen temizleme yolu.
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
        _disposed = true;
    }

    /// <summary>
    /// User koleksiyonuna eklenen yeni alt varlıklar bazen yanlışlıkla
    /// <see cref="EntityState.Modified"/> kalır; UPDATE 0 satır → concurrency hatası.
    /// DB'de satırı olmayan Modified kayıtlar <see cref="EntityState.Added"/> yapılır.
    /// </summary>
    private async Task<int> SaveChangesCoreAsync(CancellationToken ct)
    {
        await CoerceNewRefreshTokensToAddedStateAsync(ct);
        await CoerceNewAddressesToAddedStateAsync(ct);
        await CoerceNewProductImagesToAddedStateAsync(ct);
        await CoerceNewOrderTrackingsToAddedStateAsync(ct);
        await CoerceNewWishlistItemsToAddedStateAsync(ct);
        await CoerceNewWishlistsToAddedStateAsync(ct);
        return await _context.SaveChangesAsync(ct);
    }

    private async Task CoerceNewRefreshTokensToAddedStateAsync(CancellationToken ct)
    {
        var modified = _context.ChangeTracker
            .Entries<RefreshToken>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity.Id)
            .Distinct()
            .ToList();

        if (modified.Count == 0)
            return;

        var existing = await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => modified.Contains(rt.Id))
            .Select(rt => rt.Id)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<RefreshToken>())
        {
            if (entry.State == EntityState.Modified && !existingSet.Contains(entry.Entity.Id))
                entry.State = EntityState.Added;
        }
    }

    private async Task CoerceNewAddressesToAddedStateAsync(CancellationToken ct)
    {
        var modified = _context.ChangeTracker
            .Entries<Address>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity.Id)
            .Distinct()
            .ToList();

        if (modified.Count == 0)
            return;

        var existing = await _context.Addresses
            .AsNoTracking()
            .Where(a => modified.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<Address>())
        {
            if (entry.State == EntityState.Modified && !existingSet.Contains(entry.Entity.Id))
                entry.State = EntityState.Added;
        }
    }

    /// <summary>
    /// Aggregate-a əlavə olunan yeni <see cref="ProductImage"/> bəzən yanlışlıqla
    /// <see cref="EntityState.Modified"/> qalır → UPDATE 0 sətir → concurrency istisnası.
    /// Bazada yoxdursa <see cref="EntityState.Added"/> edir.
    /// </summary>
    private async Task CoerceNewProductImagesToAddedStateAsync(CancellationToken ct)
    {
        var modifiedIds = _context.ChangeTracker
            .Entries<ProductImage>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity.Id)
            .Distinct()
            .ToList();

        if (modifiedIds.Count == 0)
            return;

        var existing = await _context.ProductImages
            .AsNoTracking()
            .Where(pi => modifiedIds.Contains(pi.Id))
            .Select(pi => pi.Id)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<ProductImage>())
        {
            if (entry.State == EntityState.Modified && !existingSet.Contains(entry.Entity.Id))
                entry.State = EntityState.Added;
        }
    }

    /// <summary>
    /// Durum değişimində aggregate-ə əlavə olunan yeni <see cref="OrderTracking"/> bəzən
    /// <see cref="EntityState.Modified"/> qalır → UPDATE 0 sətir → optimistic concurrency istisnası.
    /// Bazada yoxdursa <see cref="EntityState.Added"/> edir.
    /// </summary>
    private async Task CoerceNewOrderTrackingsToAddedStateAsync(CancellationToken ct)
    {
        var modifiedIds = _context.ChangeTracker
            .Entries<OrderTracking>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity.Id)
            .Distinct()
            .ToList();

        if (modifiedIds.Count == 0)
            return;

        var existing = await _context.OrderTrackings
            .AsNoTracking()
            .Where(t => modifiedIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<OrderTracking>())
        {
            if (entry.State == EntityState.Modified && !existingSet.Contains(entry.Entity.Id))
                entry.State = EntityState.Added;
        }
    }

    /// <summary>
    /// Wishlist aggregate-ə əlavə olunan yeni <see cref="WishlistItem"/> bəzən
    /// <see cref="EntityState.Modified"/> qalır → UPDATE 0 sətir → concurrency istisnası.
    /// </summary>
    private async Task CoerceNewWishlistItemsToAddedStateAsync(CancellationToken ct)
    {
        var modifiedIds = _context.ChangeTracker
            .Entries<WishlistItem>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity.Id)
            .Distinct()
            .ToList();

        if (modifiedIds.Count == 0)
            return;

        var existing = await _context.WishlistItems
            .AsNoTracking()
            .Where(i => modifiedIds.Contains(i.Id))
            .Select(i => i.Id)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<WishlistItem>())
        {
            if (entry.State == EntityState.Modified && !existingSet.Contains(entry.Entity.Id))
                entry.State = EntityState.Added;
        }
    }

    /// <summary>
    /// İlk favori əlavəsində yeni <see cref="Wishlist"/> kökü bəzən Modified qalır;
    /// bazada yoxdursa Added edilir.
    /// </summary>
    private async Task CoerceNewWishlistsToAddedStateAsync(CancellationToken ct)
    {
        var modifiedIds = _context.ChangeTracker
            .Entries<Wishlist>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity.Id)
            .Distinct()
            .ToList();

        if (modifiedIds.Count == 0)
            return;

        var existing = await _context.Wishlists
            .AsNoTracking()
            .Where(w => modifiedIds.Contains(w.Id))
            .Select(w => w.Id)
            .ToListAsync(ct);

        var existingSet = existing.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<Wishlist>())
        {
            if (entry.State == EntityState.Modified && !existingSet.Contains(entry.Entity.Id))
                entry.State = EntityState.Added;
        }
    }
}
