using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(u => u.PublicId).IsUnique();
        builder.HasOne(u => u.Person)
            .WithOne()
            .HasForeignKey<User>(u => u.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(u => u.PersonId).IsUnique();
        builder.Property(u => u.Username).HasMaxLength(60).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.MustChangePassword).HasDefaultValueSql("true");
        builder.Property(u => u.FailedCount).HasDefaultValueSql("0");
        builder.Property(u => u.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
    }
}