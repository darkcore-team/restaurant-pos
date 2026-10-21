using ReportingService.Application.Reporting;

namespace ReportingService.Application.Reporting;

public interface IReportingReadRepository
{
    Task<DailySalesDto?> GetDailySalesAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(long restaurantId, long branchId, DateOnly date, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<WaiterSalesDto>> GetByWaiterAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default);
    Task<TipsDto?> GetTipsAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default);
}
