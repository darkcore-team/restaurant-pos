using IdentityService.Application.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/login-attempts")]
[Authorize(Policy = PosPolicies.AdminOnly)]
public class LoginAttemptsController(GetLoginAttemptsUseCase getUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId))
            return Unauthorized();

        var items = await getUseCase.ExecuteAsync(restaurantId, limit, ct);
        return Ok(Answer<IReadOnlyList<LoginAttemptDto>>.Ok(items));
    }
}
