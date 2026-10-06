using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class PhoneOtpChallengeConfiguration : IEntityTypeConfiguration<PhoneOtpChallenge>
{
    public void Configure(EntityTypeBuilder<PhoneOtpChallenge> builder)
    {
        builder.ToTable("phone_otp_challenges");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.SentAt).HasColumnName("sent_at").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(e => e.PhoneNumber).HasDatabaseName("ix_phone_otp_challenges_phone_number");

        builder.Ignore(e => e.DomainEvents);
    }
}
