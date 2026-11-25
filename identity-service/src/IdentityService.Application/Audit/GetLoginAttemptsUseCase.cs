using IdentityService.Domain.Repositories;

namespace IdentityService.Application.Audit;

public sealed record LoginAttemptDto(
    long Id,
    string UsernameTry,
    string? Ip,
    string? UserAgent,
    bool WasSuccessful,
    string? FailureReason,
    DateTimeOffset CreatedAt);

public sealed class GetLoginAttemptsUseCase(ILoginAttemptRepository loginAttemptRepository)
{
    public async Task<IReadOnlyList<LoginAttemptDto>> ExecuteAsync(
        long restaurantId, int limit = 50, CancellationToken ct = default)
    {
        var attempts = await loginAttemptRepository.GetRecentByRestaurantAsync(restaurantId, limit, ct);
        return attempts.Select(a => new LoginAttemptDto(
            a.Id,
            a.UsernameTry,
            a.Ip?.ToString(),
            a.UserAgent,
            a.WasSuccessful,
            a.FailureReason,
            a.CreatedAt)).ToList();
    }
}
