using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;

namespace TablesService.Infrastructure.Configuration;

public class TableAssignmentConfiguration : IEntityTypeConfiguration<TableAssignment>
{
    public void Configure(EntityTypeBuilder<TableAssignment> builder)
    {
        builder.ToTable("table_assignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityByDefaultColumn();
        builder.Property(a => a.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.AssignedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(a => a.Table).WithMany(t => t.Assignments).HasForeignKey(a => a.TableId);
    }
}