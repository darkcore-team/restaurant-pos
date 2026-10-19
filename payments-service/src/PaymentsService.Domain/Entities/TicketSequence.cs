namespace PaymentsService.Domain.Entities;

public class TicketSequence
{
    public long Id { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long LastNumber { get; set; }
}