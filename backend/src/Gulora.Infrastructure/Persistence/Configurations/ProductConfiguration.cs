using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasMaxLength(350)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("ix_products_slug");

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(p => p.ShortDescription)
            .HasColumnName("short_description")
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(p => p.DiscountedPrice)
            .HasColumnName("discounted_price")
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired();

        builder.Property(p => p.Sku)
            .HasColumnName("sku")
            .HasMaxLength(100);

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasFilter("sku IS NOT NULL")
            .HasDatabaseName("ix_products_sku");

        builder.Property(p => p.Brand)
            .HasColumnName("brand")
            .HasMaxLength(150);

        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(p => p.SellerId)
            .HasColumnName("seller_id");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.IsFeatured)
            .HasColumnName("is_featured")
            .IsRequired();

        builder.Property(p => p.AverageRating)
            .HasColumnName("average_rating")
            .HasColumnType("double precision")
            .IsRequired();

        builder.Property(p => p.ReviewCount)
            .HasColumnName("review_count")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Soft delete global query filter
        builder.HasQueryFilter(p => p.IsActive);

        // Navigation
        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Əlavə şəkil _images sahəsinə yazılmalıdır; əks halda EF bəzən yeni ProductImage-i Modified kimi izləyir.
        builder.Navigation(p => p.Images).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Reviews)
            .WithOne()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("ix_products_category_id");

        builder.Ignore(p => p.PrimaryImageUrl);
        builder.Ignore(p => p.EffectivePrice);
        builder.Ignore(p => p.IsAvailableForPurchase);
        builder.Ignore(p => p.DomainEvents);
    }
}
