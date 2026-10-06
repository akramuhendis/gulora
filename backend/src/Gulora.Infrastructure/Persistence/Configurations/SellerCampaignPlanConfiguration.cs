using Gulora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gulora.Infrastructure.Persistence.Configurations;

internal sealed class SellerCampaignPlanConfiguration : IEntityTypeConfiguration<SellerCampaignPlan>
{
    public void Configure(EntityTypeBuilder<SellerCampaignPlan> builder)
    {
        builder.ToTable("seller_campaign_plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.SellerUserId)
            .HasColumnName("seller_user_id")
            .IsRequired();

        builder.HasIndex(x => x.SellerUserId)
            .HasDatabaseName("ix_seller_campaign_plans_seller_user_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Percent)
            .HasColumnName("percent")
            .IsRequired();

        builder.Property(x => x.StartsAtUtc)
            .HasColumnName("starts_at_utc")
            .IsRequired();

        builder.Property(x => x.EndsAtUtc)
            .HasColumnName("ends_at_utc")
            .IsRequired();

        builder.Property(x => x.MinCartAmount)
            .HasColumnName("min_cart_amount")
            .HasPrecision(18, 2);

        builder.Property(x => x.ScopeJson)
            .HasColumnName("scope_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.SellerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);
    }
}
