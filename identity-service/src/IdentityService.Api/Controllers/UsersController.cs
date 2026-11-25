using System.Security.Claims;
using IdentityService.Application.Users;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.AdminOnly)]
public class UsersController(
    IUserRepository repository,
    IPasswordHasher<User> passwordHasher,
    UpdateUserUseCase updateUserUseCase,
    ResetUserPasswordUseCase resetPasswordUseCase,
    UnlockUserUseCase unlockUserUseCase,
    IStringLocalizer localizer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
    {
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        
        if (subject == null || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var outcome = await CreateUserUseCase.ExecuteAsync(repository, passwordHasher, actorId, request, ct);

        if (outcome.Succeeded)
        {
            return Created($"/api/users/{outcome.UserId}", new CreateUserResponse(outcome.UserId!.Value));
        }

        if (outcome.ErrorCode == CreateUserErrors.UsernameTaken)
        {
            return Conflict(new ProblemDetails { Title = localizer["UsernameTaken"] });
        }

        return BadRequest(new ProblemDetails()
        {
            Title = localizer["InvalidRequest"],
            Detail = outcome.ErrorCode
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        
        if (!long.TryParse(restaurantIdValue, out var restaurantId))
            return BadRequest(new ProblemDetails() { Title = localizer["MissingRestaurantClaim"] });

        return Ok(await GetUserUseCase.ExecuteAsync(repository, restaurantId, ct));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await updateUserUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "USER_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }

    [HttpPost("{id:long}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await resetPasswordUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "USER_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }

    [HttpPost("{id:long}/unlock")]
    public async Task<IActionResult> Unlock(long id, CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (!long.TryParse(restaurantIdValue, out var restaurantId) || !long.TryParse(subject, out var actorId))
            return Unauthorized();

        var answer = await unlockUserUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "USER_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }
}