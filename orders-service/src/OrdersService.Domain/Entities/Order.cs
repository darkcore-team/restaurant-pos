using OrdersService.Domain.Constants;

namespace OrdersService.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long? TableId { get; set; }
    public long? ReservationId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public long CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1m;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public short Covers { get; set; } = 1;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal TipAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public long? OpenedBy { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public long? ClosedBy { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}