using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Comment).HasMaxLength(4000).IsRequired();
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.IsHidden).HasDefaultValue(false);
        b.HasIndex(x => x.ProductId);
        b.HasIndex(x => new { x.ProductId, x.UserId });
    }
}
