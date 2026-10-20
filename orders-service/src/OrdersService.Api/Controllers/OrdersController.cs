using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Infrastructure.Authentication;
using OrdersService.Application.Orders;

namespace OrdersService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
public class OrdersController(
    OpenOrderUseCase openUseCase,
    GetOrderUseCase getUseCase,
    AddItemsUseCase addItemsUseCase,
    VoidItemUseCase voidItemUseCase,
    SendToKitchenUseCase sendUseCase,
    UpdateItemStatusUseCase updateItemStatusUseCase,
    GetActiveOrdersUseCase activeOrdersUseCase,
    RequestBillUseCase requestBillUseCase,
    CancelOrderUseCase cancelUseCase) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> Open([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();
        var actorId = User.RequireUserId();

        var answer = await openUseCase.ExecuteAsync(restaurantId, branchId, actorId, request, ct);
        if (answer.Success) return StatusCode(201, answer);
        if (answer.Code == "TABLE_HAS_OPEN_ORDER") return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "OrderRead")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();

        var answer = await getUseCase.ExecuteAsync(id, restaurantId, ct);
        if (answer.Success) return Ok(answer);
        return NotFound(answer);
    }

    [HttpPost("{id:long}/items")]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> AddItems(long id, [FromBody] AddItemsRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await addItemsUseCase.ExecuteAsync(id, restaurantId, actorId, request, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "ORDER_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "ORDER_NOT_OPEN") return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpDelete("{id:long}/items/{itemId:long}")]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> VoidItem(long id, long itemId, [FromBody] VoidItemRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await voidItemUseCase.ExecuteAsync(id, itemId, restaurantId, actorId, request.Reason, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "ORDER_NOT_FOUND" || answer.Code == "ITEM_NOT_FOUND") return NotFound(answer);
        if (answer.Code == "ORDER_CLOSED") return Conflict(answer);
        return BadRequest(answer);
    }

    [HttpPost("{id:long}/send")]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> Send(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await sendUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "ORDER_NOT_FOUND") return NotFound(answer);
        return BadRequest(answer);
    }

    [HttpPost("{id:long}/items/{itemId:long}/status")]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> UpdateItemStatus(long id, long itemId, [FromBody] UpdateItemStatusRequest request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await updateItemStatusUseCase.ExecuteAsync(id, itemId, restaurantId, actorId, request.Status, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "ORDER_NOT_FOUND" || answer.Code == "ITEM_NOT_FOUND") return NotFound(answer);
        return BadRequest(answer);
    }

    [HttpGet("active")]
    [Authorize(Policy = "OrderRead")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var branchId = User.RequireBranchId();

        // tenant context is validated above; pass through to use case via implicit context
        var answer = await activeOrdersUseCase.ExecuteAsync(ct);
        return answer.Success ? Ok(answer) : BadRequest(answer);
    }

    [HttpPost("{id:long}/request-bill")]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> RequestBill(long id, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await requestBillUseCase.ExecuteAsync(id, restaurantId, actorId, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "ORDER_NOT_FOUND") return NotFound(answer);
        return BadRequest(answer);
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize(Policy = PosPolicies.WaiterOrAbove)]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelOrderRequest? request, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var actorId = User.RequireUserId();

        var answer = await cancelUseCase.ExecuteAsync(id, restaurantId, actorId, request?.Reason, ct);
        if (answer.Success) return Ok(answer);
        if (answer.Code == "ORDER_NOT_FOUND") return NotFound(answer);
        return BadRequest(answer);
    }
}