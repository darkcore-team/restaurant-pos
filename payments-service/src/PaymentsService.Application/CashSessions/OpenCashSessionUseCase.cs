using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.CashSessions;

public class OpenCashSessionUseCase(
    ICashRegisterRepository cashRegisterRepository,
    ICashSessionRepository cashSessionRepository,
    IStringLocalizer localizer)
{
    public async Task<Answer<OpenCashSessionResponse>> ExecuteAsync(
        long restaurantId,
        long branchId,
        long actorId,
        OpenCashSessionRequest request,
        CancellationToken ct = default)
    {
        var register = await cashRegisterRepository.GetByIdAsync(request.CashRegisterId, restaurantId, ct);

        if (register is null || register.BranchId != branchId)
            return Answer<OpenCashSessionResponse>.Fail("CASH_REGISTER_NOT_FOUND", localizer["CashRegisterNotFound"]);

        if (!register.IsActive)
            return Answer<OpenCashSessionResponse>.Fail("CASH_REGISTER_INACTIVE", localizer["CashRegisterInactive"]);

        if (await cashSessionRepository.HasOpenSessionForRegisterAsync(register.Id, restaurantId, ct))
            return Answer<OpenCashSessionResponse>.Fail("SESSION_ALREADY_OPEN", localizer["SessionAlreadyOpen"]);

        var now = DateTimeOffset.UtcNow;
        var session = new CashSession()
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            CashRegisterId = register.Id,
            OpenedBy = actorId,
            OpenedAt = now,
            OpeningFloat = request.OpeningFloat,
            ExpectedAmount = request.OpeningFloat,
            Status = CashSessionStatus.Open,
            Notes = request.Notes,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId
        };

        await cashSessionRepository.CreateAsync(session, ct);

        return Answer<OpenCashSessionResponse>.Ok(
            new OpenCashSessionResponse(session.Id, session.CashRegisterId, session.OpeningFloat, session.OpenedAt),
            localizer["SessionOpened"]);
    }
}