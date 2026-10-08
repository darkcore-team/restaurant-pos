using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class AuthorityConfiguration : IEntityTypeConfiguration<Authority>
{
    public void Configure(EntityTypeBuilder<Authority> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(a => a.PublicId).IsUnique();
        builder.Property(a => a.Code).HasMaxLength(40).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();
        builder.Property(a => a.Name).HasMaxLength(80).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(255);
        builder.Property(a => a.Position).HasDefaultValueSql("0");
        builder.Property(a => a.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");
        builder.ToTable(t => t.HasCheckConstraint("ck_auth_code", "\"code\" ~ '^[A-Z][A-Z0-9_]*$'"));
    }
}