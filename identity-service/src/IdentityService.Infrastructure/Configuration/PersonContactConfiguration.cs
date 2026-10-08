using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class PersonContactConfiguration : IEntityTypeConfiguration<PersonContact>
{
    public void Configure(EntityTypeBuilder<PersonContact> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.HasOne<Person>()
            .WithMany()
            .HasForeignKey(c => c.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(c => c.ContactType).HasMaxLength(10).IsRequired();
        builder.Property(c => c.ContactValue).HasMaxLength(255).IsRequired();
        builder.Property(c => c.IsPrimary).HasDefaultValueSql("false");
        builder.Property(c => c.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(c => c.VerifiedAt);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(c => new { c.PersonId, c.ContactType, c.ContactValue })
            .IsUnique()
            .HasDatabaseName("uq_contact_value");
        builder.ToTable(t => t.HasCheckConstraint("ck_contact_type",
            "\"contact_type\" IN ('EMAIL','PHONE','ADDRESS')"));
    }
}