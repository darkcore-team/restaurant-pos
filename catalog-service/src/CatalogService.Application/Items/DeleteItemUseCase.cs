using CatalogService.Domain.Repositories;
using Microsoft.Extensions.Localization;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Items;

public sealed class DeleteItemUseCase(IProductRepository productRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct, IStringLocalizer localizer)
    {
        var ok = await productRepository.SoftDeleteAsync(id, restaurantId, actorId, ct);
        return ok
            ? Answer.Ok(message: localizer["ProductDeleted"])
            : Answer.Fail(ItemErrors.NotFound, localizer["ProductNotFound"]);
    }
}