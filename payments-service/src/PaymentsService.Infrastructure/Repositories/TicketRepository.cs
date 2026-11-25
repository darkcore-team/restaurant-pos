using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;

namespace PaymentsService.Infrastructure.Repositories;

public class TicketRepository(PaymentsServiceDbContext db) : ITicketRepository
{
    public async Task<Ticket?> GetByNumberAsync(long restaurantId, string number, CancellationToken ct = default)
    {
        return await db.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.RestaurantId == restaurantId && t.TicketNumber == number, ct);
    }

    public async Task<Ticket?> GetByOrderIdAsync(long restaurantId, long orderId, CancellationToken ct = default)
    {
        return await db.Tickets
            .AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId && t.OrderId == orderId)
            .OrderByDescending(t => t.IssuedAt)
            .FirstOrDefaultAsync(ct);
    }
}