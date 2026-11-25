using IdentityService.Application.Branches;
using IdentityService.Application.Users;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.AdminOnly)]
public class BranchesController(
    IUserBranchRepository userBranchRepository,
    IUserRepository userRepository,
    GetRestaurantBranchesUseCase getRestaurantBranchesUseCase,
    CreateBranchUseCase createBranchUseCase,
    UpdateBranchUseCase updateBranchUseCase,
    DeleteBranchUseCase deleteBranchUseCase,
    IStringLocalizer localizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyBranches(CancellationToken ct)
    {
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (subject is null || !long.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var branches = await GetMyBranchesUseCase.ExecuteAsync(userBranchRepository, userId, ct);
        return Ok(branches);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllRestaurantBranches(CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId))
            return Unauthorized();

        var items = await getRestaurantBranchesUseCase.ExecuteAsync(restaurantId, ct);
        return Ok(Answer<IReadOnlyList<BranchDetailDto>>.Ok(items));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveBranchRequest request, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await createBranchUseCase.ExecuteAsync(restaurantId, actorId, request, ct);
        if (answer.Success) return StatusCode(201, answer);
        return answer.Code == "DUPLICATE_CODE" ? Conflict(answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] SaveBranchRequest request, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await updateBranchUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "BRANCH_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "DUPLICATE_CODE") return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await deleteBranchUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "BRANCH_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }
    
    [HttpPost("{branchId:long}/assign")]
    public async Task<IActionResult> AssignBranch(long branchId, AssignBranchRequest request, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId))
            return Unauthorized();

        var actorId = long.Parse(User.FindFirst(OpenIddictConstants.Claims.Subject)!.Value);
        var outcome =
            await AssignBranchUseCase.ExecuteAsync(
                userBranchRepository, userRepository, branchId, request.UserId, restaurantId, actorId, ct);

        return outcome.Succeeded
            ? NoContent()
            : outcome.ErrorCode switch
            {
                AssignBranchErrors.AlreadyAssigned => Conflict(new ProblemDetails
                    { Title = localizer["BranchAlreadyAssigned"] }),
                AssignBranchErrors.UserNotFound or AssignBranchErrors.BranchNotFound
                    => BadRequest(new ProblemDetails() { Title = localizer["InvalidAssignment"], Detail = outcome.ErrorCode }),
                _ => BadRequest(new ProblemDetails() { Title = localizer["InvalidAssignment"] })
            };
    }
}
