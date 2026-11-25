using CatalogService.Application.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class CategoriesController(
    GetCategoriesUseCase getUseCase,
    CreateCategoryUseCase createUseCase,
    UpdateCategoryUseCase updateUseCase,
    DeleteCategoryUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var result = await getUseCase.ExecuteAsync(restaurantId, branchId, ct);
        return Ok(Answer<IReadOnlyList<CategoryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] SaveCategoryRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await createUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        return answer.Success ? StatusCode(201, answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] SaveCategoryRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await updateUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "CATEGORY_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
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
