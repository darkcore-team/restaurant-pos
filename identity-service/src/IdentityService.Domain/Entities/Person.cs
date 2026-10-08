using IdentityService.Domain.Constants;

namespace IdentityService.Domain.Entities;

public class Person
{
    public long  Id { get; set; }
    public Guid  PublicId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? PhotoUrl { get; set; }
    public long? DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public long? CityId { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
