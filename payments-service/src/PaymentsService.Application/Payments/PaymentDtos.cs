using System.ComponentModel.DataAnnotations;
using PaymentsService.Domain.Constants;

namespace PaymentsService.Application.Payments;

public class RegisterPaymentRequest
{
    [Required]
    public long OrderId { get; set; }
    [Required]
    public long PaymentMethodId { get; set; }
    [Required]
    public string MethodCode { get; set; } = null!;
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal TipAmount { get; set; } = 0m;
    public string? Reference { get; set; }
    public long? CashSessionId { get; set; }
    public long? CollectedByUserId { get; set; }
    [Required, Range(0.01, double.MaxValue)]
    public decimal OrderTotal { get; set; }
}

public class CancelPaymentRequest
{
    public string? Reason { get; set; }
}

public sealed record PaymentDto(
    long Id,Guid PublicId, long OrderId, string MethodCode, decimal Amount,
    decimal AmountTendered, decimal ChangeGiven, decimal TipAmount, string? Reference,
    PaymentStatus Status, long? CashSessionId, long CollectedByUserId, DateTimeOffset CreatedAt);

public sealed record RegisterPaymentResponse(
    long PaymentId, Guid PublicId, long OrderId, string MethodCode, decimal Amount,
    decimal AmountTendered, decimal ChangeGiven, decimal TipAmount, string Status,
    long? CashSessionId, string? TicketNumber, decimal BalanceDue);

public sealed record TicketDto(long Id, Guid PublicId, string TicketNumber, long OrderId, long BranchId, DateTimeOffset IssuedAt);