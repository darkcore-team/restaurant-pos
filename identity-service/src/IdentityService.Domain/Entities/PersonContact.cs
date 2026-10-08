using IdentityService.Domain.Constants;

namespace IdentityService.Domain.Entities;

public class PersonContact
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long PersonId { get; set; }
    public string ContactType { get; set; } = null!;
    public string ContactValue { get; set; } = null!;
    public bool IsPrimary { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}