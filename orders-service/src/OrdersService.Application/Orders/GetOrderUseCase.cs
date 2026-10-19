using RestaurantPos.Shared.Abstractions.Common;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders;

public sealed class GetOrderUseCase(IOrderRepository orderRepository)
{
    public async Task<Answer<OrderDto>> ExecuteAsync(long id, long restaurantId, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(id, restaurantId, ct);
        if (order is null)
            return Answer<OrderDto>.Fail("ORDER_NOT_FOUND", "Orden no encontrada");

        var dto = new OrderDto(
            order.Id,
            order.PublicId,
            order.OrderNumber,
            order.OrderType.ToString(),
            order.Status.ToString(),
            order.TableId,
            order.BranchId,
            order.CurrencyId,
            order.ExchangeRate,
            order.CustomerName,
            order.CustomerPhone,
            order.Covers,
            order.Subtotal,
            order.DiscountAmount,
            order.TaxAmount,
            order.Total,
            order.TipAmount,
            order.PaidAmount,
            order.BalanceDue,
            order.OpenedBy,
            order.OpenedAt,
            order.ClosedBy,
            order.ClosedAt,
            order.Notes,
            order.Items.OrderBy(i => i.Id).Select(i => new OrderItemDto(
                i.Id, 
                i.PublicId, 
                i.ItemId, 
                i.StationId, 
                i.StationCode, 
                i.Status.ToString(),
                i.Name,
                i.Description,
                i.UnitPrice,
                i.Quantity,
                i.ModifiersTotal,
                i.DiscountAmount,
                i.DiscountReason,
                i.TaxId,
                i.TaxRate,
                i.TaxAmount,
                i.IsTaxExempt,
                i.TaxInclusive,
                i.LineTotal,
                i.Notes,
                i.VoidedBy,
                i.VoidedAt,
                i.VoidReason,
                i.Modifiers.Select(m => new OrderItemModifierDto(
                    m.Id,
                    m.PublicId,
                    m.ModifierOptionId,
                    m.ModifierGroupId,
                    m.Name,
                    m.PriceDelta,
                    m.Notes)).ToList()
            )).ToList()
        );

        return Answer<OrderDto>.Ok(dto);
    }
}