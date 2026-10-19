using OrdersService.Domain.Entities;

namespace OrdersService.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task CreateAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task<bool> HasOpenOrderForTableAsync(long tableId, long restaurantId, CancellationToken ct = default);
    Task<int> CountByBranchAsync(long branchId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderItem>> GetActiveKdsItemsAsync(string stationCode, long restaurantId, long branchId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetActiveOrdersAsync(long restaurantId, long branchId, CancellationToken ct = default);
}