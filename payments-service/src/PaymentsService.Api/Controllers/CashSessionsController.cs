using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.CashSessions;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace PaymentsService.Api.Controllers;

[ApiController]
[Route("api/cash-sessions")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.CashierOrAbove)]
public class CashSessionsController(
    OpenCashSessionUseCase openUseCase,
    CloseCashSessionUseCase closeUseCase,
    GetActiveCashSessionUseCase activeUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Open([FromBody] OpenCashSessionRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await openUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        if (answer.Success) return StatusCode(201, answer);
        if (answer.Code == "CASH_REGISTER_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "SESSION_ALREADY_OPEN") return Conflict(answer);
        return BadRequest(answer);
    }
    
    [HttpPost("{id:long}/close")]
    public async Task<IActionResult> Close(long id, [FromBody] CloseCashSessionRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await closeUseCase.ExecuteAsync(restaurantId, branchId, actorId, id, request, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "SESSION_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "SESSION_ALREADY_CLOSED") return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();

        var answer = await activeUseCase.ExecuteAsync(restaurantId, branchId, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "NO_ACTIVE_SESSION") return NotFound(answer);
        return BadRequest(answer);
    }
}