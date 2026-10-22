namespace IdentityService.Application.Auth;

public sealed record LoginRequest(string Username, string Password);

public sealed record SessionDto(
    long Id,
    string Username,
    string DisplayName,
    long RestaurantId,
    long BranchId,
    IReadOnlyList<string> Roles);

public sealed record CsrfTokenDto(string Token);
