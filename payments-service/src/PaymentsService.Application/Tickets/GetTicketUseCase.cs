using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.Payments;

public sealed class GetTicketUseCase(ITicketRepository ticketRepository, IStringLocalizer localizer)
{
    public async Task<Answer<TicketDto>> ExecuteAsync(string number, long restaurantId, CancellationToken ct = default)
    {
        var ticket = await ticketRepository.GetByNumberAsync(restaurantId, number, ct);
        if (ticket is null)
            return Answer<TicketDto>.Fail("TICKET_NOT_FOUND", localizer["TicketNotFound"]);

        return Answer<TicketDto>.Ok(new TicketDto(
            ticket.Id,
            ticket.PublicId,
            ticket.TicketNumber,
            ticket.OrderId,
            ticket.BranchId,
            ticket.IssuedAt));
    }
}