using CatalogService.Domain.Constants;

namespace CatalogService.Domain.Entities;

public class Country
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Code { get; set; } = null!;
    public string? CodeAlpha3 { get; set; }
    public string Name { get; set; } = null!;
    public string? PhoneCode { get; set; }
    public long? DefaultCurrencyId { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Currency? DefaultCurrency { get; set; }
}