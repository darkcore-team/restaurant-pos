using CatalogService.Application.Stations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class StationsController(GetStationsUseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStations(CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return Ok(Answer<IReadOnlyList<StationDto>>.Ok(result));
    }
}