using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        builder.ToTable("login_attempts");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.PublicId)
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();
        builder.HasIndex(l => l.PublicId)
            .IsUnique();
        builder.Property(l => l.UsernameTry)
            .HasMaxLength(60)
            .IsRequired();
        builder.Property(l => l.WasSuccessful).IsRequired();
        builder.Property(l => l.FailureReason).HasMaxLength(60);
        builder.Property(l => l.UserAgent).HasMaxLength(255);
        builder.Property(l => l.Ip).HasColumnType("inet");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
        builder.HasIndex(l => new { l.UserId, l.CreatedAt }, "ix_login_attempts_user")
            .IsDescending(false, true);
    }
}