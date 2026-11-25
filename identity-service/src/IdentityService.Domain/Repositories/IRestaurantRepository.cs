using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Repositories;

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(long id, CancellationToken ct);
    Task UpdateAsync(Restaurant restaurant, CancellationToken ct);
}
