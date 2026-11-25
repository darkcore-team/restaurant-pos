using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class BranchRepository(IdentityServiceDbContext dbContext) : IBranchRepository
{
    public async Task<IReadOnlyList<Branch>> GetByRestaurantAsync(long restaurantId, CancellationToken ct)
    {
        return await dbContext.Branches
            .AsNoTracking()
            .Where(b => b.RestaurantId == restaurantId && b.Status != RecordStatus.Deleted)
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.Name)
            .ToListAsync(ct);
    }

    public async Task<Branch?> GetByIdAsync(long id, long restaurantId, CancellationToken ct)
    {
        return await dbContext.Branches
            .SingleOrDefaultAsync(
                b => b.Id == id && b.RestaurantId == restaurantId && b.Status != RecordStatus.Deleted,
                ct);
    }

    public Task<bool> CodeExistsAsync(string code, long restaurantId, long? excludeId, CancellationToken ct)
    {
        return dbContext.Branches.AnyAsync(
            b => b.RestaurantId == restaurantId
                 && b.Code == code
                 && b.Status != RecordStatus.Deleted
                 && (!excludeId.HasValue || b.Id != excludeId.Value),
            ct);
    }

    public async Task<long> GetDefaultCityIdAsync(long restaurantId, CancellationToken ct)
    {
        var existingCity = await dbContext.Branches
            .AsNoTracking()
            .Where(b => b.RestaurantId == restaurantId)
            .Select(b => (long?)b.CityId)
            .FirstOrDefaultAsync(ct);

        return existingCity ?? 1L;
    }

    public async Task CreateAsync(Branch branch, CancellationToken ct)
    {
        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Branch branch, CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
