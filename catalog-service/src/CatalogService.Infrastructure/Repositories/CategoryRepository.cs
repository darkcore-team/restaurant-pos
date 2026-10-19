using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class CategoryRepository(CatalogServiceDbContext db) : ICategoryRepository
{
    public async Task<bool> ExistsAsync(long id, long restaurantId, long branchId, CancellationToken cancellationToken = default)
        => await db.Categories.AnyAsync(c => c.Id == id && c.RestaurantId == restaurantId && c.BranchId == branchId && c.Status == RecordStatus.Active, cancellationToken);
}