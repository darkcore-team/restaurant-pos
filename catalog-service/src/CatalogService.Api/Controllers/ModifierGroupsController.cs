using CatalogService.Application.ModifierGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/modifier-groups")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class ModifierGroupsController(
    GetModifierGroupsUseCase getUseCase,
    CreateModifierGroupUseCase createUseCase,
    UpdateModifierGroupUseCase updateUseCase,
    DeleteModifierGroupUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var result = await getUseCase.ExecuteAsync(restaurantId, ct);
        return Ok(Answer<IReadOnlyList<ModifierGroupDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] SaveModifierGroupRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await createUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        return answer.Success ? StatusCode(201, answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] SaveModifierGroupRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await updateUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "MODIFIER_GROUP_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
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
