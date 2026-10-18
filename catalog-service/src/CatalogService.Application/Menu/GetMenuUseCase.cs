using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Menu;

public sealed class GetMenuUseCase(IMenuRepository repository)
{
    public async Task<MenuDto> ExecuteAsync(long branchId, CancellationToken cancellationToken = default)
    {
        var categories = await repository.GetMenuByBranchAsync(branchId, cancellationToken);

        var categoryDtos = categories.Select(c => new MenuCategoryDto(
            c.Id,
            c.PublicId,
            c.Name,
            c.NameEn,
            c.ImageUrl,
            c.Position,
            c.Products
                .OrderBy(p => p.Position)
                .Select(p => new MenuProductDto(
                    p.Id,
                    p.PublicId,
                    p.Name,
                    p.NameEn,
                    p.Description,
                    p.BasePrice,
                    p.ImageUrl,
                    p.IsAvailable,
                    p.Position,
                    p.StationId,
                    p.Station.Code,
                    p.TaxId,
                    p.IsTaxExempt,
                    p.OptionGroups
                        .OrderBy(pog => pog.Position)
                        .Select(pog => new MenuOptionGroupDto(
                            pog.OptionGroup.Id,
                            pog.OptionGroup.PublicId,
                            pog.OptionGroup.Name,
                            pog.MinSelect ?? pog.OptionGroup.MinSelect,
                            pog.MaxSelect ?? pog.OptionGroup.MaxSelect,
                            pog.OptionGroup.IsRequired,
                            pog.Position,
                            pog.OptionGroup.Options
                                .OrderBy(o => o.Position)
                                .Select(o => new MenuOptionItemDto(
                                    o.Id,
                                    o.PublicId,
                                    o.Name,
                                    o.NameEn,
                                    o.PriceDelta,
                                    o.IsDefault,
                                    o.Position
                                    )).ToList()
                        )).ToList()
                )).ToList()
        )).ToList();

        return new MenuDto(branchId, categoryDtos);
    }
}