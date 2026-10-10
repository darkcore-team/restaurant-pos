using System.Net;

namespace IdentityService.Domain.Entities;

public class LoginAttempt
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long? UserId { get; set; }
    public string UsernameTry { get; set; } = null!;
    public IPAddress? Ip { get; set; }
    public string? UserAgent { get; set; }
    public bool WasSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}