using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RestaurantPos.Shared.Abstractions.Common;
using TablesService.Domain.Entities;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class AssignTableUseCase(
    ITableRepository tableRepository,
    ITableAssignmentRepository assignmentRepository,
    IStringLocalizer localizer)
{
    public async Task<Answer> ExecuteAsync(
        long tableId,
        long restaurantId,
        long branchId,
        long waiterId,
        long actorId,
        CancellationToken ct)
    {
        var table = await tableRepository.GetByIdAsync(tableId, restaurantId, ct);
        if (table is null)
            return Answer.Fail("TABLE_NOT_FOUND", localizer["TableNotFound"]);

        var existingOnTable = await assignmentRepository.GetActiveByTableAsync(tableId, ct);
        if (existingOnTable is not null)
            return Answer.Fail("TABLE_ALREADY_ASSIGNED", localizer["TableAlreadyAssigned"]);

        var existingForWaiter = await assignmentRepository.GetActiveByWaiterAsync(waiterId, ct);
        if (existingForWaiter is not null)
            return Answer.Fail("WAITER_ALREADY_ASSIGNED", localizer["WaiterAlreadyAssigned"]);

        var now = DateTimeOffset.UtcNow;
        var assignment = new TableAssignment
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            TableId = tableId,
            WaiterId = waiterId,
            AssignedAt = now,
            AssignedBy = actorId,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        };

        try
        {
            await assignmentRepository.CreateAsync(assignment, ct);
        }
        catch (DbUpdateException)
        {
            return Answer.Fail("ASSIGNMENT_CONFLICT", localizer["AssignmentConflict"]);
        }

        return Answer.Ok(message: localizer["TableAssigned"]);
    }
}