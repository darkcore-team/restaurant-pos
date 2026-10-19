namespace IdentityService.Application.Authorities;

public sealed record AuthorityDto
{
    public long Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}