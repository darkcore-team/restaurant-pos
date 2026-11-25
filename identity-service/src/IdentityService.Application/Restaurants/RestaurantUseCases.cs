using System.ComponentModel.DataAnnotations;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace IdentityService.Application.Restaurants;

public sealed record RestaurantDto(
    long Id,
    Guid PublicId,
    string Name,
    string? TaxId,
    string Timezone,
    string Status);

public sealed class UpdateRestaurantRequest
{
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(40)] public string? TaxId { get; set; }
    [Required, MaxLength(60)] public string Timezone { get; set; } = "America/Managua";
}

public sealed class GetRestaurantUseCase(IRestaurantRepository restaurantRepository)
{
    public async Task<Answer<RestaurantDto>> ExecuteAsync(long restaurantId, CancellationToken ct = default)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(restaurantId, ct);
        if (restaurant is null)
            return Answer<RestaurantDto>.Fail("RESTAURANT_NOT_FOUND", "Restaurante no encontrado.");

        return Answer<RestaurantDto>.Ok(ToDto(restaurant));
    }

    internal static RestaurantDto ToDto(Restaurant r) =>
        new(r.Id, r.PublicId, r.Name, r.TaxId, r.Timezone, r.Status.ToString());
}

public sealed class UpdateRestaurantUseCase(IRestaurantRepository restaurantRepository)
{
    public async Task<Answer<RestaurantDto>> ExecuteAsync(
        long restaurantId, long actorId, UpdateRestaurantRequest request, CancellationToken ct = default)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(restaurantId, ct);
        if (restaurant is null)
            return Answer<RestaurantDto>.Fail("RESTAURANT_NOT_FOUND", "Restaurante no encontrado.");

        restaurant.Name = request.Name.Trim();
        restaurant.TaxId = request.TaxId?.Trim();
        restaurant.Timezone = request.Timezone.Trim();
        restaurant.UpdatedAt = DateTimeOffset.UtcNow;
        restaurant.UpdatedBy = actorId;

        await restaurantRepository.UpdateAsync(restaurant, ct);
        return Answer<RestaurantDto>.Ok(GetRestaurantUseCase.ToDto(restaurant));
    }
}
