using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class OptionGroupConfiguration : IEntityTypeConfiguration<OptionGroup>
{
    public void Configure(EntityTypeBuilder<OptionGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(g => g.PublicId).IsUnique();

        builder.Property(g => g.Name).HasMaxLength(120).IsRequired();
        builder.Property(g => g.MinSelect).HasDefaultValue((short)1);
        builder.Property(g => g.MaxSelect).HasDefaultValue((short)1);
        builder.Property(g => g.IsRequired).HasDefaultValueSql("false");
        builder.Property(g => g.Position).HasDefaultValueSql("0");
        builder.Property(g => g.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(g => g.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(g => g.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(g => new { g.Id, g.RestaurantId }).IsUnique();

        builder.ToTable(t => t.HasCheckConstraint("ck_og_select",
            "\"min_select\" >= 0 AND \"max_select\" >= 1 AND \"min_select\" <= \"max_select\""));
    }
}
