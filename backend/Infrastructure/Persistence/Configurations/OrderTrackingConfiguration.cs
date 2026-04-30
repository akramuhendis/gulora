using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class OrderTrackingConfiguration : IEntityTypeConfiguration<OrderTracking>
{
    public void Configure(EntityTypeBuilder<OrderTracking> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => x.OrderId);
    }
}
