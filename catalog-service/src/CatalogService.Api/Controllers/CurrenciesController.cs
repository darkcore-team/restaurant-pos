using CatalogService.Application.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class CurrenciesController(GetCurrenciesUseCase useCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrencies(CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(cancellationToken);
        return Ok(result);
    }
}