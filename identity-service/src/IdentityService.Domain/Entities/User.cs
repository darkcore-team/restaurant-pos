using IdentityService.Domain.Constants;

namespace IdentityService.Domain.Entities;

public class User
{
    public long  Id { get; set; }
    public Guid PublicId  { get; set; }
    public long PersonId { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTimeOffset? PasswordUpdatedAt { get; set; }
    public bool MustChangePassword { get; set; } = true;
    public short FailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long ? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}
