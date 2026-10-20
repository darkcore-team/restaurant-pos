using PaymentsService.Domain.Entities;

namespace PaymentsService.Domain.Repositories;

public interface IPaymentRepository
{
    Task CreateAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByOrderAsync(long orderId, long restaurantId, CancellationToken ct = default);
    Task<decimal> SumKeptBySessionAsync(long cashSessionId, CancellationToken ct = default);
    Task<decimal> SumCompletedByOrderAsync(long orderId, CancellationToken ct = default);
}