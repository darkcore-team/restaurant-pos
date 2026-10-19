using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using Microsoft.Extensions.Localization;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Items;

public sealed class CreateItemUseCase(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IStationRepository stationRepository,
    ITaxRepository taxRepository,
    IOptionGroupRepository optionGroupRepository)
{
    public async Task<Answer<long?>> ExecuteAsync(
        long restaurantId,
        long branchId,
        long actorId,
        CreateItemRequest request,
        CancellationToken ct,
        IStringLocalizer localizer)
    {
        if (!await categoryRepository.ExistsAsync(request.CategoryId, restaurantId, branchId, ct))
            return Answer<long?>.Fail(ItemErrors.UnknownCategory, localizer["UnknownCategory"]);

        if (!await stationRepository.ExistsAsync(request.StationId, restaurantId, ct))
            return Answer<long?>.Fail(ItemErrors.UnknownStation, localizer["UnknownStation"]);

        if (request.TaxId.HasValue && !await taxRepository.ExistsAsync(request.TaxId.Value, ct))
            return Answer<long?>.Fail(ItemErrors.UnknownTax, localizer["UnknownTax"]);

        var optionGroupIds = request.OptionGroups?.Select(l => l.OptionGroupId).Distinct().ToList() ?? new();
        if (optionGroupIds.Count > 0 && await optionGroupRepository.CountExistingAsync(optionGroupIds, restaurantId, ct) != optionGroupIds.Count)
            return Answer<long?>.Fail(ItemErrors.UnknownOptionGroup, localizer["UnknownOptionGroup"]);

        var now = DateTimeOffset.UtcNow;
        var product = new Product
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            CategoryId = request.CategoryId,
            StationId = request.StationId,
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            BasePrice = request.BasePrice,
            TaxId = request.TaxId,
            IsTaxExempt = request.IsTaxExempt,
            IsAvailable = request.IsAvailable,
            Position = request.Position,
            Status = RecordStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        };

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

        await productRepository.CreateAsync(product, ct);
        return Answer<long?>.Ok(product.Id, localizer["ProductCreated"]);
    }
}