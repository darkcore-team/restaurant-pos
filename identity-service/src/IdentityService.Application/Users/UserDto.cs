namespace IdentityService.Application.Users;

public record UserDto
{
    public long Id { get; init; }
    public Guid PublicId { get; init; }
    public string Username { get; init; } = null!;
    public string FirsName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public List<string> Roles { get; init; } = [];
    public List<BranchRefDto> Branches { get; init; } = [];
}

public sealed record BranchRefDto
{
    public long Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
}