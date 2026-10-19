using System.ComponentModel.DataAnnotations;
using OrdersService.Domain.Constants;

namespace OrdersService.Application.Orders;

public sealed class CreateOrderRequest
{
    [Required] public long CurrencyId { get; set; }
    public long? TableId { get; set; }
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public short Covers { get; set; } = 1;
    public string? Notes { get; set; }
}

public sealed record OrderItemModifierDto(
    long Id, Guid PublicId, long? ModifierOptionId, long? ModifierGroupId,
    string Name, decimal PriceDelta, string? Notes);

public sealed record OrderItemDto(
    long Id, Guid PublicId, long? ItemId, long? StationId, string? StationCode, string Status,
    string Name, string? Description, decimal UnitPrice, decimal Quantity,
    decimal ModifiersTotal, decimal DiscountAmount, string? DiscountReason,
    long? TaxId, decimal? TaxRate, decimal TaxAmount, bool IsTaxExempt, bool TaxInclusive,
    decimal LineTotal, string? Notes, long? VoidedBy, DateTimeOffset? VoidedAt, string? VoidReason,
    IReadOnlyList<OrderItemModifierDto> Modifiers);

public sealed record OrderDto(
    long Id, Guid PublicId, string OrderNumber, string OrderType, string Status,
    long? TableId, long BranchId, long CurrencyId, decimal ExchangeRate,
    string? CustomerName, string? CustomerPhone, short Covers,
    decimal Subtotal, decimal DiscountAmount, decimal TaxAmount, decimal Total,
    decimal TipAmount, decimal PaidAmount, decimal BalanceDue,
    long? OpenedBy, DateTimeOffset OpenedAt, long? ClosedBy, DateTimeOffset? ClosedAt,
    string? Notes, IReadOnlyList<OrderItemDto> Items);

public sealed record CreateOrderResponse(long Id, string OrderNumber);

public sealed class AddItemsRequest
{
    [Required] public List<AddItemLineRequest> Items { get; set; } = new();
}

public sealed class AddItemLineRequest
{
    public long? ItemId { get; set; }
    [Required] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; } = 1m;
    public long? StationId { get; set; }
    public string? StationCode { get; set; }
    public long? TaxId { get; set; }
    public decimal? TaxRate { get; set; }
    public bool IsTaxExempt { get; set; }
    public string? Notes { get; set; }
    public List<AddItemModifierRequest>? Modifiers { get; set; }
}

public sealed class AddItemModifierRequest
{
    public long? ModifierOptionId { get; set; }
    public long? ModifierGroupId { get; set; }
    [Required] public string Name { get; set; } = null!;
    public decimal PriceDelta { get; set; }
    public string? Notes { get; set; }
}

public sealed class UpdateItemStatusRequest
{
    [Required] public string Status { get; set; } = null!;
}

public sealed class CancelOrderRequest
{
    public string? Reason { get; set; }
}