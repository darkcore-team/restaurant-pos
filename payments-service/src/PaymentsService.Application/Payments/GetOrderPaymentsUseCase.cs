using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.Payments;

public sealed class GetOrderPaymentsUseCase(IPaymentRepository paymentRepository)
{
    public async Task<Answer<IReadOnlyList<PaymentDto>>> ExecuteAsync(long orderId, long restaurantId,
        CancellationToken ct = default)
    {
        var payments = await paymentRepository.GetByOrderAsync(orderId, restaurantId, ct);
        
        var dtos = payments.Select(p => new PaymentDto(
            p.Id, p.PublicId, p.OrderId, p.MethodCode, p.Amount, p.AmountTendered,
            p.ChangeGiven, p.TipAmount, p.Reference, p.Status, p.CashSessionId,
            p.CollectedByUserId, p.CreatedAt)).ToList();
        
        return Answer<IReadOnlyList<PaymentDto>>.Ok(dtos);
    }
}