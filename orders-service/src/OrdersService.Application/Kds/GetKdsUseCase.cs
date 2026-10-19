using Microsoft.Extensions.Localization;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Abstractions.Tenancy;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Kds;

public sealed class GetKdsUseCase(IOrderRepository orderRepository, ITenantContext tenantContext, IStringLocalizer localizer)
{
    public async Task<Answer<IReadOnlyList<KdsOrderGroupDto>>> ExecuteAsync(string stationCode, CancellationToken ct = default)
    {
        var restaurantId = tenantContext.RestaurantId;
        var branchId = tenantContext.BranchId;

        if (restaurantId is null || branchId is null)
            return Answer<IReadOnlyList<KdsOrderGroupDto>>.Fail("MISSING_TENANT", localizer["MissingTenant"]);

        var items = await orderRepository.GetActiveKdsItemsAsync(stationCode, restaurantId.Value, branchId.Value, ct);

        var groups = items
            .GroupBy(i => i.OrderId)
            .Select(g => new KdsOrderGroupDto(
                g.Key,
                g.First().Order.OrderNumber,
                g.First().Order.TableId,
                g.First().Order.Status.ToString(),
                g.Select(i => new KdsItemDto(
                    i.Id,
                    i.PublicId,
                    i.ItemId,
                    i.Name,
                    i.Quantity,
                    i.ModifiersTotal,
                    i.Status.ToString(),
                    i.Notes,
                    i.StationCode,
                    i.Modifiers.Select(m => m.Name).ToList()
                )).ToList()
            )).ToList();

        return Answer<IReadOnlyList<KdsOrderGroupDto>>.Ok(groups);
    }
}