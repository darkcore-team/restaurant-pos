using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Abstractions.Tenancy;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Floor;

public sealed class GetFloorUseCase(IFloorRepository floorRepository, ITenantContext tenantContext)
{
    public async Task<Answer<IReadOnlyList<ZoneDto>>> ExecuteAsync(CancellationToken ct = default)
    {
        var restaurantId = tenantContext.RestaurantId;
        var branchId = tenantContext.BranchId;
        if (restaurantId is null || branchId is null)
            return Answer<IReadOnlyList<ZoneDto>>.Fail("MISSING_TENANT", "Falta restaurant_id/branch_id en el token");

        var zones = await floorRepository.GetFloorAsync(restaurantId.Value, branchId.Value, ct);

        var dtos = zones.Select(z => new ZoneDto(
            z.Id,
            z.PublicId,
            z.Name,
            z.Position,
            z.IsActive,
            z.Tables.Select(t =>
            {
                var active = t.Assignments.FirstOrDefault();
                return new TableDto(
                    t.Id,
                    t.PublicId,
                    t.Code,
                    t.Name,
                    t.MinCapacity,
                    t.MaxCapacity,
                    t.IsCombinable,
                    t.Status.ToString(),
                    t.Position,
                    t.Notes,
                    t.IsActive,
                    t.ZoneId,
                    active?.WaiterId,
                    active?.AssignedAt);
            }).ToList()
        )).ToList();

        return Answer<IReadOnlyList<ZoneDto>>.Ok(dtos);
    }
}