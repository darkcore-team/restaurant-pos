using OrdersService.Domain.Constants;

namespace OrdersService.Domain.Entities;

public class OrderStatusHistory
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public long? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    public Order Order { get; set; } = null!;
}