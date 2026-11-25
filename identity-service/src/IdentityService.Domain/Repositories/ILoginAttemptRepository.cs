using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface ILoginAttemptRepository
{
    Task<IReadOnlyList<LoginAttempt>> GetRecentByRestaurantAsync(long restaurantId, int limit, CancellationToken ct);
}
