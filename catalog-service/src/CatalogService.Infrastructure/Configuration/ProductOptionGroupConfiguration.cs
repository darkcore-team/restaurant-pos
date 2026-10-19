using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class ProductOptionGroupConfiguration : IEntityTypeConfiguration<ProductOptionGroup>
{
    public void Configure(EntityTypeBuilder<ProductOptionGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(x => x.PublicId).IsUnique();

        builder.Property(x => x.Position).HasDefaultValueSql("0");
        builder.Property(x => x.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(x => x.Product)
            .WithMany(p => p.OptionGroups)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.OptionGroup)
            .WithMany(g => g.ProductOptionGroups)
            .HasForeignKey(x => x.OptionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ProductId, x.OptionGroupId }).IsUnique();
    }
}
