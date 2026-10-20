using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.CashSessions;

public class CloseCashSessionUseCase(
    ICashSessionRepository cashSessionRepository,
    IPaymentRepository paymentRepository,
    IStringLocalizer localizer)
{
    public async Task<Answer<CloseCashSessionResponse>> ExecuteAsync(
        long restaurantId,
        long branchId,
        long actorId,
        long sessionId,
        CloseCashSessionRequest request,
        CancellationToken ct)
    {
        var session = await cashSessionRepository.GetByIdAsync(sessionId, restaurantId, ct);
        
        if (session is null || session.BranchId != branchId)
            return Answer<CloseCashSessionResponse>.Fail("SESSION_NOT_FOUND", localizer["SessionNotFound"]);

        if (session.Status != CashSessionStatus.Open)
            return Answer<CloseCashSessionResponse>.Fail("SESSION_ALREADY_CLOSED", localizer["SessionAlreadyClosed"]);

        var keptCash = await paymentRepository.SumKeptBySessionAsync(session.Id, ct);
        var expected = Math.Round(session.OpeningFloat = keptCash, 2);
        var difference = Math.Round(request.CountedAmount - expected, 2);

        var now = DateTimeOffset.UtcNow;
        session.ExpectedAmount = expected;
        session.CountedAmount = request.CountedAmount;
        session.Difference = difference;
        session.Status = CashSessionStatus.Closed;
        session.ClosedBy = actorId;
        session.ClosedAt = now;
        if (request.Notes is not null) session.Notes = request.Notes;
        session.UpdatedAt = now;
        session.UpdatedBy = actorId;

        await cashSessionRepository.UpdateAsync(session, ct);
        
        return Answer<CloseCashSessionResponse>.Ok(
            new CloseCashSessionResponse(session.Id, session.ExpectedAmount, session.CountedAmount.Value,
            session.Difference.Value, session.Status.ToString()), localizer["SessionClosed"]);
    }
}