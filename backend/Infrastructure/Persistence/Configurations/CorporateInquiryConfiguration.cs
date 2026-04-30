using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

public sealed class CorporateInquiryConfiguration : IEntityTypeConfiguration<CorporateInquiry>
{
    public void Configure(EntityTypeBuilder<CorporateInquiry> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Company).HasMaxLength(300).IsRequired();
        b.Property(x => x.ContactName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(320).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(64).IsRequired();
        b.Property(x => x.Occasion).HasMaxLength(200);
        b.Property(x => x.Budget).HasPrecision(18, 2);
        b.Property(x => x.Message).HasMaxLength(4000).IsRequired();
    }
}
