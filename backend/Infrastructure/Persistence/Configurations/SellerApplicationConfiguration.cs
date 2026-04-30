using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class SellerApplicationConfiguration : IEntityTypeConfiguration<SellerApplication>
{
    public void Configure(EntityTypeBuilder<SellerApplication> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Company).HasMaxLength(300).IsRequired();
        b.Property(x => x.TaxNumber).HasMaxLength(64).IsRequired();
        b.Property(x => x.Email).HasMaxLength(320).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(64).IsRequired();
        b.Property(x => x.City).HasMaxLength(128).IsRequired();
        b.Property(x => x.Category).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        b.Property(x => x.RejectionReason).HasMaxLength(2000);
    }
}
