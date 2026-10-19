using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using Microsoft.Extensions.Localization;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Items;

public sealed class UpdateItemUseCase(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IStationRepository stationRepository,
    ITaxRepository taxRepository,
    IOptionGroupRepository optionGroupRepository)
{
    public async Task<Answer> ExecuteAsync(
        long id,
        long restaurantId,
        long actorId,
        UpdateItemRequest request,
        CancellationToken ct,
        IStringLocalizer localizer)
    {
        var product = await productRepository.GetByIdAsync(id, restaurantId, ct);
        if (product is null)
            return Answer.Fail(ItemErrors.NotFound, localizer["ProductNotFound"]);

        if (!await categoryRepository.ExistsAsync(request.CategoryId, restaurantId, product.BranchId, ct))
            return Answer.Fail(ItemErrors.UnknownCategory, localizer["UnknownCategory"]);

        if (!await stationRepository.ExistsAsync(request.StationId, restaurantId, ct))
            return Answer.Fail(ItemErrors.UnknownStation, localizer["UnknownStation"]);

        if (request.TaxId.HasValue && !await taxRepository.ExistsAsync(request.TaxId.Value, ct))
            return Answer.Fail(ItemErrors.UnknownTax, localizer["UnknownTax"]);

        var optionGroupIds = request.OptionGroups?.Select(l => l.OptionGroupId).Distinct().ToList() ?? new();
        if (optionGroupIds.Count > 0 && await optionGroupRepository.CountExistingAsync(optionGroupIds, restaurantId, ct) != optionGroupIds.Count)
            return Answer.Fail(ItemErrors.UnknownOptionGroup, localizer["UnknownOptionGroup"]);

        var now = DateTimeOffset.UtcNow;
        product.Name = request.Name;
        product.NameEn = request.NameEn;
        product.Description = request.Description;
        product.BasePrice = request.BasePrice;
        product.CategoryId = request.CategoryId;
        product.StationId = request.StationId;
        product.TaxId = request.TaxId;
        product.IsTaxExempt = request.IsTaxExempt;
        product.IsAvailable = request.IsAvailable;
        product.Position = request.Position;
        product.UpdatedAt = now;
        product.UpdatedBy = actorId;

        product.OptionGroups.Clear();
        if (request.OptionGroups is not null)
        {
            foreach (var link in request.OptionGroups)
            {
                var og = await optionGroupRepository.GetByIdAsync(link.OptionGroupId, ct)
                    ?? throw new InvalidOperationException("Option group not found");
                product.OptionGroups.Add(new ProductOptionGroup
                {
                    RestaurantId = restaurantId,
                    Product = product,
                    OptionGroupId = link.OptionGroupId,
                    MinSelect = link.MinSelect ?? og.MinSelect,
                    MaxSelect = link.MaxSelect ?? og.MaxSelect,
                    Position = link.Position,
                    Status = RecordStatus.Active,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = actorId,
                    UpdatedBy = actorId
                });
            }
        }

        await productRepository.UpdateAsync(product, ct);
        return Answer.Ok(message: localizer["ProductUpdated"]);
    }
}