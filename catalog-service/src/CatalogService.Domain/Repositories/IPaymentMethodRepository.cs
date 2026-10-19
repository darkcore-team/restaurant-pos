using CatalogService.Domain.Entities;

namespace CatalogService.Domain.Repositories;

public interface IPaymentMethodRepository
{
    Task<IReadOnlyList<PaymentMethod>> GetActiveAsync(CancellationToken ct = default);
}