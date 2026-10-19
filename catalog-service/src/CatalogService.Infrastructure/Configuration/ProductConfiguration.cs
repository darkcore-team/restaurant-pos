using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(p => p.PublicId).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.NameEn).HasMaxLength(150);
        builder.Property(p => p.BasePrice).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.IsAvailable).HasDefaultValueSql("true");
        builder.Property(p => p.IsTaxExempt).HasDefaultValueSql("false");
        builder.Property(p => p.Position).HasDefaultValueSql("0");
        builder.Property(p => p.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Station)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tax)
            .WithMany()
            .HasForeignKey(p => p.TaxId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => new { p.BranchId, p.RestaurantId, p.Status, p.CategoryId, p.Position });
        builder.HasIndex(p => new { p.Id, p.RestaurantId }).IsUnique();

        builder.ToTable(t => t.HasCheckConstraint("ck_product_price", "\"base_price\" >= 0"));
    }
}
