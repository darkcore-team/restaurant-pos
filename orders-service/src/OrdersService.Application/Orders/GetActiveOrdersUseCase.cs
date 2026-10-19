using RestaurantPos.Shared.Abstractions.Common;
using RestaurantPos.Shared.Abstractions.Tenancy;
using OrdersService.Domain.Repositories;

namespace OrdersService.Application.Orders;

public sealed class GetActiveOrdersUseCase(IOrderRepository orderRepository, ITenantContext tenantContext)
{
    public async Task<Answer<IReadOnlyList<OrderDto>>> ExecuteAsync(CancellationToken ct = default)
    {
        var restaurantId = tenantContext.RestaurantId;
        var branchId = tenantContext.BranchId;
        
        if (restaurantId is null || branchId is null)
            return Answer<IReadOnlyList<OrderDto>>.Fail("MISSING_TENANT", "Falta restaurant_id/branch_id");

        var orders = await orderRepository.GetActiveOrdersAsync(restaurantId.Value, branchId.Value, ct);

        var dtos = orders.Select(o => new OrderDto(
            o.Id,
            o.PublicId,
            o.OrderNumber,
            o.OrderType.ToString(),
            o.Status.ToString(),
            o.TableId,
            o.BranchId,
            o.CurrencyId,
            o.ExchangeRate,
            o.CustomerName,
            o.CustomerPhone,
            o.Covers,
            o.Subtotal,
            o.DiscountAmount,
            o.TaxAmount,
            o.Total,
            o.TipAmount,
            o.PaidAmount,
            o.BalanceDue,
            o.OpenedBy,
            o.OpenedAt,
            o.ClosedBy,
            o.ClosedAt,
            o.Notes,
            o.Items.OrderBy(i => i.Id).Select(i => new OrderItemDto(
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
        )).ToList();

        return Answer<IReadOnlyList<OrderDto>>.Ok(dtos);
    }
}