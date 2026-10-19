using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Infrastructure.Configuration;

public sealed class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(l => l.PublicId).IsUnique();
        builder.Property(l => l.Code).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
        builder.Property(l => l.Name).HasMaxLength(80).IsRequired();
        builder.Property(l => l.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(l => l.UpdatedAt).HasDefaultValueSql("now()");
    }
}

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Symbol).HasMaxLength(8).IsRequired();
        builder.Property(c => c.DecimalPlaces).HasDefaultValue((short)2);
        builder.Property(c => c.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");
    }
}

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.CodeAlpha3).HasMaxLength(3).IsFixedLength();
        builder.HasIndex(c => c.CodeAlpha3).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PhoneCode).HasMaxLength(8);
        builder.Property(c => c.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(c => c.DefaultCurrency)
            .WithMany()
            .HasForeignKey(c => c.DefaultCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(r => r.PublicId).IsUnique();
        builder.Property(r => r.Code).HasMaxLength(12).IsRequired();
        builder.Property(r => r.Name).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(r => r.Country)
            .WithMany()
            .HasForeignKey(r => r.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.CountryId, r.Code }).IsUnique();
    }
}

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(c => c.PublicId).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(12);
        builder.Property(c => c.Name).HasMaxLength(120).IsRequired();
        builder.Property(c => c.IsCapital).HasDefaultValueSql("false");
        builder.Property(c => c.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(c => c.Country)
            .WithMany()
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Region)
            .WithMany()
            .HasForeignKey(c => c.RegionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.CountryId, c.Name }).IsUnique();
    }
}

public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(d => d.PublicId).IsUnique();
        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(120).IsRequired();
        builder.Property(d => d.RequiresExpiryDate).HasDefaultValueSql("false");
        builder.Property(d => d.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(d => d.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(d => d.Country)
            .WithMany()
            .HasForeignKey(d => d.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.CountryId, d.Code }).IsUnique();
    }
}

public sealed class TaxConfiguration : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(t => t.PublicId).IsUnique();
        builder.Property(t => t.Code).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(80).IsRequired();
        builder.Property(t => t.Rate).HasPrecision(5, 4).IsRequired();
        builder.Property(t => t.IsInclusive).HasDefaultValueSql("false");
        builder.Property(t => t.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasOne(t => t.Country)
            .WithMany()
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.CountryId, t.Code }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("ck_tax_rate", "\"rate\" >= 0 AND \"rate\" <= 1"));
    }
}

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(m => m.PublicId).IsUnique();
        builder.Property(m => m.Code).HasMaxLength(30).IsRequired();
        builder.HasIndex(m => m.Code).IsUnique();
        builder.Property(m => m.Name).HasMaxLength(80).IsRequired();
        builder.Property(m => m.RequiresReference).HasDefaultValueSql("false");
        builder.Property(m => m.RequiresAmountTendered).HasDefaultValueSql("false");
        builder.Property(m => m.RequiresCashSession).HasDefaultValueSql("false");
        builder.Property(m => m.Position).HasDefaultValueSql("0");
        builder.Property(m => m.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.UpdatedAt).HasDefaultValueSql("now()");
    }
}
