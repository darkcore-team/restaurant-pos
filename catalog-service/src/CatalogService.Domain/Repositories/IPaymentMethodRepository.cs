using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IPaymentMethodRepository
{
    Task<IReadOnlyList<PaymentMethod>> GetActiveAsync(CancellationToken ct = default);
    Task<PaymentMethod?> GetByIdAsync(long id, CancellationToken ct = default);
    Task CreateAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
    Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
}