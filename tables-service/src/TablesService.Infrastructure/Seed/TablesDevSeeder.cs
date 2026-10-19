using TablesService.Domain.Constants;
using TablesService.Domain.Entities;
using TablesService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TablesService.Infrastructure.Seed;

public sealed class TablesDevSeeder(TablesServiceDbContext db)
{
    private const long RestaurantId = 1;
    private const long BranchId = 1;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await db.Zones.AnyAsync(z => z.RestaurantId == RestaurantId && z.BranchId == BranchId, ct))
            return;

        var salon = new Zone
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Name = "Salón",
            Position = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var terraza = new Zone
        {
            RestaurantId = RestaurantId,
            BranchId = BranchId,
            Name = "Terraza",
            Position = 2,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Zones.AddRange(salon, terraza);
        await db.SaveChangesAsync(ct);

        db.DiningTables.AddRange(
            new DiningTable { RestaurantId = RestaurantId, BranchId = BranchId, ZoneId = salon.Id, Code = "M1", Name = "Mesa 1", MinCapacity = 2, MaxCapacity = 4, Status = TableStatus.Free, Position = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new DiningTable { RestaurantId = RestaurantId, BranchId = BranchId, ZoneId = salon.Id, Code = "M2", Name = "Mesa 2", MinCapacity = 2, MaxCapacity = 4, Status = TableStatus.Free, Position = 2, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new DiningTable { RestaurantId = RestaurantId, BranchId = BranchId, ZoneId = salon.Id, Code = "M3", Name = "Mesa 3", MinCapacity = 4, MaxCapacity = 6, Status = TableStatus.Free, Position = 3, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new DiningTable { RestaurantId = RestaurantId, BranchId = BranchId, ZoneId = terraza.Id, Code = "T1", Name = "Terraza 1", MinCapacity = 2, MaxCapacity = 4, Status = TableStatus.Free, Position = 1, IsActive = true, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync(ct);
    }
}