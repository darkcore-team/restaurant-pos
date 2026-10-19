namespace OrdersService.Application.Kds;

public sealed record KdsItemDto(
    long OrderItemId,
    Guid PublicId,
    long? CatalogItemId,
    string Name,
    decimal Quantity,
    decimal ModifiersTotal,
    string Status,
    string? Notes,
    string? StationCode,
    IReadOnlyList<string> Modifiers);

public sealed record KdsOrderGroupDto(
    long OrderId,
    string OrderNumber,
    long? TableId,
    string OrderStatus,
    IReadOnlyList<KdsItemDto> Items);