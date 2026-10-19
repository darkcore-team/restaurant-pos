using OrdersService.Domain.Constants;

namespace OrdersService.Domain.Entities;

public class OrderItemModifier
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long OrderItemId { get; set; }
    public long? ModifierOptionId { get; set; }
    public long? ModifierGroupId { get; set; }
    public string Name { get; set; } = null!;
    public decimal PriceDelta { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    public OrderItem OrderItem { get; set; } = null!;
}