using PaymentsService.Domain.Entities;

namespace PaymentsService.Domain.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByNumberAsync(long restaurantId, string number, CancellationToken ct = default);
}