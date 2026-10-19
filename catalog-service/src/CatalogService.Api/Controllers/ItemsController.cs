using CatalogService.Application.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;
using System.Security.Claims;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.ManagerOrAbove)]
public class ItemsController(
    CreateItemUseCase createUseCase,
    UpdateItemUseCase updateUseCase,
    DeleteItemUseCase deleteUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request, CancellationToken ct)
    {
        if (!TryGetTenant(out var restaurantId, out var branchId, out var actorId))
            return Unauthorized(Answer<long?>.Fail("UNAUTHORIZED", "No autenticado"));

        var answer = await createUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        return answer.Success ? StatusCode(201, answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        if (!TryGetTenant(out var restaurantId, out _, out var actorId))
            return Unauthorized(Answer.Fail("UNAUTHORIZED", "No autenticado"));

        var answer = await updateUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == ItemErrors.NotFound ? NotFound(answer) : BadRequest(answer);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        if (!TryGetTenant(out var restaurantId, out _, out var actorId))
            return Unauthorized(Answer.Fail("UNAUTHORIZED", "No autenticado"));

        var answer = await deleteUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        return NotFound(answer);
    }

    private bool TryGetTenant(out long restaurantId, out long branchId, out long actorId)
    {
        restaurantId = 0; branchId = 0; actorId = 0;
        var r = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var b = User.FindFirst(PosClaims.BranchId)?.Value;
        var s = User.FindFirst("sub")?.Value;
        return long.TryParse(r, out restaurantId) && long.TryParse(b, out branchId) && long.TryParse(s, out actorId);
    }
}