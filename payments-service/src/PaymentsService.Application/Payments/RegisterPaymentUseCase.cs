using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Events;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.Payments;

public sealed class RegisterPaymentUseCase(
    IPaymentRepository paymentRepository,
    ICashSessionRepository cashSessionRepository,
    IStringLocalizer localizer)
{
    public async Task<Answer<RegisterPaymentResponse>> ExecuteAsync(
        long restaurantId, long branchId, long actorId, RegisterPaymentRequest request, CancellationToken ct = default)
    {
        var methodCode = request.MethodCode?.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(methodCode) || !PaymentMethods.Rules.TryGetValue(methodCode, out var rule))
            return Answer<RegisterPaymentResponse>.Fail("PAYMENT_METHOD_INVALID", localizer["PaymentMethodInvalid"]);

        if (request.TipAmount < 0 || request.TipAmount > request.Amount)
            return Answer<RegisterPaymentResponse>.Fail("TIP_GREATER_THAN_AMOUNT", localizer["TipGreaterThanAmount"]);

        if (request.OrderTotal <= 0)
            return Answer<RegisterPaymentResponse>.Fail("ORDER_TOTAL_INVALID", localizer["OrderTotalInvalid"]);

        if (rule.RequiresReference && string.IsNullOrWhiteSpace(request.Reference))
            return Answer<RegisterPaymentResponse>.Fail("REFERENCE_REQUIRED", localizer["ReferenceRequired"]);

        if (rule.RequiresAmountTendered && request.AmountTendered < request.Amount)
            return Answer<RegisterPaymentResponse>.Fail("INSUFFICIENT_CASH", localizer["CashTenderedInsufficient"]);

        long? cashSessionId = null;
        if (rule.RequiresCashSession)
        {
            CashSession? session = null;
            if (request.CashSessionId.HasValue)
            {
                session = await cashSessionRepository.GetByIdAsync(request.CashSessionId.Value, restaurantId, ct);
            }
            else
            {
                session = await cashSessionRepository.GetActiveAsync(branchId, restaurantId, ct);
            }
            
            if (session is null) return Answer<RegisterPaymentResponse>.Fail("NO_ACTIVE_SESSION", localizer["NoActiveSession"]);
            if (session.BranchId != branchId) return Answer<RegisterPaymentResponse>.Fail("NO_ACTIVE_SESSION", localizer["NoActiveSession"]);
            if (session.Status != CashSessionStatus.Open) return Answer<RegisterPaymentResponse>.Fail("SESSION_NOT_OPEN", localizer["SessionNotOpen"]);
            cashSessionId = session.Id;
        }

        var tendered = rule.RequiresAmountTendered ? request.AmountTendered : request.Amount;
        var change = tendered - request.Amount;

        var paidSoFar = await paymentRepository.SumCompletedByOrderAsync(request.OrderId, restaurantId, ct);
        var contribution = request.Amount - request.TipAmount;
        
        if (paidSoFar + contribution > request.OrderTotal)
            return Answer<RegisterPaymentResponse>.Fail("PAYMENT_EXCEEDS_BALANCE", localizer["PaymentExceedsBalance"]);

        var newBalance = request.OrderTotal - (paidSoFar + contribution);
        var issueTicket = newBalance <= 0;

        var now = DateTimeOffset.UtcNow;
        var payment = new Payment()
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId = branchId,
            OrderId = request.OrderId,
            CashSessionId = cashSessionId,
            PaymentMethodId = request.PaymentMethodId,
            MethodCode = methodCode,
            Amount = request.Amount,
            AmountTendered = tendered,
            ChangeGiven = change,
            TipAmount = request.TipAmount,
            Reference = request.Reference,
            Status = PaymentStatus.Completed,
            CollectedByUserId = request.CollectedByUserId ?? actorId,
            RecordedByUserId = actorId,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId
        };

        Ticket? ticket = null;
        if (issueTicket)
        {
            ticket = new Ticket()
            {
                PublicId = Guid.NewGuid(),
                RestaurantId = restaurantId,
                BranchId = branchId,
                OrderId = request.OrderId,
                TicketNumber = "",
                IssuedAt = now,
                CreatedAt = now,
                CreatedBy = actorId,
                UpdatedAt = now,
                UpdatedBy = actorId
            };
        }

        var evt = new PaymentRegisteredEvent()
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            PaymentPublicId = payment.PublicId,
            OrderId = request.OrderId,
            MethodCode = methodCode,
            Amount = request.Amount,
            TipAmount = request.TipAmount
        };

        var result = await paymentRepository.RegisterAsync(payment, ticket, evt, ct);
        
        return Answer<RegisterPaymentResponse>.Ok(
            new RegisterPaymentResponse(
                result.PaymentId, result.PublicId, request.OrderId, methodCode, request.Amount,
                tendered, change, request.TipAmount, payment.Status.ToString(),
                cashSessionId, result.TicketNumber, newBalance < 0 ? 0 : newBalance),
            localizer["PaymentCreated"]);
    }
}