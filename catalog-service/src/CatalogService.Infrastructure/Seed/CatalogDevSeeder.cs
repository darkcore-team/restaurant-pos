using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Seed;

public sealed class CatalogDevSeeder(CatalogServiceDbContext db)
{
    private const long RestaurantId = 1;
    private const long BranchId = 1;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedLanguagesAsync(ct);
        await SeedCurrenciesAsync(ct);
        await SeedPaymentMethodsAsync(ct);
        await SeedStationsAsync(ct);
        await SeedCatalogAsync(ct);
    }

    private async Task SeedLanguagesAsync(CancellationToken ct)
    {
        if (await db.Languages.AnyAsync(ct)) return;
        db.Languages.AddRange(
            new Language { Code = "es", Name = "Español", Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new Language { Code = "en", Name = "English", Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCurrenciesAsync(CancellationToken ct)
    {
        if (await db.Currencies.AnyAsync(ct)) return;
        db.Currencies.AddRange(
            new Currency() { Code = "NIO", Name = "Córdoba", Symbol = "C$", DecimalPlaces = 2, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new Currency { Code = "USD", Name = "Dólar", Symbol = "$", DecimalPlaces = 2, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
            );
        await db.SaveChangesAsync(ct);
    }
    
    private async Task SeedPaymentMethodsAsync(CancellationToken ct)
    {
        if (await db.PaymentMethods.AnyAsync(ct)) return;

        db.PaymentMethods.AddRange(
            new PaymentMethod { Code = "CASH", Name = "Efectivo", RequiresReference = false, RequiresAmountTendered = true, RequiresCashSession = true, Position = 1, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new PaymentMethod { Code = "CARD", Name = "Tarjeta", RequiresReference = true, RequiresAmountTendered = false, RequiresCashSession = true, Position = 2, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new PaymentMethod { Code = "TRANSFER", Name = "Transferencia", RequiresReference = true, RequiresAmountTendered = false, RequiresCashSession = false, Position = 3, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }
    
    private async Task SeedStationsAsync(CancellationToken ct)
    {
        if (await db.Stations.AnyAsync(s => s.RestaurantId == RestaurantId, ct)) return;

        db.Stations.AddRange(
            new Station { RestaurantId = RestaurantId, Code = "COCINA", Name = "Cocina", Color = "#ef4444", Position = 1, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new Station { RestaurantId = RestaurantId, Code = "BAR", Name = "Bar", Color = "#3b82f6", Position = 2, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedCatalogAsync(CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(c => c.RestaurantId == RestaurantId && c.BranchId == BranchId, ct)) return;

        var cocina = await db.Stations.SingleAsync(s => s.RestaurantId == RestaurantId && s.Code == "COCINA", ct);
        var bar = await db.Stations.SingleAsync(s => s.RestaurantId == RestaurantId && s.Code == "BAR", ct);

        var sizeGroup = new OptionGroup
        {
            RestaurantId = RestaurantId,
            Name = "Tamaño",
            MinSelect = 1,
            MaxSelect = 1,
            IsRequired = true,
            Position = 1,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        sizeGroup.Options.Add(new ProductOption { RestaurantId = RestaurantId, Name = "Pequeño", PriceDelta = 0m, IsDefault = true, Position = 1, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        sizeGroup.Options.Add(new ProductOption { RestaurantId = RestaurantId, Name = "Grande", PriceDelta = 2.50m, IsDefault = false, Position = 2, Status = RecordStatus.Active, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow });
        db.OptionGroups.Add(sizeGroup);
        await db.SaveChangesAsync(ct);

        var comidas = new Category
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Name = "Comidas",
            Position = 1,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var bebidas = new Category
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Name = "Bebidas",
            Position = 2,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var burger = new Product
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Category = comidas,
            StationId = cocina.Id,
            Name = "Hamburguesa Clásica",
            Description = "Carne, queso, lechuga y tomate",
            BasePrice = 8.50m,
            IsAvailable = true,
            Position = 1,
            IsTaxExempt = false,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        burger.OptionGroups.Add(new ProductOptionGroup
        {
            RestaurantId = RestaurantId,
            Product = burger,
            OptionGroup = sizeGroup,
            MinSelect = 1,
            MaxSelect = 1,
            Position = 1,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        var soda = new Product
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Category = bebidas,
            StationId = bar.Id,
            Name = "Coca Cola",
            BasePrice = 1.50m,
            IsAvailable = true,
            Position = 1,
            IsTaxExempt = false,
            Status = RecordStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        comidas.Products.Add(burger);
        bebidas.Products.Add(soda);
        db.Categories.AddRange(comidas, bebidas);
        await db.SaveChangesAsync(ct);
    }
}