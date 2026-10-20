using RestaurantPos.Shared.Abstractions.Events;

namespace PaymentsService.Domain.Events;

public record PaymentRegisteredEvent : DomainEvent
{
    public override string EventType => "payments.payment_registered";
    public Guid PaymentPublicId { get; init; }
    public long OrderId { get; init; }
    public string MethodCode { get; init; } = null!;
    public decimal Amount { get; init; }
    public decimal TipAmount { get; init; }
}