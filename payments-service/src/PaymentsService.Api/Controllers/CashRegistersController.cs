using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.CashRegisters;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace PaymentsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.CashierOrAbove)]
public class CashRegistersController(GetCashRegistersUseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();

        var answer = await useCase.ExecuteAsync(restaurantId, branchId, ct);
        return answer.Success ? Ok(answer) : BadRequest(answer);
    }
}