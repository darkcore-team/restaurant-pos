using OrdersService.Domain.Constants;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Application.Orders;

public sealed class CancelOrderUseCase(IOrderRepository orderRepository)
{
    public async Task<Answer> ExecuteAsync(long orderId, long restaurantId, long actorId, string? reason, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(orderId, restaurantId, ct);
        
        if (order is null)
            return Answer.Fail("ORDER_NOT_FOUND", "Orden no encontrada");

        if (order.Status is OrderStatus.Closed or OrderStatus.Cancelled)
            return Answer.Fail("INVALID_STATUS", "La orden ya está cerrada/cancelada");

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;
        order.Status = OrderStatus.Cancelled;
        
        foreach (var item in order.Items.Where(i => i.VoidedAt == null))
        {
            item.Status = OrderItemStatus.Cancelled;
            item.VoidedBy = actorId;
            item.VoidedAt = now;
            item.VoidReason = reason ?? "Order cancelled";
            item.UpdatedAt = now;
            item.UpdatedBy = actorId;
        }
        order.UpdatedAt = now;
        order.UpdatedBy = actorId;

        order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
        {
            RestaurantId = restaurantId,
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Cancelled,
            ChangedAt = now,
            ChangedBy = actorId,
            Reason = reason ?? "Order cancelled",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        });

        await orderRepository.UpdateAsync(order, ct);
        return Answer.Ok(message: "Orden cancelada");
    }
}