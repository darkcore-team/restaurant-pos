using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class Currency
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public short DecimalPlaces { get; set; } = 2;
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}