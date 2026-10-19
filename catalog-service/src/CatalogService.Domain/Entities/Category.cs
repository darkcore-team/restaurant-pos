using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class Category
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? ImageUrl { get; set; }
    public short Position { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}