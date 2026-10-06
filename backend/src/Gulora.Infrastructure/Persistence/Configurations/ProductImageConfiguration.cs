using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(i => i.Url)
            .HasColumnName("url")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(300);

        builder.Property(i => i.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(i => i.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(i => i.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(i => new { i.ProductId, i.DisplayOrder })
            .HasDatabaseName("ix_product_images_product_id_display_order");

        builder.Ignore(i => i.DomainEvents);
    }
}
