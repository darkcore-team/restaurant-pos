using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Application.Orders;

public sealed class RequestBillUseCase(IOrderRepository orderRepository, IStringLocalizer localizer)
{
    public async Task<Answer> ExecuteAsync(long orderId, long restaurantId, long actorId, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(orderId, restaurantId, ct);
        if (order is null)
            return Answer.Fail("ORDER_NOT_FOUND", localizer["OrderNotFound"]);

        if (order.Status is OrderStatus.Closed or OrderStatus.Cancelled or OrderStatus.Billing)
            return Answer.Fail("INVALID_STATUS", localizer["CannotRequestBill"]);

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;
        order.Status = OrderStatus.Billing;
        order.UpdatedAt = now;
        order.UpdatedBy = actorId;

        order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
        {
            RestaurantId = restaurantId,
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Billing,
            ChangedAt = now,
            ChangedBy = actorId,
            Reason = "Bill requested",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        });

        await orderRepository.UpdateAsync(order, ct);
        return Answer.Ok(message: localizer["BillRequested"]);
    }
}