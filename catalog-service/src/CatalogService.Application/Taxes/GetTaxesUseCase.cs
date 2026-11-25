using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Taxes;

public sealed record TaxDto(long Id, Guid PublicId, string Code, string Name, decimal Rate, bool IsInclusive);

public sealed class GetTaxesUseCase(ITaxRepository taxRepository)
{
    public async Task<IReadOnlyList<TaxDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var taxes = await taxRepository.GetActiveAsync(ct);
        return taxes.Select(t => new TaxDto(t.Id, t.PublicId, t.Code, t.Name, t.Rate, t.IsInclusive)).ToList();
    }
}
