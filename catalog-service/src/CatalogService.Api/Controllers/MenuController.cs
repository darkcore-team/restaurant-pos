using CatalogService.Application.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class MenuController(GetMenuUseCase useCase, IStringLocalizer localizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMenu([FromQuery] long branchId, CancellationToken cancellationToken)
    {
        if (branchId <= 0)
        {
            return BadRequest(Answer<MenuDto>.Fail("INVALID_BRANCH", localizer["BranchIdRequired"]));
        }

        var result = await useCase.ExecuteAsync(branchId, cancellationToken);
        return Ok(Answer<MenuDto>.Ok(result));
    }
}