using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Application.Orders;

public sealed class UpdateItemStatusUseCase(IOrderRepository orderRepository, IStringLocalizer localizer)
{
    public async Task<Answer> ExecuteAsync(
        long orderId,
        long itemId,
        long restaurantId,
        long actorId,
        string newStatus,
        CancellationToken ct)
    {
        if (!Enum.TryParse<OrderItemStatus>(newStatus, true, out var parsed))
            return Answer.Fail("INVALID_STATUS", localizer["InvalidItemStatus"]);

        var order = await orderRepository.GetByIdAsync(orderId, restaurantId, ct);
        if (order is null)
            return Answer.Fail("ORDER_NOT_FOUND", localizer["OrderNotFound"]);

        var item = order.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return Answer.Fail("ITEM_NOT_FOUND", localizer["ItemNotFound"]);

        if (item.VoidedAt.HasValue)
            return Answer.Fail("ITEM_VOIDED", localizer["ItemVoidedStatus"]);

        var now = DateTimeOffset.UtcNow;
        item.Status = parsed;
        item.UpdatedAt = now;
        item.UpdatedBy = actorId;
        
        var active = order.Items.Where(i => i.VoidedAt == null && i.Status != OrderItemStatus.Cancelled).ToList();
        var previousStatus = order.Status;
        
        if (active.Count > 0 && active.All(i => i.Status == OrderItemStatus.Ready || i.Status == OrderItemStatus.Served))
        {
            if (order.Status == OrderStatus.InPreparation) order.Status = OrderStatus.Ready;
        }
        else if (active.Any(i => i.Status == OrderItemStatus.InPreparation))
        {
            if (order.Status == OrderStatus.Ready) order.Status = OrderStatus.InPreparation;
        }

        if (order.Status != previousStatus)
        {
            order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
            {
                RestaurantId = restaurantId,
                OrderId = order.Id,
                FromStatus = previousStatus,
                ToStatus = order.Status,
                ChangedAt = now,
                ChangedBy = actorId,
                Reason = "Derived from item status",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = actorId,
                UpdatedBy = actorId
            });
        }

        order.UpdatedAt = now;
        order.UpdatedBy = actorId;
        await orderRepository.UpdateAsync(order, ct);
        return Answer.Ok(message: localizer["ItemStatusUpdated"]);
    }
}