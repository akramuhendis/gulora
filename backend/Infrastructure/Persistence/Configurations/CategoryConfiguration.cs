using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Icon).HasMaxLength(128);
        b.Property(x => x.Color).HasMaxLength(32);
        b.Property(x => x.ImageUrl).HasMaxLength(2048);
    }
}
