using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(120).IsRequired();
        b.Property(x => x.RecipientName).HasMaxLength(256).IsRequired();
        b.Property(x => x.RecipientPhone).HasMaxLength(64).IsRequired();
        b.Property(x => x.City).HasMaxLength(128).IsRequired();
        b.Property(x => x.District).HasMaxLength(128).IsRequired();
        b.Property(x => x.Line).HasMaxLength(500).IsRequired();
        b.Property(x => x.PostalCode).HasMaxLength(32);
        b.HasIndex(x => x.UserId);
    }
}
