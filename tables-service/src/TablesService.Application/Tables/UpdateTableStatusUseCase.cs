using RestaurantPos.Shared.Abstractions.Common;
using TablesService.Domain.Constants;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class UpdateTableStatusUseCase(ITableRepository tableRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, string status, CancellationToken ct)
    {
        var table = await tableRepository.GetByIdAsync(id, restaurantId, ct);
        if (table is null)
            return Answer.Fail("TABLE_NOT_FOUND", "Mesa no encontrada");

        if (!TryParseStatus(status, out var parsed))
            return Answer.Fail("INVALID_STATUS", "Estado inválido");

        table.Status = parsed;
        table.UpdatedAt = DateTimeOffset.UtcNow;
        await tableRepository.UpdateAsync(table, ct);
        return Answer.Ok(message: "Estado actualizado");
    }

    private static bool TryParseStatus(string value, out TableStatus status)
    {
        status = default;
        var v = value.Trim().ToUpperInvariant().Replace("_", "");
        status = v switch
        {
            "FREE" => TableStatus.Free,
            "OCCUPIED" => TableStatus.Occupied,
            "BILLING" => TableStatus.Billing,
            "RESERVED" => TableStatus.Reserved,
            "DIRTY" => TableStatus.Dirty,
            "OUTOFSERVICE" => TableStatus.OutOfService,
            _ => default
        };
        if (v is "FREE" or "OCCUPIED" or "BILLING" or "RESERVED" or "DIRTY" or "OUTOFSERVICE")
            return true;
        return Enum.TryParse<TableStatus>(value, true, out status);
    }
}