using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Items;

public sealed class GetItemsUseCase(IProductRepository productRepository)
{
    public async Task<IReadOnlyList<ItemDto>> ExecuteAsync(long restaurantId, long branchId, CancellationToken ct = default)
    {
        var products = await productRepository.GetByBranchAsync(restaurantId, branchId, ct);
        return products.Select(p => new ItemDto(
            p.Id,
            p.PublicId,
            p.Name,
            p.NameEn,
            p.Description,
            p.BasePrice,
            p.CategoryId,
            p.StationId,
            p.TaxId,
            p.IsTaxExempt,
            p.IsAvailable,
            p.Position,
            p.OptionGroups.Select(og => og.OptionGroupId).ToList()
        )).ToList();
    }
}
