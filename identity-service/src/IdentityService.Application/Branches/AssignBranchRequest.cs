using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Users;

public record AssignBranchRequest
{
    [Required]
    public long UserId { get; init; }
}

public sealed record AssignBranchOutcome
{
    public bool Succeeded { get; init; }
    public string? ErrorCode { get; init; }
}

public static class AssignBranchErrors
{
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string BranchNotFound = "BRANCH_NOT_FOUND";
    public const string AlreadyAssigned = "ALREADY_ASSIGNED";
}
