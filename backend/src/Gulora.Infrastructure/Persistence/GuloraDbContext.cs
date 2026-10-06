using Gulora.Domain.Entities;
using Gulora.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Gulora.Infrastructure.Persistence;

/// <summary>
/// Gülora ana DbContext'i — tüm entity kümeleri burada tanımlanır.
/// EF Core yapılandırması Fluent API ile Configurations/ klasöründe yapılır;
/// Domain entity'leri üzerinde hiçbir EF attribute kullanılmaz (gulora-csharp-backend.mdc).
/// </summary>
public sealed class GuloraDbContext : DbContext
{
    public GuloraDbContext(DbContextOptions<GuloraDbContext> options) : base(options) { }

    // ── Entity DbSet'leri ──────────────────────────────────────────────────────

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<DiscountCoupon> DiscountCoupons => Set<DiscountCoupon>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    public DbSet<PhoneOtpChallenge> PhoneOtpChallenges => Set<PhoneOtpChallenge>();
    public DbSet<SellerApplication> SellerApplications => Set<SellerApplication>();
    public DbSet<SellerShop> SellerShops => Set<SellerShop>();
    public DbSet<SellerCampaignPlan> SellerCampaignPlans => Set<SellerCampaignPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tüm IEntityTypeConfiguration<T> uygulamalarını bu assembly'den otomatik yükle
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GuloraDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
