
namespace ReportingService.Application.Reporting;

public sealed class GetDailySalesUseCase(IReportingReadRepository repo)
{
    public Task<DailySalesDto?> ExecuteAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default)
        => repo.GetDailySalesAsync(restaurantId, branchId, date, ct);
}
