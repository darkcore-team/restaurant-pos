using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using IdentityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class RestaurantRepository(IdentityServiceDbContext dbContext) : IRestaurantRepository
{
    public Task<Restaurant?> GetByIdAsync(long id, CancellationToken ct)
    {
        return dbContext.Restaurants.SingleOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task UpdateAsync(Restaurant restaurant, CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
