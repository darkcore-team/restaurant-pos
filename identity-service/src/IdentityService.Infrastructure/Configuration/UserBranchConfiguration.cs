using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class UserBranchConfiguration : IEntityTypeConfiguration<UserBranch>
{
    public void Configure(EntityTypeBuilder<UserBranch> builder)
    {
        builder.HasKey(ub => ub.Id);
        builder.HasOne(ub => ub.User)
            .WithMany()
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ub => ub.Branch)
            .WithMany(b => b.UserBranches)
            .HasForeignKey(ub => ub.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(ub => ub.IsPrimary).HasDefaultValueSql("false");
        builder.Property(ub => ub.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(ub => ub.UpdatedAt).HasDefaultValueSql("now()");
        
        builder.HasIndex(ub => new { ub.UserId, ub.BranchId })
            .IsUnique()
            .HasDatabaseName("uq_user_branch");
    }
}