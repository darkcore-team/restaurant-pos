using System.Security.Claims;
using IdentityService.Application.Users;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.AdminOnly)]
public class UsersController(IUserRepository repository, IPasswordHasher<User> passwordHasher) : ControllerBase
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
            return Conflict(new ProblemDetails { Title = "El usuario ya existe" });
        }

        return BadRequest(new ProblemDetails()
        {
            Title = "Solicitud Inválida",
            Detail = outcome.ErrorCode
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var restaurantIdValue = User.FindFirst(PosClaims.RestaurantId)?.Value;
        
        if (!long.TryParse(restaurantIdValue, out var restaurantId))
            return BadRequest(new ProblemDetails() { Title = "Missing restaurant_id claim" });

        return Ok(await GetUserUseCase.ExecuteAsync(repository, restaurantId, ct));
    }
}