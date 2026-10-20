using RestaurantPos.Shared.Abstractions.Events;

namespace PaymentsService.Domain.Events;

public sealed record PaymentCancelledEvent : DomainEvent
{
    public override string EventType => "payments.payment_cancelled";
    public Guid PaymentPublicid { get; init; }
    public long OrderId { get; init; }
    public decimal Amount { get; init; }
    public decimal TipAmount { get; init; }
}