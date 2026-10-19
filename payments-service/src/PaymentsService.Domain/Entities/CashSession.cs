using PaymentsService.Domain.Constants;

namespace PaymentsService.Domain.Entities;

public class CashSession
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long CashRegisterId { get; set; }
    public long OpenedBy { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public long? ClosedBy { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal? CountedAmount { get; set; }
    public decimal? Difference { get; set; }
    public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    public CashRegister CashRegister { get; set; } = null!;
}