namespace ReportingService.Domain.Entities;

public class DailyWaiterSales
{
    public long Id { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public long WaiterId { get; set; }
    public decimal Total { get; set; }
    public decimal Tips { get; set; }
    public int OrderCount { get; set; }
}
