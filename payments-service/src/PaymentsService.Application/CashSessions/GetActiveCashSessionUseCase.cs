using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.CashSessions;

public class GetActiveCashSessionUseCase(
    ICashSessionRepository cashSessionRepository,
    IPaymentRepository paymentRepository,
    IStringLocalizer localizer)
{
    public async Task<Answer<CashSessionDto>> ExecuteAsync(long restaurantId, long branchId, CancellationToken ct)
    {
        var session = await cashSessionRepository.GetActiveAsync(branchId, restaurantId, ct);
        if (session is null)
            return Answer<CashSessionDto>.Fail("NO_ACTIVE_SESSION", localizer["NoActiveSession"]);

        var keptCash = await paymentRepository.SumKeptBySessionAsync(session.Id, ct);
        var expected = Math.Round(session.OpeningFloat + keptCash, 2);

        return Answer<CashSessionDto>.Ok(CashSessionDto.From(session, expected), localizer["Ok"]);
    }
}