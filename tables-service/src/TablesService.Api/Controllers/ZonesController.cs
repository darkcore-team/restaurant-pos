using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;
using TablesService.Application.Zones;

namespace TablesService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class ZonesController(
    GetZonesUseCase getUseCase,
    CreateZoneUseCase createUseCase,
    UpdateZoneUseCase updateUseCase,
    DeleteZoneUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var items = await getUseCase.ExecuteAsync(restaurantId, branchId, ct);
        return Ok(Answer<IReadOnlyList<ZoneDto>>.Ok(items));
    }

    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] SaveZoneRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await createUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        return answer.Success ? StatusCode(201, answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] SaveZoneRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await updateUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "ZONE_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await deleteUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "ZONE_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }
}
