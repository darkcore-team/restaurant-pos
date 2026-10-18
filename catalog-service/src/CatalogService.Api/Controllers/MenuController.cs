using CatalogService.Application.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class MenuController(GetMenuUseCase useCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(MenuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMenu([FromQuery] long branchId, CancellationToken cancellationToken)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "El parametro 'branchId' es obligatorio y debe se mayor a 0." });
        }

        var result = await useCase.ExecuteAsync(branchId, cancellationToken);
        return Ok(result);
    }
}