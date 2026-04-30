using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.DeliveryFee).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.CouponCode).HasMaxLength(64);

        b.OwnsOne(x => x.Address, a =>
        {
            a.Property(x => x.RecipientName).HasMaxLength(256).IsRequired();
            a.Property(x => x.RecipientPhone).HasMaxLength(64).IsRequired();
            a.Property(x => x.City).HasMaxLength(128).IsRequired();
            a.Property(x => x.District).HasMaxLength(128).IsRequired();
            a.Property(x => x.Line).HasMaxLength(500).IsRequired();
            a.Property(x => x.PostalCode).HasMaxLength(32);
        });

        b.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(o => o.Tracking)
            .WithOne()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(o => o.Tracking).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
