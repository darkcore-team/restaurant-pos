using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Constants;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Events;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.Payments;

public sealed class CancelPaymentUseCase(
    IPaymentRepository paymentRepository,
    IStringLocalizer localizer)
{
    public async Task<Answer<PaymentDto>> ExecuteAsync(long paymentId, long restaurantId, long actorId, string? reason,
        CancellationToken ct = default)
    {
        var payment = await paymentRepository.GetByIdAsync(paymentId, restaurantId, ct);
        
        if (payment is null)
            return Answer<PaymentDto>.Fail("PAYMENT_NOT_FOUND", localizer["PaymentNotFound"]);

        if (payment.Status != PaymentStatus.Completed)
            return Answer<PaymentDto>.Fail("PAYMENT_NOT_CANCELABLE", localizer["PaymentNotCancelable"]);

        var now = DateTimeOffset.UtcNow;
        payment.Status = PaymentStatus.Cancelled;
        payment.UpdatedAt = now;
        payment.UpdatedBy = actorId;
        if (!string.IsNullOrWhiteSpace(reason)) payment.Reference = (payment.Reference ?? "") + $" [CANCEL: {reason}]";

        var evt = new PaymentCancelledEvent()
        {
            RestaurantId = restaurantId,
            BranchId = payment.BranchId,
            PaymentPublicid = payment.PublicId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            TipAmount = payment.TipAmount
        };

        await paymentRepository.CancelAsync(payment, evt, ct);

        var dto = ToDto(payment);
        return Answer<PaymentDto>.Ok(dto, localizer["PaymentCancelled"]);
    }

    private static PaymentDto ToDto(Payment p) => new PaymentDto(
        p.Id, p.PublicId, p.OrderId, p.MethodCode, p.Amount, p.AmountTendered,
        p.ChangeGiven, p.TipAmount, p.Reference, p.Status, p.CashSessionId,
        p.CollectedByUserId, p.CreatedAt);
}