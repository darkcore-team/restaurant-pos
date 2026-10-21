using System.Text.Json;

namespace ReportingService.Domain.Entities;

public class DailySales
{
    public long Id { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalTips { get; set; }
    public int OrderCount { get; set; }
    public string ByPaymentMethod { get; set; } = "{}";
    public DateTimeOffset UpdatedAt { get; set; }
}
