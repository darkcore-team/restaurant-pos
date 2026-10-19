using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace OrdersService.Application.Orders;

public sealed class OpenOrderUseCase(IOrderRepository orderRepository, IStringLocalizer localizer)
{
    public async Task<Answer<CreateOrderResponse>> ExecuteAsync(
        long restaurantId,
        long branchId,
        long actorId,
        CreateOrderRequest request,
        CancellationToken ct)
    {
        if (request.OrderType == OrderType.DineIn && request.TableId is null)
            return Answer<CreateOrderResponse>.Fail("TABLE_REQUIRED", localizer["TableRequired"]);

        if (request.OrderType == OrderType.DineIn && request.TableId.HasValue)
        {
            if (await orderRepository.HasOpenOrderForTableAsync(request.TableId.Value, restaurantId, ct))
                return Answer<CreateOrderResponse>.Fail("TABLE_HAS_OPEN_ORDER", localizer["TableHasOpenOrder"]);
        }

        var next = await orderRepository.CountByBranchAsync(branchId, ct) + 1;
        var orderNumber = $"ORD-{branchId:D3}-{next:D4}";

        var now = DateTimeOffset.UtcNow;
        var order = new Order
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            TableId = request.TableId,
            OrderNumber = orderNumber,
            OrderType = request.OrderType,
            Status = OrderStatus.Open,
            CurrencyId = request.CurrencyId,
            ExchangeRate = 1m,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Covers = request.Covers,
            Notes = request.Notes,
            OpenedBy = actorId,
            OpenedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        };

        order.StatusHistory.Add(new OrderStatusHistory
        {
            RestaurantId = restaurantId,
            FromStatus = null,
            ToStatus = OrderStatus.Open,
            ChangedAt = now,
            ChangedBy = actorId,
            Reason = "Order opened",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId
        });

        try
        {
            await orderRepository.CreateAsync(order, ct);
        }
        catch (DbUpdateException)
        {
            return Answer<CreateOrderResponse>.Fail("ORDER_CREATE_FAILED", localizer["OrderCreateFailed"]);
        }

        return Answer<CreateOrderResponse>.Ok(new CreateOrderResponse(order.Id, order.OrderNumber), localizer["OrderCreated"]);
    }
}