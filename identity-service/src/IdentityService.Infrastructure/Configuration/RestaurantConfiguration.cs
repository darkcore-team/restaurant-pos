using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configuration;

public sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PublicId).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(r => r.PublicId).IsUnique();
        builder.Property(r => r.Name).HasMaxLength(150).IsRequired();
        builder.Property(r => r.TaxId).HasMaxLength(40);
        builder.HasIndex(r => r.TaxId).IsUnique();
        builder.Property(r => r.Timezone).HasMaxLength(64).HasDefaultValueSql("'America/Managua'");
        builder.Property(r => r.DefaultLanguageId).IsRequired();
        builder.Property(r => r.DefaultCurrencyId).IsRequired();
        builder.Property(r => r.DefaultCountryId);
        builder.Property(r => r.BaseCurrencyId).IsRequired();
        builder.Property(r => r.Settings).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(r => r.Status).HasDefaultValueSql("'ACTIVE'");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
    }
}