using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Application.Orders;

public sealed class VoidItemUseCase(IOrderRepository orderRepository, IStringLocalizer localizer)
{
    public async Task<Answer> ExecuteAsync(
        long orderId,
        long itemId,
        long restaurantId,
        long actorId,
        string reason,
        CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(orderId, restaurantId, ct);
        if (order is null)
            return Answer.Fail("ORDER_NOT_FOUND", localizer["OrderNotFound"]);

        if (order.Status is OrderStatus.Closed or OrderStatus.Cancelled)
            return Answer.Fail("ORDER_CLOSED", localizer["OrderClosed"]);

        var item = order.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
            return Answer.Fail("ITEM_NOT_FOUND", localizer["ItemNotFound"]);

        if (item.VoidedAt.HasValue)
            return Answer.Fail("ALREADY_VOIDED", localizer["AlreadyVoided"]);

        var now = DateTimeOffset.UtcNow;
        item.Status = OrderItemStatus.Cancelled;
        item.VoidedBy = actorId;
        item.VoidedAt = now;
        item.VoidReason = reason;
        item.UpdatedAt = now;
        item.UpdatedBy = actorId;

        RecalculateTotals(order);
        await orderRepository.UpdateAsync(order, ct);
        return Answer.Ok(message: localizer["ItemVoided"]);
    }

    private static void RecalculateTotals(Order order)
    {
        var subtotal = 0m;
        var tax = 0m;
        foreach (var i in order.Items.Where(i => i.VoidedAt == null))
        {
            var lineSub = (i.UnitPrice + i.ModifiersTotal) * i.Quantity - i.DiscountAmount;
            subtotal += lineSub;
            tax += i.TaxAmount;
        }
        order.Subtotal = Math.Round(subtotal, 2);
        order.TaxAmount = Math.Round(tax, 2);
        order.Total = Math.Round(order.Subtotal + order.TaxAmount - order.DiscountAmount + order.TipAmount, 2);
        order.BalanceDue = Math.Round(order.Total - order.PaidAmount, 2);
        order.UpdatedAt = DateTimeOffset.UtcNow;
    }
}