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

    public async Task<OptionGroup?> GetByIdWithOptionsAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.OptionGroups
            .Include(og => og.Options.Where(o => o.Status == RecordStatus.Active))
            .FirstOrDefaultAsync(og => og.Id == id && og.RestaurantId == restaurantId && og.Status == RecordStatus.Active, cancellationToken);

    public async Task<IReadOnlyList<OptionGroup>> GetByRestaurantAsync(long restaurantId, CancellationToken cancellationToken = default)
        => await db.OptionGroups
            .AsNoTracking()
            .Include(og => og.Options.Where(o => o.Status == RecordStatus.Active))
            .Where(og => og.RestaurantId == restaurantId && og.Status == RecordStatus.Active)
            .OrderBy(og => og.Position)
            .ThenBy(og => og.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(long id, long restaurantId, CancellationToken cancellationToken = default)
        => await db.OptionGroups.AnyAsync(og => og.Id == id && og.RestaurantId == restaurantId && og.Status == RecordStatus.Active, cancellationToken);

    public async Task<int> CountExistingAsync(IEnumerable<long> ids, long restaurantId, CancellationToken cancellationToken = default)
        => await db.OptionGroups.CountAsync(og => ids.Contains(og.Id) && og.RestaurantId == restaurantId && og.Status == RecordStatus.Active, cancellationToken);

    public async Task CreateAsync(OptionGroup group, CancellationToken cancellationToken = default)
    {
        db.OptionGroups.Add(group);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OptionGroup group, CancellationToken cancellationToken = default)
    {
        db.OptionGroups.Update(group);
        await db.SaveChangesAsync(cancellationToken);
    }
}
