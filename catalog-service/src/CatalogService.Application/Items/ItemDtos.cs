using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Items;

public sealed class CreateItemRequest
{
    [Required] public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal BasePrice { get; set; }
    [Required] public long CategoryId { get; set; }
    [Required] public long StationId { get; set; }
    public long? TaxId { get; set; }
    public bool IsTaxExempt { get; set; }
    public bool IsAvailable { get; set; } = true;
    public short Position { get; set; }
    public List<ItemOptionGroupLink>? OptionGroups { get; set; }
}

public sealed class UpdateItemRequest
{
    [Required] public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal BasePrice { get; set; }
    [Required] public long CategoryId { get; set; }
    [Required] public long StationId { get; set; }
    public long? TaxId { get; set; }
    public bool IsTaxExempt { get; set; }
    public bool IsAvailable { get; set; } = true;
    public short Position { get; set; }
    public List<ItemOptionGroupLink>? OptionGroups { get; set; }
}

public sealed class ItemOptionGroupLink
{
    [Required] public long OptionGroupId { get; set; }
    public short? MinSelect { get; set; }
    public short? MaxSelect { get; set; }
    public short Position { get; set; }
}

public sealed record ItemDto(
    long Id, Guid PublicId, string Name, string? NameEn, string? Description,
    decimal BasePrice, long CategoryId, long StationId, long? TaxId, bool IsTaxExempt,
    bool IsAvailable, short Position, IReadOnlyList<long> OptionGroupIds);

public static class ItemErrors
{
    public const string UnknownCategory = "UNKNOWN_CATEGORY";
    public const string UnknownStation = "UNKNOWN_STATION";
    public const string UnknownOptionGroup = "UNKNOWN_OPTION_GROUP";
    public const string UnknownTax = "UNKNOWN_TAX";
    public const string NotFound = "ITEM_NOT_FOUND";
}