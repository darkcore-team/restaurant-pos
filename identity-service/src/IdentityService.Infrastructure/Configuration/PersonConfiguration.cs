using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class PersonConfiguration: IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(p => p.PublicId).IsUnique();
        builder.Property(p => p.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Gender).HasMaxLength(20);
        builder.Property(p => p.DocumentNumber).HasMaxLength(50);
        builder.Property(p => p.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(p => new { p.LastName, p.FirstName }).HasDatabaseName("ix_people_name");
        builder.HasIndex(p => new { p.DocumentTypeId, p.DocumentNumber })
            .IsUnique()
            .HasDatabaseName("uq_people_document")
            .HasFilter("\"document_number\" IS NOT NULL");
        builder.ToTable(t => t.HasCheckConstraint("ck_person_doc",
            "(\"document_type_id\" IS NULL) = (\"document_number\" IS NULL)"));
    }
}