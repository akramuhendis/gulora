using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("wishlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id");

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(w => w.UserId)
            .IsUnique()
            .HasDatabaseName("ix_wishlists_user_id");

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(w => w.Items)
            .WithOne()
            .HasForeignKey(i => i.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(w => w.Count);
        builder.Ignore(w => w.DomainEvents);
    }
}
