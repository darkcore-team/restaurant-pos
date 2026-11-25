using CatalogService.Application.Stations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class StationsController(
    GetStationsUseCase getUseCase,
    CreateStationUseCase createUseCase,
    UpdateStationUseCase updateUseCase,
    DeleteStationUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStations(CancellationToken cancellationToken)
    {
        var result = await getUseCase.ExecuteAsync(cancellationToken);
        return Ok(Answer<IReadOnlyList<StationDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] SaveStationRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await createUseCase.ExecuteAsync(restaurantId, actorId, request, ct);
        if (answer.Success) return StatusCode(201, answer);
        return answer.Code == "STATION_CODE_EXISTS" ? Conflict(answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] SaveStationRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await updateUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "STATION_NOT_FOUND") return NotFound(answer);
        return answer.Code == "STATION_CODE_EXISTS" ? Conflict(answer) : BadRequest(answer);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await deleteUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        return answer.Success ? Ok(answer) : NotFound(answer);
    }
}
