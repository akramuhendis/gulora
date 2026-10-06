using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class SellerShopConfiguration : IEntityTypeConfiguration<SellerShop>
{
    public void Configure(EntityTypeBuilder<SellerShop> builder)
    {
        builder.ToTable("seller_shops");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasDatabaseName("ix_seller_shops_user_id");

        builder.Property(s => s.StoreName)
            .HasColumnName("store_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.StoreSlug)
            .HasColumnName("store_slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(s => s.StoreSlug)
            .IsUnique()
            .HasDatabaseName("ix_seller_shops_store_slug");

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(s => s.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(2000);

        builder.Property(s => s.BannerUrl)
            .HasColumnName("banner_url")
            .HasMaxLength(2000);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<SellerShop>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}
