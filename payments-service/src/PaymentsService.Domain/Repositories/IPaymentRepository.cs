using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Events;
using RestaurantPos.Shared.Abstractions.Events;

namespace PaymentsService.Domain.Repositories;

public interface IPaymentRepository
{
    Task CreateAsync(Payment payment, CancellationToken ct = default);
    Task UpdateAsync(Payment payment, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetByOrderAsync(long orderId, long restaurantId, CancellationToken ct = default);
    Task<decimal> SumKeptBySessionAsync(long cashSessionId, CancellationToken ct = default);
    Task<decimal> SumCompletedByOrderAsync(long orderId, long restaurantId, CancellationToken ct = default);
    Task<PaymentRegistrationResult> RegisterAsync(Payment payment, Ticket? ticket, DomainEvent domainEvent, CancellationToken ct = default);
    Task CancelAsync(Payment payment, DomainEvent domainEvent, CancellationToken ct = default);
}