using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class PaymentMethod
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool RequiresReference { get; set; }
    public bool RequiresAmountTendered { get; set; }
    public bool RequiresCashSession { get; set; }
    public short Position { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}