using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class OptionGroupRepository(CatalogServiceDbContext db) : IOptionGroupRepository
{
    public async Task<OptionGroup?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await db.OptionGroups.AsNoTracking().FirstOrDefaultAsync(og => og.Id == id && og.Status == RecordStatus.Active, cancellationToken);

    public async Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.OptionGroups.AnyAsync(og => og.Id == id && og.RestaurantId == restaurantId && og.Status == RecordStatus.Active, cancellationToken);

    public async Task<int> CountExistingAsync(IEnumerable<long> ids, long restaurantId, CancellationToken cancellationToken = default)
        => await db.OptionGroups.CountAsync(og => ids.Contains(og.Id) && og.RestaurantId == restaurantId && og.Status == RecordStatus.Active, cancellationToken);
}