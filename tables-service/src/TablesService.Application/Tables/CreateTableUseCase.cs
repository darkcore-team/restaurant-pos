using RestaurantPos.Shared.Abstractions.Common;
using TablesService.Domain.Constants;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class CreateTableUseCase(ITableRepository tableRepository, IZoneRepository zoneRepository)
{
    public async Task<Answer<long?>> ExecuteAsync(
        long restaurantId,
        long branchId,
        long actorId,
        CreateTableRequest request,
        CancellationToken ct)
    {
        if (!await zoneRepository.ExistsAsync(request.ZoneId, restaurantId, branchId, ct))
            return Answer<long?>.Fail("UNKNOWN_ZONE", "Zona no encontrada");

        if (await tableRepository.CodeExistsAsync(request.Code, branchId, ct))
            return Answer<long?>.Fail("DUPLICATE_CODE", "Ya existe una mesa con ese código");

        var now = DateTimeOffset.UtcNow;
        var table = new DiningTable
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            ZoneId = request.ZoneId,
            Code = request.Code,
            Name = request.Name,
            MinCapacity = request.MinCapacity,
            MaxCapacity = request.MaxCapacity,
            IsCombinable = request.IsCombinable,
            Status = TableStatus.Free,
            Position = request.Position,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        };

        await tableRepository.CreateAsync(table, ct);
        return Answer<long?>.Ok(table.Id, "Mesa creada");
    }
}