using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;
using OrdersService.Application.Kds;

namespace OrdersService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.WaiterOrAbove)]
public class KdsController(GetKdsUseCase useCase) : ControllerBase
{
    [HttpGet("{stationCode}")]
    public async Task<IActionResult> GetStation(string stationCode, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();

        // tenant context is validated above; use case reads it from ITenantContext
        var answer = await useCase.ExecuteAsync(stationCode, ct);
        return answer.Success ? Ok(answer) : BadRequest(answer);
    }
}