using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class DiscountCouponConfiguration : IEntityTypeConfiguration<DiscountCoupon>
{
    public void Configure(EntityTypeBuilder<DiscountCoupon> builder)
    {
        builder.ToTable("discount_coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("ix_discount_coupons_code");

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Value)
            .HasColumnName("value")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(c => c.MinOrderAmount)
            .HasColumnName("min_order_amount")
            .HasColumnType("numeric(18,2)");

        builder.Property(c => c.MaxDiscountAmount)
            .HasColumnName("max_discount_amount")
            .HasColumnType("numeric(18,2)");

        builder.Property(c => c.UsageLimit)
            .HasColumnName("usage_limit");

        builder.Property(c => c.UsageCount)
            .HasColumnName("usage_count")
            .IsRequired();

        builder.Property(c => c.StartsAt)
            .HasColumnName("starts_at");

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.RestrictedUserId)
            .HasColumnName("restricted_user_id");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasQueryFilter(c => c.IsActive);

        builder.Ignore(c => c.DomainEvents);
    }
}
