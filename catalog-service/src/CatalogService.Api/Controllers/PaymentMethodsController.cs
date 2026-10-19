using CatalogService.Application.PaymentMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class PaymentMethodsController(GetPaymentMethodsUseCase useCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentMethodDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(ct);
        return Ok(result);
    }
}