namespace OrdersService.Domain.Entities;

public class OrderPayment
{
    public Guid PaymentPublicId { get; set; }
    public long RestaurantId { get; set; }
    public long OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal TipAmount { get; set; }
    public string Status { get; set; } = "COMPLETED";
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
