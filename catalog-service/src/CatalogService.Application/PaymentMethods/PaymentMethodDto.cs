namespace CatalogService.Application.PaymentMethods;

public sealed record PaymentMethodDto(
    long Id,
    Guid PublicId,
    string Code,
    string Name,
    bool RequiresReference,
    bool RequiresAmountTendered,
    bool RequiresCashSession,
    short Position);