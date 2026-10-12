namespace IdentityService.Application.Users;

public record CreateUserResponse(long Id);

public record CreateUserOutcome
{
    public bool Succeeded { get; init; }
    public long? UserId { get; set; }
    public string? ErrorCode { get; init; }
}

public static class CreateUserErrors
{
    public const string UsernameTaken = "USERNAME_TAKEN";
    public const string UnknownAuthority = "UNKNOWN_AUTHORITY";
    public const string UnknownBranch = "UNKNOWN_BRANCH";
}
