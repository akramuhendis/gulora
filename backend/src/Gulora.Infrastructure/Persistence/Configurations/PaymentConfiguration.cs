using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Method)
            .HasColumnName("method")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasColumnName("currency")
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(p => p.ProviderTransactionId)
            .HasColumnName("provider_transaction_id")
            .HasMaxLength(200);

        builder.Property(p => p.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(p => p.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName("ix_payments_idempotency_key");

        builder.Property(p => p.PaidAt)
            .HasColumnName("paid_at");

        builder.Property(p => p.RefundedAt)
            .HasColumnName("refunded_at");

        builder.Property(p => p.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("ix_payments_order_id");

        builder.Ignore(p => p.DomainEvents);
    }
}
