using RestaurantPos.Shared.Abstractions.Common;
using TablesService.Domain.Constants;
using TablesService.Domain.Repositories;

namespace TablesService.Application.Tables;

public sealed class ReleaseTableUseCase(
    ITableRepository tableRepository,
    ITableAssignmentRepository assignmentRepository)
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
            return Answer.Fail("TABLE_NOT_FOUND", "Mesa no encontrada");

        var assignment = await assignmentRepository.GetActiveByTableAsync(tableId, ct);
        if (assignment is null)
            return Answer.Fail("NO_ACTIVE_ASSIGNMENT", "La mesa no tiene asignación activa");

        if (!isManager && assignment.WaiterId != actorId)
            return Answer.Fail("FORBIDDEN", "No podés liberar la mesa de otro mesero");

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

        return Answer.Ok(message: "Mesa liberada");
    }
}