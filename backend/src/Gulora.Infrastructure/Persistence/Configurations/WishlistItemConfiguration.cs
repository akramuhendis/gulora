using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.WishlistId)
            .HasColumnName("wishlist_id")
            .IsRequired();

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        // Bir ürün bir istek listesinde yalnızca bir kez yer alabilir
        builder.HasIndex(i => new { i.WishlistId, i.ProductId })
            .IsUnique()
            .HasDatabaseName("ix_wishlist_items_wishlist_product");

        builder.Ignore(i => i.DomainEvents);
    }
}
