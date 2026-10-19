using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;

namespace TablesService.Infrastructure.Configuration;

public class DiningTableConfiguration : IEntityTypeConfiguration<DiningTable>
{
    public void Configure(EntityTypeBuilder<DiningTable> builder)
    {
        builder.ToTable("dining_tables");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityByDefaultColumn();
        builder.Property(t => t.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.Code).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.MinCapacity).HasDefaultValue((short)1);
        builder.Property(t => t.MaxCapacity).HasDefaultValue((short)4);
        builder.Property(t => t.IsCombinable).HasDefaultValue(false);
        builder.Property(t => t.Status).HasDefaultValue(TableStatus.Free);
        builder.Property(t => t.Position).HasDefaultValue((short)0);
        builder.Property(t => t.Notes).HasColumnType("text");
        builder.Property(t => t.IsActive).HasDefaultValue(true);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(t => new { t.BranchId, t.ZoneId, t.Position }).HasDatabaseName("ix_table_branch");
        builder.HasIndex(t => new { t.BranchId, t.Code }).IsUnique().HasDatabaseName("uq_table_code");

        builder.HasOne(t => t.Zone).WithMany(z => z.Tables).HasForeignKey(t => t.ZoneId);
    }
}