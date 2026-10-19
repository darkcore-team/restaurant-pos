using PaymentsService.Domain.Constants;

namespace PaymentsService.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long OrderId { get; set; }
    public long? CashSessionId { get; set; }
    public long PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeGiven { get; set; }
    public decimal TipAmount { get; set; }
    public string? Reference { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public long CollectedByUserId { get; set; }
    public long? RecordedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}