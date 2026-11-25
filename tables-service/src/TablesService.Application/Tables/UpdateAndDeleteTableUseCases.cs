using System.ComponentModel.DataAnnotations;
using RestaurantPos.Shared.Abstractions.Common;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class UpdateTableRequest
{
    [Required] public long ZoneId { get; set; }
    [Required, MaxLength(20)] public string Code { get; set; } = null!;
    [Required, MaxLength(80)] public string Name { get; set; } = null!;
    [Range(1, 50)] public short MinCapacity { get; set; } = 1;
    [Range(1, 50)] public short MaxCapacity { get; set; } = 4;
    public bool IsCombinable { get; set; }
    public short Position { get; set; }
    [MaxLength(250)] public string? Notes { get; set; }
}

public sealed class UpdateTableUseCase(ITableRepository tableRepository, IZoneRepository zoneRepository)
{
    public async Task<Answer> ExecuteAsync(
        long id, long restaurantId, long branchId, long actorId, UpdateTableRequest request, CancellationToken ct = default)
    {
        var table = await tableRepository.GetByIdAsync(id, restaurantId, ct);
        if (table is null)
            return Answer.Fail("TABLE_NOT_FOUND", "Mesa no encontrada.");

        if (!await zoneRepository.ExistsAsync(request.ZoneId, restaurantId, branchId, ct))
            return Answer.Fail("UNKNOWN_ZONE", "La zona indicada no existe.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await tableRepository.CodeExistsAsync(code, branchId, id, ct))
            return Answer.Fail("DUPLICATE_CODE", $"Ya existe otra mesa con código '{code}'.");

        table.ZoneId = request.ZoneId;
        table.Code = code;
        table.Name = request.Name.Trim();
        table.MinCapacity = request.MinCapacity;
        table.MaxCapacity = Math.Max(request.MinCapacity, request.MaxCapacity);
        table.IsCombinable = request.IsCombinable;
        table.Position = request.Position;
        table.Notes = request.Notes?.Trim();
        table.UpdatedAt = DateTimeOffset.UtcNow;
        table.UpdatedBy = actorId;

        await tableRepository.UpdateAsync(table, ct);
        return Answer.Ok(message: "Mesa actualizada.");
    }
}

public sealed class DeleteTableUseCase(ITableRepository tableRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var table = await tableRepository.GetByIdAsync(id, restaurantId, ct);
        if (table is null)
            return Answer.Fail("TABLE_NOT_FOUND", "Mesa no encontrada.");

        var now = DateTimeOffset.UtcNow;
        table.IsActive = false;
        table.DeletedAt = now;
        table.UpdatedAt = now;
        table.UpdatedBy = actorId;

        await tableRepository.UpdateAsync(table, ct);
        return Answer.Ok(message: "Mesa eliminada.");
    }
}
