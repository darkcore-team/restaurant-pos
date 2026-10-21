
namespace ReportingService.Application.Reporting;

public sealed class GetByWaiterUseCase(IReportingReadRepository repo)
{
    public Task<IReadOnlyList<WaiterSalesDto>> ExecuteAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default)
        => repo.GetByWaiterAsync(restaurantId, branchId, date, ct);
}
