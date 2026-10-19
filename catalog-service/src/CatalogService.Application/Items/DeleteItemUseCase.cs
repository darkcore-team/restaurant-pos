using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Items;

public sealed class DeleteItemUseCase(IProductRepository productRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct)
    {
        var ok = await productRepository.SoftDeleteAsync(id, restaurantId, actorId, ct);
        return ok
            ? Answer.Ok(message: "Producto eliminado")
            : Answer.Fail(ItemErrors.NotFound, "Producto no encontrado");
    }
}