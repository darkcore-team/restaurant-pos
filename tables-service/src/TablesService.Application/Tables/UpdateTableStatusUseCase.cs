using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Abstractions.Events;
using TablesService.Domain.Constants;
using TablesService.Domain.Events;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class UpdateTableStatusUseCase(
    ITableRepository tableRepository,
    IEventBus eventBus,
    IStringLocalizer localizer,
    ILogger<UpdateTableStatusUseCase> logger)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, string status, CancellationToken ct)
    {
        var table = await tableRepository.GetByIdAsync(id, restaurantId, ct);
        if (table is null)
            return Answer.Fail("TABLE_NOT_FOUND", localizer["TableNotFound"]);

        if (!TryParseStatus(status, out var parsed))
            return Answer.Fail("INVALID_STATUS", localizer["InvalidStatus"]);

        table.Status = parsed;
        table.UpdatedAt = DateTimeOffset.UtcNow;
        await tableRepository.UpdateAsync(table, ct);

        try
        {
            await eventBus.PublishAsync(new TableStatusChangedEvent
            {
                RestaurantId = table.RestaurantId,
                BranchId = table.BranchId,
                TableId = table.Id,
                TableCode = table.Code,
                Status = table.Status.ToString()
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish TableStatusChangedEvent for table {TableId}", table.Id);
        }

        return Answer.Ok(message: localizer["StatusUpdated"]);
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