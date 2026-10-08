using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class UserAuthorityConfiguration : IEntityTypeConfiguration<UserAuthority>
{
    public void Configure(EntityTypeBuilder<UserAuthority> builder)
    {
        builder.HasKey(ua => ua.Id);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Authority>()
            .WithMany()
            .HasForeignKey(ua => ua.AuthorityId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ua => ua.GrantedBy)
            .OnDelete(DeleteBehavior.NoAction);
        builder.Property(ua => ua.GrantedAt)
            .HasDefaultValueSql("now()");
        builder.Property(ua => ua.CreatedAt)
            .HasDefaultValueSql("now()");
        builder.Property(ua => ua.UpdatedAt)
            .HasDefaultValueSql("now()");
        builder.HasIndex(ua => new { ua.UserId, ua.AuthorityId })
            .IsUnique()
            .HasDatabaseName("uq_user_authority");
    }
}