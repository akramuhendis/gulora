using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class OrderTrackingConfiguration : IEntityTypeConfiguration<OrderTracking>
{
    public void Configure(EntityTypeBuilder<OrderTracking> builder)
    {
        builder.ToTable("order_trackings");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Note)
            .HasColumnName("note")
            .HasMaxLength(1000);

        builder.Property(t => t.CargoCompany)
            .HasColumnName("cargo_company")
            .HasMaxLength(100);

        builder.Property(t => t.TrackingNumber)
            .HasColumnName("tracking_number")
            .HasMaxLength(100);

        builder.Property(t => t.ChangedByUserId)
            .HasColumnName("changed_by_user_id");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(t => t.OrderId)
            .HasDatabaseName("ix_order_trackings_order_id");

        builder.Ignore(t => t.DomainEvents);
    }
}
