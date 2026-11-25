using System.ComponentModel.DataAnnotations;
using RestaurantPos.Shared.Abstractions.Common;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Zones;

public sealed record ZoneDto(long Id, Guid PublicId, string Name, short Position, bool IsActive);

public sealed class SaveZoneRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    public short Position { get; set; }
}

public sealed class GetZonesUseCase(IZoneRepository zoneRepository)
{
    public async Task<IReadOnlyList<ZoneDto>> ExecuteAsync(long restaurantId, long branchId, CancellationToken ct = default)
    {
        var zones = await zoneRepository.GetByBranchAsync(restaurantId, branchId, ct);
        return zones.Select(ToDto).ToList();
    }

    internal static ZoneDto ToDto(Zone z) => new(z.Id, z.PublicId, z.Name, z.Position, z.IsActive);
}

public sealed class CreateZoneUseCase(IZoneRepository zoneRepository)
{
    public async Task<Answer<ZoneDto>> ExecuteAsync(
        long restaurantId, long branchId, long actorId, SaveZoneRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var zone = new Zone
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId = branchId,
            Name = request.Name.Trim(),
            Position = request.Position,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        await zoneRepository.CreateAsync(zone, ct);
        return Answer<ZoneDto>.Ok(GetZonesUseCase.ToDto(zone));
    }
}

public sealed class UpdateZoneUseCase(IZoneRepository zoneRepository)
{
    public async Task<Answer<ZoneDto>> ExecuteAsync(
        long id, long restaurantId, long actorId, SaveZoneRequest request, CancellationToken ct = default)
    {
        var zone = await zoneRepository.GetByIdAsync(id, restaurantId, ct);
        if (zone is null)
            return Answer<ZoneDto>.Fail("ZONE_NOT_FOUND", "Zona no encontrada.");

        zone.Name = request.Name.Trim();
        zone.Position = request.Position;
        zone.UpdatedAt = DateTimeOffset.UtcNow;
        zone.UpdatedBy = actorId;

        await zoneRepository.UpdateAsync(zone, ct);
        return Answer<ZoneDto>.Ok(GetZonesUseCase.ToDto(zone));
    }
}

public sealed class DeleteZoneUseCase(IZoneRepository zoneRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var zone = await zoneRepository.GetByIdAsync(id, restaurantId, ct);
        if (zone is null)
            return Answer.Fail("ZONE_NOT_FOUND", "Zona no encontrada.");

        var now = DateTimeOffset.UtcNow;
        zone.IsActive = false;
        zone.DeletedAt = now;
        zone.UpdatedAt = now;
        zone.UpdatedBy = actorId;

        await zoneRepository.UpdateAsync(zone, ct);
        return Answer.Ok(message: "Zona eliminada.");
    }
}
