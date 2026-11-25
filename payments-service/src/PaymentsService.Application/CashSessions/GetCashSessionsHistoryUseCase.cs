using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.CashSessions;

public sealed class GetCashSessionsHistoryUseCase(ICashSessionRepository sessionRepository)
{
    public async Task<Answer<IReadOnlyList<CashSessionDto>>> ExecuteAsync(
        long restaurantId, long branchId, int limit = 50, CancellationToken ct = default)
    {
        var sessions = await sessionRepository.GetHistoryAsync(branchId, restaurantId, limit, ct);
        var dtos = sessions.Select(s => CashSessionDto.From(s, s.ExpectedAmount)).ToList();
        return Answer<IReadOnlyList<CashSessionDto>>.Ok(dtos);
    }
}
