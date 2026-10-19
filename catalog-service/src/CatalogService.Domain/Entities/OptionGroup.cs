using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class OptionGroup
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long? BranchId { get; set; }
    public string Name { get; set; } = null!;
    public short MinSelect { get; set; } = 1;
    public short MaxSelect { get; set; } = 1;
    public bool IsRequired { get; set; }
    public short Position { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<ProductOption> Options { get; set; } = new List<ProductOption>();
    public ICollection<ProductOptionGroup> ProductOptionGroups { get; set; } = new List<ProductOptionGroup>();
}