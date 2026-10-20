using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.Payments;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace PaymentsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.CashierOrAbove)]
public class PaymentsController(
    RegisterPaymentUseCase registerUseCase,
    CancelPaymentUseCase cancelUseCase,
    GetOrderPaymentsUseCase getOrderPaymentsUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterPaymentRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await registerUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        
        if (answer.Success) return StatusCode(201, answer);
        
        if (answer.Code is "NO_ACTIVE_SESSION" or "SESSION_NOT_OPEN" or "PAYMENT_EXCEEDS_BALANCE"
            or "ORDER_ALREADY_PAID")
            return Conflict(answer);
        
        return BadRequest(answer);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetByOrder([FromQuery] long? orderId, CancellationToken ct)
    {
        if (!orderId.HasValue) return BadRequest(Answer.Fail("ORDER_ID_REQUIRED", "orderId is required"));
        var restaurantId = User.RequireRestaurantId();
        
        var answer = await getOrderPaymentsUseCase.ExecuteAsync(orderId.Value, restaurantId, ct);
        if (answer.Success) return Ok(answer);
        return BadRequest(answer);
    }
    
    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelPaymentRequest? request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();
        
        var answer = await cancelUseCase.ExecuteAsync(id, restaurantId, actorId, request?.Reason, ct);
        
        if (answer.Success) return Ok(answer);
        
        if (answer.Code == "PAYMENT_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "PAYMENT_NOT_CANCELABLE") return Conflict(answer);
        
        return BadRequest(answer);
    }
}