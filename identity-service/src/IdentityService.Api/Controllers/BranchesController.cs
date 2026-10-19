using IdentityService.Application.Branches;
using IdentityService.Application.Users;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.AdminOnly)]
public class BranchesController(
    IUserBranchRepository userBranchRepository,
    IUserRepository userRepository,
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
