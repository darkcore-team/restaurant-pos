namespace IdentityService.Application.Branches;

public sealed record BranchDto
{
    public long Id { get; init; }
    public Guid PublicId { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public bool IsDefault { get; init; }
    public long RestaurantId { get; init; }
}