using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RestaurantPos.Shared.Persistence;

namespace CatalogService.Infrastructure.Data;

public sealed class CatalogServiceDbContext(DbContextOptions options) : TenantDbContext(options)
{
    protected override string Schema => "catalog";

    // Business
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<OptionGroup> OptionGroups => Set<OptionGroup>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionGroup> ProductOptionGroups => Set<ProductOptionGroup>();

    // Lookup
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogServiceDbContext).Assembly);
    }
}