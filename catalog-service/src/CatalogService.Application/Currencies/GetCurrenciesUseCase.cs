using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Currencies;

public sealed class GetCurrenciesUseCase(ICurrencyRepository repository)
{
    public async Task<IReadOnlyList<CurrencyDto>> ExecuteAsync(CancellationToken ct)
    {
        var items = await repository.GetActiveAsync(ct);
        return items.Select(c => new CurrencyDto(c.Id, c.PublicId, c.Code, c.Name, c.Symbol, c.DecimalPlaces)).ToList();
    }
}