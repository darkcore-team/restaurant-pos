using IdentityService.Application.Restaurants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.AdminOnly)]
public class RestaurantController(
    GetRestaurantUseCase getUseCase,
    UpdateRestaurantUseCase updateUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId))
            return Unauthorized();

        var answer = await getUseCase.ExecuteAsync(restaurantId, ct);
        return answer.Success ? Ok(answer) : NotFound(answer);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRestaurantRequest request, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await updateUseCase.ExecuteAsync(restaurantId, actorId, request, ct);
        return answer.Success ? Ok(answer) : BadRequest(answer);
    }
}
