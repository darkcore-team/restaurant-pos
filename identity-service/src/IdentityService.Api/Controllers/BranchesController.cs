using IdentityService.Application.Branches;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController(IUserBranchRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyBranches(CancellationToken ct)
    {
        var subject = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (subject is null || !long.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var branches = await GetMyBranchesUseCase.ExecuteAsync(repository, userId, ct);
        return Ok(branches);
    }
}
