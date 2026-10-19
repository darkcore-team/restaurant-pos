using TablesService.Domain.Constants;

namespace TablesService.Domain.Entities;

public class TableAssignment
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long TableId { get; set; }
    public long WaiterId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public long? AssignedBy { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public long? ReleasedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }

    public DiningTable Table { get; set; } = null!;
}