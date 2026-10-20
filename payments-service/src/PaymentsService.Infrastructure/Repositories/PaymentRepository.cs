using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Events;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;
using RestaurantPos.Shared.Abstractions.Events;
using RestaurantPos.Shared.Persistence.Outbox;

namespace PaymentsService.Infrastructure.Repositories;

public class PaymentRepository(PaymentsServiceDbContext db) : IPaymentRepository
{
    public async Task CreateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        await db.SaveChangesAsync(ct);
    }

    public async Task<Payment?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
    {
        return await db.Payments.FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurantId, ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByOrderAsync(long orderId, long restaurantId, CancellationToken ct = default)
    {
        return await db.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == orderId && p.RestaurantId == restaurantId && p.DeletedAt == null)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<decimal> SumKeptBySessionAsync(long cashSessionId, CancellationToken ct = default)
    {
        // Cash only, card and transfers are not physical money
        return await db.Payments
            .Where(p => p.CashSessionId == cashSessionId && p.Status == PaymentStatus.Completed && p.MethodCode == PaymentMethods.Cash)
            .SumAsync(p => p.AmountTendered - p.ChangeGiven, ct);
    }

    public async Task<decimal> SumCompletedByOrderAsync(long orderId, long restaurantId, CancellationToken ct = default)
    {
        // amount - tip (tip does not reduce debt)
        return await db.Payments
            .Where(p => p.OrderId == orderId && p.RestaurantId == restaurantId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount - p.TipAmount, ct);
    }

    public async Task<PaymentRegistrationResult> RegisterAsync(Payment payment, Ticket? ticket, DomainEvent domainEvent, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        db.Payments.Add(payment);

        if (ticket is not null)
        {
            // atomic upsert
            var next = await db.Database.SqlQuery<long>($@"
                INSERT INTO payments.ticket_sequences (restaurant_id, branch_id, last_number)
                VALUES ({ticket.RestaurantId}, {ticket.BranchId}, 1)
                ON CONFLICT (restaurant_id, branch_id)
                DO UPDATE SET last_number = payments.ticket_sequences.last_number + 1
                RETURNING last_number
            ").SingleAsync(ct);

            ticket.TicketNumber = $"TKT-{ticket.BranchId:D3}-{next:D6}";
            db.Tickets.Add(ticket);
        }
        
        db.AddDomainEvent(domainEvent);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new PaymentRegistrationResult(payment.Id, payment.PublicId, ticket?.TicketNumber);
    }

    public async Task CancelAsync(Payment payment, DomainEvent domainEvent, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        db.AddDomainEvent(domainEvent);
        await db.SaveChangesAsync(ct);
    }
}