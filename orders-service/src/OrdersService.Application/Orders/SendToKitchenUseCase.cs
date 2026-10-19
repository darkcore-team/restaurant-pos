using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

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

        await orderRepository.UpdateAsync(order, ct);
        return Answer.Ok(message: localizer["SentToKitchen"]);
    }
}