using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;
using TablesService.Application.Floor;

namespace TablesService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.WaiterOrAbove)]
public class FloorController(GetFloorUseCase useCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFloor(CancellationToken ct)
    {
        var answer = await useCase.ExecuteAsync(ct);
        return answer.Success ? Ok(answer) : BadRequest(answer);
    }
}