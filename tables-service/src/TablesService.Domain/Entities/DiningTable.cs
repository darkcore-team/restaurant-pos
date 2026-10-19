using TablesService.Domain.Constants;

namespace TablesService.Domain.Entities;

public class DiningTable
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public long ZoneId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short MinCapacity { get; set; } = 1;
    public short MaxCapacity { get; set; } = 4;
    public bool IsCombinable { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Free;
    public short Position { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Zone Zone { get; set; } = null!;
    public ICollection<TableAssignment> Assignments { get; set; } = new List<TableAssignment>();
}