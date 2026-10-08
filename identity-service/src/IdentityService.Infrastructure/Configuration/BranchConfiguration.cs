using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(b => b.PublicId).IsUnique();
        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(b => b.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(b => b.CityId).IsRequired();
        builder.Property(b => b.Code).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Name).HasMaxLength(120).IsRequired();
        builder.Property(b => b.Phone).HasMaxLength(32);
        builder.Property(b => b.IsDefault).HasDefaultValueSql("false");
        builder.Property(b => b.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(b => b.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(b => new { b.RestaurantId, b.Code }).IsUnique();
        builder.HasIndex(b => new { b.Id, b.RestaurantId }).IsUnique();
        builder.HasIndex(b => b.RestaurantId).IsUnique().HasFilter("\"is_default\"");
        builder.ToTable(t => t.HasCheckConstraint("ck_branch_hours",
            "\"closes_at\" IS NULL OR \"opens_at\" IS NULL OR \"closes_at\" > \"opens_at\""));
    }
}