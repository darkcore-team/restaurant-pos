using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Application.Orders;

public sealed class AddItemsUseCase(IOrderRepository orderRepository, IStringLocalizer localizer)
{
    public async Task<Answer<IReadOnlyList<long>>> ExecuteAsync(
        long orderId,
        long restaurantId,
        long actorId,
        AddItemsRequest request,
        CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(orderId, restaurantId, ct);

        if (order is null)
            return Answer<IReadOnlyList<long>>.Fail("ORDER_NOT_FOUND", localizer["OrderNotFound"]);

        if (order.Status is OrderStatus.Closed or OrderStatus.Cancelled or OrderStatus.Billing)
            return Answer<IReadOnlyList<long>>.Fail("ORDER_NOT_OPEN", localizer["OrderNotOpen"]);

        var now = DateTimeOffset.UtcNow;
        var newItems = new List<OrderItem>();

        foreach (var line in request.Items)
        {
            var modifiersTotal = line.Modifiers?.Sum(m => m.PriceDelta) ?? 0m;
            var taxRate = line.IsTaxExempt ? 0m : (line.TaxRate ?? 0m);

            var item = new OrderItem
            {
                RestaurantId = restaurantId,
                OrderId = order.Id,
                ItemId = line.ItemId,
                StationId = line.StationId,
                StationCode = line.StationCode,
                Status = OrderItemStatus.Queued,
                Name = line.Name,
                Description = line.Description,
                UnitPrice = line.UnitPrice,
                Quantity = line.Quantity <= 0 ? 1m : line.Quantity,
                ModifiersTotal = modifiersTotal,
                DiscountAmount = 0m,
                TaxId = line.TaxId,
                TaxRate = taxRate,
                TaxAmount = 0m,
                IsTaxExempt = line.IsTaxExempt,
                TaxInclusive = false,
                Notes = line.Notes,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = actorId,
                UpdatedBy = actorId
            };

            var lineSubtotal = (item.UnitPrice + item.ModifiersTotal) * item.Quantity - item.DiscountAmount;
            item.TaxAmount = item.IsTaxExempt ? 0m : Math.Round(lineSubtotal * taxRate, 2);

            if (line.Modifiers is not null)
            {
                foreach (var m in line.Modifiers)
                {
                    item.Modifiers.Add(new OrderItemModifier
                    {
                        RestaurantId = restaurantId,
                        OrderItem = item,
                        ModifierOptionId = m.ModifierOptionId,
                        ModifierGroupId = m.ModifierGroupId,
                        Name = m.Name,
                        PriceDelta = m.PriceDelta,
                        Notes = m.Notes,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = actorId,
                        UpdatedBy = actorId
                    });
                }
            }

            order.Items.Add(item);
            newItems.Add(item);
        }

        RecalculateTotals(order);

        try
        {
            await orderRepository.UpdateAsync(order, ct);
        }
        catch (DbUpdateException)
        {
            return Answer<IReadOnlyList<long>>.Fail("ADD_ITEMS_FAILED", localizer["AddItemsFailed"]);
        }

        var ids = newItems.Select(i => i.Id).ToList();
        return Answer<IReadOnlyList<long>>.Ok(ids, localizer["ItemsAdded"]);
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
    }
}