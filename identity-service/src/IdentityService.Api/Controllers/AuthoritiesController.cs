using IdentityService.Application.Authorities;
using IdentityService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthoritiesController(IAuthorityRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAuthorities(CancellationToken ct)
    {
        var authorities = await GetAuthoritiesUseCase.ExecuteAsync(repository, ct);
        return Ok(authorities);
    }
}