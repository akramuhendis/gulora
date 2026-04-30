using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.HasKey(x => x.Id);
        b.Ignore(x => x.Addons);
        b.Property<List<string>>("_addons").HasColumnName("Addons").HasColumnType("jsonb");

        b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ProductImageUrl).HasMaxLength(2048).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.VariantLabel).HasMaxLength(200);
        b.HasIndex(x => x.OrderId);
        b.HasIndex(x => x.ProductId);
    }
}
