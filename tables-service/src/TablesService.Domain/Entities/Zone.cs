using TablesService.Domain.Constants;

namespace TablesService.Domain.Entities;

public class Zone
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public string Name { get; set; } = null!;
    public short Position { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<DiningTable> Tables { get; set; } = new List<DiningTable>();
}