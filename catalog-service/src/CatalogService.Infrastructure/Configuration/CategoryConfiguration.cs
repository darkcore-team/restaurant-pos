using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(c => c.PublicId).IsUnique();

        builder.Property(c => c.Name).HasMaxLength(120).IsRequired();
        builder.Property(c => c.NameEn).HasMaxLength(120);
        builder.Property(c => c.Position).HasDefaultValueSql("0");
        builder.Property(c => c.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(c => new { c.BranchId, c.RestaurantId, c.Position });
        builder.HasIndex(c => new { c.Id, c.RestaurantId }).IsUnique();
    }
}
