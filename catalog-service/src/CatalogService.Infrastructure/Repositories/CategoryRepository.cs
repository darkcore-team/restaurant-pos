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

    public async Task<IReadOnlyList<Category>> GetByBranchAsync(long restaurantId, long branchId, CancellationToken cancellationToken = default)
        => await db.Categories
            .AsNoTracking()
            .Where(c => c.RestaurantId == restaurantId && c.BranchId == branchId && c.Status == RecordStatus.Active)
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId && c.Status == RecordStatus.Active, cancellationToken);

    public async Task CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        db.Categories.Update(category);
        await db.SaveChangesAsync(cancellationToken);
    }
}
