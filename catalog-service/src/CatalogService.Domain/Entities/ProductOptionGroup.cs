using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class ProductOptionGroup
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long ProductId { get; set; }
    public long OptionGroupId { get; set; }
    public short? MinSelect { get; set; }
    public short? MaxSelect { get; set; }
    public short Position { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Product Product { get; set; } = null!;
    public OptionGroup OptionGroup { get; set; } = null!;
}