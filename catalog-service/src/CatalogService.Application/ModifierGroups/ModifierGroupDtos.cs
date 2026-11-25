using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.ModifierGroups;

public sealed record ModifierOptionDto(
    long Id,
    Guid PublicId,
    string Name,
    decimal PriceDelta,
    bool IsDefault,
    short Position);

public sealed record ModifierGroupDto(
    long Id,
    Guid PublicId,
    string Name,
    short MinSelect,
    short MaxSelect,
    bool IsRequired,
    short Position,
    IReadOnlyList<ModifierOptionDto> Options);

public sealed class SaveModifierOptionInput
{
    public long? Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = null!;
    [Range(0, double.MaxValue)] public decimal PriceDelta { get; set; }
    public bool IsDefault { get; set; }
    public short Position { get; set; }
}

public sealed class SaveModifierGroupRequest
{
    [Required, MaxLength(120)] public string Name { get; set; } = null!;
    [Range(0, 50)] public short MinSelect { get; set; }
    [Range(1, 50)] public short MaxSelect { get; set; } = 1;
    public bool IsRequired { get; set; }
    public short Position { get; set; }
    public List<SaveModifierOptionInput> Options { get; set; } = [];
}
