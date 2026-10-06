using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class SellerApplicationConfiguration : IEntityTypeConfiguration<SellerApplication>
{
    public void Configure(EntityTypeBuilder<SellerApplication> builder)
    {
        builder.ToTable("seller_applications");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Company).HasColumnName("company").HasMaxLength(300).IsRequired();
        builder.Property(e => e.TaxNumber).HasColumnName("tax_number").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(30).IsRequired();
        builder.Property(e => e.City).HasColumnName("city").HasMaxLength(120).IsRequired();
        builder.Property(e => e.Category).HasColumnName("category").HasMaxLength(150).IsRequired();
        builder.Property(e => e.Message).HasColumnName("message").HasColumnType("text");
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(1000);

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.TaxNumber).HasDatabaseName("ix_seller_applications_tax_number");
        builder.HasIndex(e => e.Email).HasDatabaseName("ix_seller_applications_email");

        builder.Ignore(e => e.DomainEvents);
    }
}
