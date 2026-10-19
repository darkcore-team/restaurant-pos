using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class ProductOption
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long OptionGroupId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public decimal PriceDelta { get; set; }
    public bool IsDefault { get; set; }
    public short Position { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public OptionGroup OptionGroup { get; set; } = null!;
}