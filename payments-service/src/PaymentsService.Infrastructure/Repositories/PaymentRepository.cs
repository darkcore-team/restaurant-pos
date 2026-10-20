using Microsoft.EntityFrameworkCore;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using PaymentsService.Infrastructure.Data;

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
        return await db.Payments
            .Where(p => p.CashSessionId == cashSessionId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.AmountTendered - p.ChangeGiven, ct);
    }

    public async Task<decimal> SumCompletedByOrderAsync(long orderId, CancellationToken ct = default)
    {
        return await db.Payments
            .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount, ct);
    }
}