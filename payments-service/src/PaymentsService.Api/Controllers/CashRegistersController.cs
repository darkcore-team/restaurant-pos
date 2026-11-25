using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.CashRegisters;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace PaymentsService.Api.Controllers;

[ApiController]
[Route("api/cash-registers")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.CashierOrAbove)]
public class CashRegistersController(
    GetCashRegistersUseCase getUseCase,
    CreateCashRegisterUseCase createUseCase,
    UpdateCashRegisterUseCase updateUseCase,
    DeleteCashRegisterUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();

        var answer = await getUseCase.ExecuteAsync(restaurantId, branchId, ct);
        return answer.Success ? Ok(answer) : BadRequest(answer);
    }

    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] SaveCashRegisterRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await createUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        if (answer.Success) return StatusCode(201, answer);
        return answer.Code == "DUPLICATE_CODE" ? Conflict(answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] SaveCashRegisterRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await updateUseCase.ExecuteAsync(id, restaurantId, branchId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "CASH_REGISTER_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "DUPLICATE_CODE") return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await deleteUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "CASH_REGISTER_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "REGISTER_HAS_OPEN_SESSION") return Conflict(answer);
        return BadRequest(answer);
    }
}