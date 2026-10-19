using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(s => s.PublicId).IsUnique();

        builder.Property(s => s.Code).HasMaxLength(30).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(80).IsRequired();
        builder.Property(s => s.Color).HasMaxLength(9);
        builder.Property(s => s.Position).HasDefaultValueSql("0");
        builder.Property(s => s.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(s => new { s.RestaurantId, s.Code }).IsUnique();
        builder.HasIndex(s => new { s.Id, s.RestaurantId }).IsUnique();
    }
}