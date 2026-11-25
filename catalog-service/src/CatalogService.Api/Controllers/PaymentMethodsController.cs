using CatalogService.Application.PaymentMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class PaymentMethodsController(
    GetPaymentMethodsUseCase getUseCase,
    CreatePaymentMethodUseCase createUseCase,
    UpdatePaymentMethodUseCase updateUseCase,
    DeletePaymentMethodUseCase deleteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken ct)
    {
        var result = await getUseCase.ExecuteAsync(ct);
        return Ok(Answer<IReadOnlyList<PaymentMethodDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Create([FromBody] SavePaymentMethodRequest request, CancellationToken ct)
    {
        var actorId = User.RequireUserId();
        var answer = await createUseCase.ExecuteAsync(actorId, request, ct);
        return answer.Success ? StatusCode(201, answer) : BadRequest(answer);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Update(long id, [FromBody] SavePaymentMethodRequest request, CancellationToken ct)
    {
        var actorId = User.RequireUserId();
        var answer = await updateUseCase.ExecuteAsync(id, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "PAYMENT_METHOD_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = PosPolicies.ManagerOrAbove)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var actorId = User.RequireUserId();
        var answer = await deleteUseCase.ExecuteAsync(id, actorId, ct);
        if (answer.Success) return Ok(answer);
        return answer.Code == "PAYMENT_METHOD_NOT_FOUND" ? NotFound(answer) : BadRequest(answer);
    }
}