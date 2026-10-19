using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;

namespace TablesService.Infrastructure.Configuration;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ToTable("zones");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Id).UseIdentityByDefaultColumn();
        builder.Property(z => z.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(z => z.Name).HasMaxLength(100).IsRequired();
        builder.Property(z => z.Position).HasDefaultValue((short)0);
        builder.Property(z => z.IsActive).HasDefaultValue(true);
        builder.Property(z => z.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(z => z.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(z => new { z.BranchId, z.RestaurantId, z.Position }).HasDatabaseName("ix_zone_branch_pos");

        builder.HasMany(z => z.Tables).WithOne(t => t.Zone).HasForeignKey(t => t.ZoneId);
    }
}