using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class ProductRepository(CatalogServiceDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
        => await db.Products
            .Include(p => p.OptionGroups)
            .FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurantId && p.Status == RecordStatus.Active, ct);

    public async Task CreateAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Update(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> SoftDeleteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurantId && p.Status == RecordStatus.Active, ct);
        if (product is null) return false;
        product.Status = RecordStatus.Inactive;
        product.DeletedAt = DateTimeOffset.UtcNow;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        product.UpdatedBy = actorId;
        await db.SaveChangesAsync(ct);
        return true;
    }
}