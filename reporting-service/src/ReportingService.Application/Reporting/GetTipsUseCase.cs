
namespace ReportingService.Application.Reporting;

public sealed class GetTipsUseCase(IReportingReadRepository repo)
{
    public Task<TipsDto?> ExecuteAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default)
        => repo.GetTipsAsync(restaurantId, branchId, date, ct);
}
