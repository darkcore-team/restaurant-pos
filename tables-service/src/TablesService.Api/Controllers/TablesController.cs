using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;
using System.Security.Claims;
using TablesService.Application.Tables;

namespace TablesService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.WaiterOrAbove)]
public class TablesController(
    CreateTableUseCase createUseCase,
    UpdateTableStatusUseCase updateStatusUseCase,
    AssignTableUseCase assignUseCase,
    ReleaseTableUseCase releaseUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] CreateTableRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await createUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        return answer.Success ? StatusCode(201, answer) : BadRequest(answer);
    }

    [HttpPatch("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateTableStatusRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();

        var answer = await updateStatusUseCase.ExecuteAsync(id, restaurantId, request.Status, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "TABLE_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }

    [HttpPost("{id:long}/assign")]
    public async Task<IActionResult> Assign(long id, [FromBody] AssignTableRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var isManager = User.IsInRole("MANAGER") || User.IsInRole("ADMIN") || User.IsInRole("OWNER");
        var waiterId = isManager ? request.WaiterId : actorId;

        var answer = await assignUseCase.ExecuteAsync(id, restaurantId, branchId, waiterId, actorId, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "TABLE_NOT_FOUND") return NotFound(answer);
        if (answer.Code is "TABLE_ALREADY_ASSIGNED" or "WAITER_ALREADY_ASSIGNED" or "ASSIGNMENT_CONFLICT")
            return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpDelete("{id:long}/assign")]
    public async Task<IActionResult> Release(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var isManager = User.IsInRole("MANAGER") || User.IsInRole("ADMIN") || User.IsInRole("OWNER");
        var answer = await releaseUseCase.ExecuteAsync(id, restaurantId, actorId, isManager, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "TABLE_NOT_FOUND" || answer.Code == "NO_ACTIVE_ASSIGNMENT") return NotFound(answer);
        if (answer.Code == "FORBIDDEN") return Forbid();
        return BadRequest(answer);
    }
}