using Gulora.Domain.Entities;
using Gulora.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(o => o.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("ix_orders_order_number");

        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("ix_orders_user_id");

        // Admin panelinde durum bazlı filtreleme (GetByStatusAsync) için
        builder.HasIndex(o => o.Status)
            .HasDatabaseName("ix_orders_status");

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.DeliveryType)
            .HasColumnName("delivery_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasColumnName("payment_status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.SubTotal)
            .HasColumnName("sub_total")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(o => o.DeliveryFee)
            .HasColumnName("delivery_fee")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(o => o.DiscountAmount)
            .HasColumnName("discount_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasColumnName("total_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(o => o.CouponCode)
            .HasColumnName("coupon_code")
            .HasMaxLength(50);

        builder.Property(o => o.GiftCardMessage)
            .HasColumnName("gift_card_message")
            .HasMaxLength(2000);

        builder.Property(o => o.GiftCardSenderName)
            .HasColumnName("gift_card_sender_name")
            .HasMaxLength(120);

        builder.Property(o => o.GiftCardOccasion)
            .HasColumnName("gift_card_occasion")
            .HasMaxLength(80);

        // AddressSnapshot — JSON olarak tek sütunda saklanır
        builder.Property(o => o.AddressSnapshot)
            .HasColumnName("address_snapshot")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<AddressSnapshot>(v, JsonSerializerOptions.Default)!)
            .IsRequired();

        builder.Property(o => o.TrackingNumber)
            .HasColumnName("tracking_number")
            .HasMaxLength(100);

        builder.Property(o => o.CargoCompany)
            .HasColumnName("cargo_company")
            .HasMaxLength(100);

        builder.Property(o => o.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(o => o.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        // Navigation
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Trackings)
            .WithOne()
            .HasForeignKey(t => t.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Read-only nav + private list: field access ensures yeni kəlmələr düzgün Added olur.
        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Trackings).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(o => o.DomainEvents);
    }
}
