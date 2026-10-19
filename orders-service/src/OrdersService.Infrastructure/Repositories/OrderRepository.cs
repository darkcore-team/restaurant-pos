using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Constants;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Repositories;
using OrdersService.Infrastructure.Data;

namespace OrdersService.Infrastructure.Repositories;

public sealed class OrderRepository(OrdersServiceDbContext db) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default)
        => await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId, ct);

    public async Task CreateAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> HasOpenOrderForTableAsync(long tableId, long restaurantId, CancellationToken ct = default)
        => await db.Orders.AnyAsync(o => o.TableId == tableId && o.RestaurantId == restaurantId
            && (o.Status == OrderStatus.Open || o.Status == OrderStatus.InPreparation
                || o.Status == OrderStatus.Ready || o.Status == OrderStatus.Served
                || o.Status == OrderStatus.Billing), ct);

    public async Task<int> CountByBranchAsync(long branchId, CancellationToken ct = default)
        => await db.Orders.CountAsync(o => o.BranchId == branchId, ct);

    public async Task<IReadOnlyList<OrderItem>> GetActiveKdsItemsAsync(string stationCode, long restaurantId, long branchId, CancellationToken ct = default)
        => await db.OrderItems
            .AsNoTracking()
            .Include(i => i.Order)
            .Include(i => i.Modifiers)
            .Where(i => i.StationCode == stationCode
                && i.RestaurantId == restaurantId
                && i.Order.BranchId == branchId
                && i.VoidedAt == null
                && i.Status != OrderItemStatus.Cancelled
                && i.Order.Status != OrderStatus.Closed
                && i.Order.Status != OrderStatus.Cancelled)
            .OrderBy(i => i.Order.OpenedAt)
            .ThenBy(i => i.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetActiveOrdersAsync(long restaurantId, long branchId, CancellationToken ct = default)
        => await db.Orders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .Where(o => o.RestaurantId == restaurantId && o.BranchId == branchId
                && (o.Status == OrderStatus.Open || o.Status == OrderStatus.InPreparation
                    || o.Status == OrderStatus.Ready || o.Status == OrderStatus.Served
                    || o.Status == OrderStatus.Billing))
            .OrderByDescending(o => o.OpenedAt)
            .ToListAsync(ct);
}