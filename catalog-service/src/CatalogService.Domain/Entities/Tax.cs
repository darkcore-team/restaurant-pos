using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class Tax
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long CountryId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public bool IsInclusive { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Country Country { get; set; } = null!;
}