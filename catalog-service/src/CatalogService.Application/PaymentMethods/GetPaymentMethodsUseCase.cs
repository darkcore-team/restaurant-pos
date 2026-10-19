using CatalogService.Domain.Repositories;

namespace CatalogService.Application.PaymentMethods;

public class GetPaymentMethodsUseCase(IPaymentMethodRepository repository)
{
    public async Task<IReadOnlyList<PaymentMethodDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var items = await repository.GetActiveAsync(ct);

        return items.Select(pm => new PaymentMethodDto(
            pm.Id,
            pm.PublicId,
            pm.Code,
            pm.Name,
            pm.RequiresReference,
            pm.RequiresAmountTendered,
            pm.RequiresCashSession,
            pm.Position
        )).ToList();
    }
}