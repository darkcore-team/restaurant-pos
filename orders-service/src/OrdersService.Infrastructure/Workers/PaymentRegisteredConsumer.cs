using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Events;
using OrdersService.Domain.Entities;
using OrdersService.Infrastructure.Data;
using RestaurantPos.Shared.Abstractions.Events;
using RestaurantPos.Shared.Abstractions.Tenancy;
using RestaurantPos.Shared.Infrastructure.Messaging;
using RestaurantPos.Shared.Persistence.Outbox;

namespace OrdersService.Infrastructure.Workers;

public sealed class PaymentRegisteredConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentRegisteredConsumer> logger)
    : RabbitMqConsumer(connectionProvider, options, logger)
{
    protected override string ConsumerName => "orders-payments";
    protected override IReadOnlyList<string> BindingPatterns => ["payments.payment_registered", "payments.payment_cancelled"];

    protected override async Task HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
        if (envelope.RestaurantId is null) return;

        using var scope = scopeFactory.CreateScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        setter.SetTenant(envelope.RestaurantId.Value, envelope.BranchId);
        try
        {
            if (envelope.EventType == "payments.payment_registered")
            {
                var payload = JsonSerializer.Deserialize<PaymentRegisteredPayload>(envelope.Data.GetRawText(), PosJson.Options);
                if (payload is not null) await HandleRegisteredAsync(scope, envelope, payload, ct);
            }
            else if (envelope.EventType == "payments.payment_cancelled")
            {
                var payload = JsonSerializer.Deserialize<PaymentCancelledPayload>(envelope.Data.GetRawText(), PosJson.Options);
                if (payload is not null) await HandleCancelledAsync(scope, envelope, payload, ct);
            }
        }
        finally
        {
            setter.Clear();
        }
    }

    private static async Task HandleRegisteredAsync(IServiceScope scope, EventEnvelope envelope, PaymentRegisteredPayload p, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<OrdersServiceDbContext>();

        var already = await db.OrderPayments.AnyAsync(x => x.PaymentPublicId == p.PaymentPublicId, ct);
        if (already) return;

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == p.OrderId && o.RestaurantId == envelope.RestaurantId!.Value, ct);
        if (order is null) return;

        db.OrderPayments.Add(new OrderPayment
        {
            PaymentPublicId = p.PaymentPublicId,
            RestaurantId = envelope.RestaurantId!.Value,
            OrderId = p.OrderId,
            Amount = p.Amount,
            TipAmount = p.TipAmount,
            Status = "COMPLETED",
            OccurredOn = envelope.OccurredOn,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await RecomputeAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task HandleCancelledAsync(IServiceScope scope, EventEnvelope envelope, PaymentCancelledPayload p, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<OrdersServiceDbContext>();

        var row = await db.OrderPayments.FirstOrDefaultAsync(x => x.PaymentPublicId == p.PaymentPublicId, ct);
        if (row is null) return;
        if (row.Status == "CANCELLED") return;

        row.Status = "CANCELLED";
        await db.SaveChangesAsync(ct);
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == p.OrderId && o.RestaurantId == envelope.RestaurantId!.Value, ct);
        if (order is null) return;

        await RecomputeAsync(db, order, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task RecomputeAsync(OrdersServiceDbContext db, Order order, CancellationToken ct)
    {
        var paid = await db.OrderPayments
            .Where(x => x.OrderId == order.Id && x.Status == "COMPLETED")
            .SumAsync(x => x.Amount - x.TipAmount, ct);

        order.PaidAmount = paid;
        order.BalanceDue = order.Total - paid;
        if (order.BalanceDue < 0) order.BalanceDue = 0;

        if (order.BalanceDue <= 0
            && order.Status is not OrderStatus.Closed and not OrderStatus.Cancelled)
        {
            var prev = order.Status;
            order.Status = OrderStatus.Closed;
            order.ClosedAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                RestaurantId = order.RestaurantId,
                OrderId = order.Id,
                FromStatus = prev,
                ToStatus = OrderStatus.Closed,
                ChangedAt = DateTimeOffset.UtcNow,
                ChangedBy = null,
                Reason = "Payment completed",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            db.AddDomainEvent(new OrderClosedEvent
            {
                RestaurantId = order.RestaurantId,
                BranchId = order.BranchId,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OpenedBy = order.OpenedBy,
                Total = order.Total,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                TipAmount = order.TipAmount,
                ClosedAt = order.ClosedAt!.Value,
                Items = order.Items.Select(i => new OrderClosedItem(i.Name, i.Quantity, i.UnitPrice, i.LineTotal)).ToList()
            });
        }
    }
}

public sealed record PaymentRegisteredPayload(Guid PaymentPublicId, long OrderId, string MethodCode, decimal Amount, decimal TipAmount);
public sealed record PaymentCancelledPayload(Guid PaymentPublicId, long OrderId, decimal Amount, decimal TipAmount);
