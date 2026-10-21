using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReportingService.Domain.Entities;
using ReportingService.Infrastructure.Data;
using RestaurantPos.Shared.Abstractions.Events;
using RestaurantPos.Shared.Abstractions.Tenancy;
using RestaurantPos.Shared.Infrastructure.Messaging;
using RestaurantPos.Shared.Persistence;

namespace ReportingService.Infrastructure.Workers;

public sealed class ReportingConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<ReportingConsumer> logger)
    : RabbitMqConsumer(connectionProvider, options, logger)
{
    protected override string ConsumerName => "reporting";
    protected override IReadOnlyList<string> BindingPatterns => ["orders.order_closed", "payments.payment_registered"];

    protected override async Task HandleAsync(EventEnvelope envelope, CancellationToken ct)
    {
        if (envelope.RestaurantId is null) return;
        using var scope = scopeFactory.CreateScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        setter.SetTenant(envelope.RestaurantId.Value, envelope.BranchId);
        try
        {
            if (envelope.EventType == "orders.order_closed")
            {
                var p = JsonSerializer.Deserialize<OrderClosedPayload>(envelope.Data.GetRawText(), PosJson.Options);
                if (p is not null) await HandleOrderClosedAsync(scope, envelope, p, ct);
            }
            else if (envelope.EventType == "payments.payment_registered")
            {
                var p = JsonSerializer.Deserialize<PaymentRegisteredPayload>(envelope.Data.GetRawText(), PosJson.Options);
                if (p is not null) await HandlePaymentRegisteredAsync(scope, envelope, p, ct);
            }
        }
        finally { setter.Clear(); }
    }

    private static async Task HandleOrderClosedAsync(IServiceScope scope, EventEnvelope env, OrderClosedPayload p, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<ReportingServiceDbContext>();
        var date = DateOnly.FromDateTime(p.ClosedAt.UtcDateTime);

        // daily_sales upsert
        var ds = await db.DailySales.FirstOrDefaultAsync(x => x.RestaurantId == env.RestaurantId!.Value && x.BranchId == env.BranchId!.Value && x.BusinessDate == date, ct);
        if (ds is null)
        {
            ds = new DailySales { RestaurantId = env.RestaurantId!.Value, BranchId = env.BranchId!.Value, BusinessDate = date, ByPaymentMethod = "{}" };
            db.DailySales.Add(ds);
        }
        ds.TotalSales += p.Total;
        ds.TotalDiscount += p.DiscountAmount;
        ds.TotalTax += p.TaxAmount;
        ds.TotalTips += p.TipAmount;
        ds.OrderCount += 1;
        ds.UpdatedAt = DateTimeOffset.UtcNow;

        // daily_product_sales upsert
        foreach (var it in p.Items)
        {
            var existing = await db.DailyProductSales.FirstOrDefaultAsync(x =>
                x.RestaurantId == env.RestaurantId!.Value && x.BranchId == env.BranchId!.Value && x.BusinessDate == date && x.ProductName == it.Name, ct);
            if (existing is null)
            {
                db.DailyProductSales.Add(new DailyProductSales
                {
                    RestaurantId = env.RestaurantId!.Value, BranchId = env.BranchId!.Value, BusinessDate = date,
                    ProductName = it.Name, Quantity = it.Quantity, Total = it.LineTotal
                });
            }
            else
            {
                existing.Quantity += it.Quantity;
                existing.Total += it.LineTotal;
            }
        }

        // daily_waiter_sales upsert
        if (p.OpenedBy.HasValue)
        {
            var w = await db.DailyWaiterSales.FirstOrDefaultAsync(x =>
                x.RestaurantId == env.RestaurantId!.Value && x.BranchId == env.BranchId!.Value && x.BusinessDate == date && x.WaiterId == p.OpenedBy.Value, ct);
            if (w is null)
            {
                db.DailyWaiterSales.Add(new DailyWaiterSales
                {
                    RestaurantId = env.RestaurantId!.Value, BranchId = env.BranchId!.Value, BusinessDate = date,
                    WaiterId = p.OpenedBy.Value, Total = p.Total, Tips = p.TipAmount, OrderCount = 1
                });
            }
            else
            {
                w.Total += p.Total;
                w.Tips += p.TipAmount;
                w.OrderCount += 1;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task HandlePaymentRegisteredAsync(IServiceScope scope, EventEnvelope env, PaymentRegisteredPayload p, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<ReportingServiceDbContext>();
        var date = DateOnly.FromDateTime(DateTime.UtcNow); // payment date; ideally order's close date. Approx.
        // update by_payment_method on daily_sales if exists
        var ds = await db.DailySales.FirstOrDefaultAsync(x => x.RestaurantId == env.RestaurantId!.Value && x.BranchId == env.BranchId!.Value && x.BusinessDate == date, ct);
        if (ds is null) return; // will be created when order closes
        var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(ds.ByPaymentMethod, PosJson.Options) ?? new();
        dict[p.MethodCode] = dict.GetValueOrDefault(p.MethodCode) + (p.Amount - p.TipAmount);
        ds.ByPaymentMethod = JsonSerializer.Serialize(dict, PosJson.Options);
        ds.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record OrderClosedPayload(long OrderId, string OrderNumber, long? OpenedBy, decimal Total, decimal DiscountAmount, decimal TaxAmount, decimal TipAmount, DateTimeOffset ClosedAt, List<OrderClosedItemPayload> Items);
public sealed record OrderClosedItemPayload(string Name, decimal Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record PaymentRegisteredPayload(Guid PaymentPublicId, long OrderId, string MethodCode, decimal Amount, decimal TipAmount);
