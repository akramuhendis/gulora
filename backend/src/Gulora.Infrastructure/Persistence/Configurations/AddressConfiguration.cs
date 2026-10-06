using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.Title)
            .HasColumnName("title")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.RecipientName)
            .HasColumnName("recipient_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(a => a.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.District)
            .HasColumnName("district")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.FullLine)
            .HasColumnName("full_line")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(20);

        builder.Property(a => a.CountryCode)
            .HasColumnName("country_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(a => a.IsDefault)
            .HasColumnName("is_default")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(a => new { a.UserId, a.IsDefault })
            .HasDatabaseName("ix_addresses_user_id_is_default");

        builder.Ignore(a => a.DomainEvents);
    }
}
