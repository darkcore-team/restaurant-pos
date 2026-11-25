using System.ComponentModel.DataAnnotations;

namespace IdentityService.Application.Users;

public record CreateUserRequest
{
    [Required(AllowEmptyStrings = false), MaxLength(80)]
    public string FirstName { get; init; } = null!;

    [Required(AllowEmptyStrings = false), MaxLength(80)]
    public string LastName { get; init; } = null!;

    [Required(AllowEmptyStrings = false), MaxLength(60)]
    public string Username { get; init; } = null!;

    [Required(AllowEmptyStrings = false), MinLength(6), MaxLength(128)]
    public string Password { get; init; } = null!;

    [Required, MinLength(1)] public List<string> AuthorityCodes { get; init; } = [];
    [Required, MinLength(1)] public List<long> BranchIds { get; init; } = [];
}