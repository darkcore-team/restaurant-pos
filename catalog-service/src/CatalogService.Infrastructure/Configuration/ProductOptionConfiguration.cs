using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(o => o.PublicId).IsUnique();

        builder.Property(o => o.Name).HasMaxLength(120).IsRequired();
        builder.Property(o => o.NameEn).HasMaxLength(120);
        builder.Property(o => o.PriceDelta).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(o => o.IsDefault).HasDefaultValueSql("false");
        builder.Property(o => o.Position).HasDefaultValueSql("0");
        builder.Property(o => o.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(o => o.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(o => o.OptionGroup)
            .WithMany(g => g.Options)
            .HasForeignKey(o => o.OptionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.OptionGroupId, o.Position });
        builder.HasIndex(o => new { o.Id, o.RestaurantId }).IsUnique();

        builder.ToTable(t => t.HasCheckConstraint("ck_po_delta", "\"price_delta\" >= 0"));
    }
}
