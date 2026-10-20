namespace PaymentsService.Domain.Constants;

public static class PaymentMethods
{
    public sealed record Rule(bool RequiresReference, bool RequiresCashSession, bool RequiresAmountTendered);

    public static readonly IReadOnlyDictionary<string, Rule> Rules =
        new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["CASH"] = new(RequiresReference: false, RequiresCashSession: true, RequiresAmountTendered: true),
            ["CARD"] = new(RequiresReference: true, RequiresCashSession: true, RequiresAmountTendered: false),
            ["TRANSFER"] = new(RequiresReference: true, RequiresCashSession: false, RequiresAmountTendered: false)
        };
    
    public const string Cash = "CASH";
    public const string Card = "CARD";
    public const string Transfer = "TRANSFER";
}