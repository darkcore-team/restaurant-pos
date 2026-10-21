namespace ReportingService.Domain.Entities;

public class DailyProductSales
{
    public long Id { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }
}
