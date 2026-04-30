using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.HasKey(p => p.Id);
        b.Ignore(p => p.Images);
        b.Ignore(p => p.Tags);
        b.Property<List<string>>("_images").HasColumnName("Images").HasColumnType("jsonb");
        b.Property<List<string>>("_tags").HasColumnName("Tags").HasColumnType("jsonb");

        b.Property(p => p.Name).HasMaxLength(200).IsRequired();
        b.Property(p => p.Slug).HasMaxLength(200).IsRequired();
        b.HasIndex(p => p.Slug).IsUnique();
        b.Property(p => p.Price).HasPrecision(18, 2);
        b.Property(p => p.OriginalPrice).HasPrecision(18, 2);
        b.Property(p => p.Rating).HasPrecision(9, 2);
        b.Property(p => p.ImageUrl).HasMaxLength(2048).IsRequired();
        b.Property(p => p.Badge).HasMaxLength(120);
        b.Property(p => p.Description).HasMaxLength(8000);

        b.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
