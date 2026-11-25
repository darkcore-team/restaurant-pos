using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Abstractions.Events;
using TablesService.Domain.Constants;
using TablesService.Domain.Events;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class ReleaseTableUseCase(
    ITableRepository tableRepository,
    ITableAssignmentRepository assignmentRepository,
    IEventBus eventBus,
    IStringLocalizer localizer,
    ILogger<ReleaseTableUseCase> logger)
{
    public async Task<Answer> ExecuteAsync(
        long tableId,
        long restaurantId,
        long actorId,
        bool isManager,
        CancellationToken ct)
    {
        var table = await tableRepository.GetByIdAsync(tableId, restaurantId, ct);
        if (table is null)
            return Answer.Fail("TABLE_NOT_FOUND", localizer["TableNotFound"]);

        var assignment = await assignmentRepository.GetActiveByTableAsync(tableId, ct);
        if (assignment is null)
            return Answer.Fail("NO_ACTIVE_ASSIGNMENT", localizer["NoActiveAssignment"]);

        if (!isManager && assignment.WaiterId != actorId)
            return Answer.Fail("FORBIDDEN", localizer["ForbiddenRelease"]);

        var now = DateTimeOffset.UtcNow;
        assignment.ReleasedAt = now;
        assignment.ReleasedBy = actorId;
        assignment.UpdatedAt = now;
        assignment.UpdatedBy = actorId;
        await assignmentRepository.UpdateAsync(assignment, ct);

        table.Status = TableStatus.Dirty;
        table.UpdatedAt = now;
        table.UpdatedBy = actorId;
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

        return Answer.Ok(message: localizer["TableReleased"]);
    }
}