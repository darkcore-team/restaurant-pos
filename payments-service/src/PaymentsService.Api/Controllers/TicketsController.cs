using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.Payments;
using RestaurantPos.Shared.Infrastructure.Authentication;

namespace PaymentsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PosPolicies.TenantAccess)]
[Authorize(Policy = PosPolicies.CashierOrAbove)]
public class TicketsController(GetTicketUseCase getTicketUseCase) : ControllerBase
{
    [HttpGet("{number}")]
    public async Task<IActionResult> Get(string number, CancellationToken ct)
    {
        var restaurantId = User.RequireRestaurantId();
        var answer = await getTicketUseCase.ExecuteAsync(number, restaurantId, ct);

        if (answer.Success) return Ok(answer);
        if (answer.Code == "TICKET_NOT_FOUND") return NotFound(answer);

        return BadRequest(answer);
    }
}