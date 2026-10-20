using PaymentsService.Domain.Constants;

namespace PaymentsService.Domain.Entities;

public class Ticket
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long OrderId { get; set; }
    public string TicketNumber { get; set; } = null!;
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}