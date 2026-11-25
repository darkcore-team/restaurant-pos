using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Events;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Abstractions.Events;

namespace OrdersService.Application.Orders;

public sealed class SendToKitchenUseCase(IOrderRepository orderRepository, IStringLocalizer localizer)
{
    public async Task<Answer> ExecuteAsync(long orderId, long restaurantId, long actorId, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(orderId, restaurantId, ct);
        if (order is null)
            return Answer.Fail("ORDER_NOT_FOUND", localizer["OrderNotFound"]);

        if (order.Status != OrderStatus.Open)
            return Answer.Fail("INVALID_STATUS", localizer["InvalidStatus"]);

        var activeItems = order.Items.Where(i => i.VoidedAt == null).ToList();
        if (activeItems.Count == 0)
            return Answer.Fail("NO_ITEMS", localizer["NoItems"]);

        var now = DateTimeOffset.UtcNow;
        order.Status = OrderStatus.InPreparation;
        foreach (var item in activeItems.Where(i => i.Status == OrderItemStatus.Queued))
        {
            item.Status = OrderItemStatus.Queued;
            item.UpdatedAt = now;
            item.UpdatedBy = actorId;
        }
        order.UpdatedAt = now;
        order.UpdatedBy = actorId;

        order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
        {
            RestaurantId = restaurantId,
            OrderId = order.Id,
            FromStatus = OrderStatus.Open,
            ToStatus = OrderStatus.InPreparation,
            ChangedAt = now,
            ChangedBy = actorId,
            Reason = "Sent to kitchen",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        });

        var stations = activeItems
            .Select(i => i.StationCode)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var events = new List<DomainEvent>();
        if (stations.Count == 0)
        {
            events.Add(new OrderSentToKitchenEvent
            {
                RestaurantId = order.RestaurantId,
                BranchId = order.BranchId,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TableId = order.TableId,
                StationCode = null
            });
        }
        else
        {
            foreach (var stationCode in stations)
            {
                events.Add(new OrderSentToKitchenEvent
                {
                    RestaurantId = order.RestaurantId,
                    BranchId = order.BranchId,
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    TableId = order.TableId,
                    StationCode = stationCode
                });
            }
        }

        await orderRepository.UpdateAsync(order, events, ct);
        return Answer.Ok(message: localizer["SentToKitchen"]);
    }
}