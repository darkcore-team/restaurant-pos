using OrdersService.Domain.Constants;

namespace OrdersService.Domain.Entities;

public class OrderItem
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long OrderId { get; set; }
    public long? ItemId { get; set; }
    public long? StationId { get; set; }
    public string? StationCode { get; set; }
    public OrderItemStatus Status { get; set; } = OrderItemStatus.Queued;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; } = 1m;
    public decimal ModifiersTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public long? TaxId { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public bool IsTaxExempt { get; set; }
    public bool TaxInclusive { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
    public long? VoidedBy { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    public Order Order { get; set; } = null!;
    public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
}