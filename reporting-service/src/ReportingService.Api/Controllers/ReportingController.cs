using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportingService.Application.Reporting;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace ReportingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.CashierOrAbove)]
public class ReportingController(GetDailySalesUseCase dailySales, GetTopProductsUseCase topProducts, GetByWaiterUseCase byWaiter, GetTipsUseCase tips) : ControllerBase
{
    [HttpGet("daily-sales")]
    public async Task<IActionResult> DailySales([FromQuery] long branchId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var data = await dailySales.ExecuteAsync(restaurantId, branchId, date, ct);
        if (data is null) return NotFound();
        return Ok(data);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts([FromQuery] long branchId, [FromQuery] DateOnly date, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var restaurantId = User.RequireRestaurantId();
        return Ok(await topProducts.ExecuteAsync(restaurantId, branchId, date, limit, ct));
    }

    [HttpGet("by-waiter")]
    public async Task<IActionResult> ByWaiter([FromQuery] long branchId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        return Ok(await byWaiter.ExecuteAsync(restaurantId, branchId, date, ct));
    }

    [HttpGet("tips")]
    public async Task<IActionResult> Tips([FromQuery] long branchId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var data = await tips.ExecuteAsync(restaurantId, branchId, date, ct);
        if (data is null) return NotFound();
        return Ok(data);
    }
}
