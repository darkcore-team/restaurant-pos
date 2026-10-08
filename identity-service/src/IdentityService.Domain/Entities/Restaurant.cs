using System.Text.Json;
using IdentityService.Domain.Constants;

namespace IdentityService.Domain.Entities;

public class Restaurant
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public string? TaxId { get; set; }
    public string Timezone { get; set; } = "America/Managua";
    public long DefaultLanguageId { get; set; }
    public long DefaultCurrencyId { get; set; }
    public long? DefaultCountryId { get; set; }
    public long BaseCurrencyId { get; set; }
    public JsonElement Settings { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
