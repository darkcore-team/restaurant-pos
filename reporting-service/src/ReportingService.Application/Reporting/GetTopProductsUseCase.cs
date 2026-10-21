
namespace ReportingService.Application.Reporting;

public sealed class GetTopProductsUseCase(IReportingReadRepository repo)
{
    public Task<IReadOnlyList<TopProductDto>> ExecuteAsync(long restaurantId, long branchId, DateOnly date, int limit, CancellationToken ct = default)
        => repo.GetTopProductsAsync(restaurantId, branchId, date, limit, ct);
}
