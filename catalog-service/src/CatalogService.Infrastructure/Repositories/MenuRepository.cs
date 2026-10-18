using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public sealed class MenuRepository(CatalogServiceDbContext db) : IMenuRepository
{
    public async Task<IReadOnlyList<Category>> GetMenuByBranchAsync(long branchId, CancellationToken cancellationToken = default)
    {
        return await db.Categories
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && c.Status == RecordStatus.Active)
            .OrderBy(c => c.Position)
            .Include(c => c.Products.Where(p => p.Status == RecordStatus.Active))
                .ThenInclude(p => p.Station)
            .Include(c => c.Products.Where(p => p.Status == RecordStatus.Active))
                .ThenInclude(p => p.OptionGroups.Where(pog => pog.Status == RecordStatus.Active))
                    .ThenInclude(pog => pog.OptionGroup)
                        .ThenInclude(og => og.Options.Where(o => o.Status == RecordStatus.Active))
            .ToListAsync(cancellationToken);
    }
}