using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    decimal UsdExchangeRate,
    string Status);

public sealed class UpdateRestaurantRequest
{
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(40)] public string? TaxId { get; set; }
    [Required, MaxLength(60)] public string Timezone { get; set; } = "America/Managua";
    [Range(0.01, 10000)] public decimal? UsdExchangeRate { get; set; }
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

    internal static RestaurantDto ToDto(Restaurant r)
    {
        var rate = 36.62m;
        if (r.Settings.ValueKind == JsonValueKind.Object
            && r.Settings.TryGetProperty("usdExchangeRate", out var prop)
            && prop.TryGetDecimal(out var parsed)
            && parsed > 0)
        {
            rate = parsed;
        }

        return new(r.Id, r.PublicId, r.Name, r.TaxId, r.Timezone, rate, r.Status.ToString());
    }
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

        if (request.UsdExchangeRate.HasValue && request.UsdExchangeRate.Value > 0)
        {
            var settingsObj = restaurant.Settings.ValueKind == JsonValueKind.Object
                ? JsonNode.Parse(restaurant.Settings.GetRawText()) as JsonObject ?? new JsonObject()
                : new JsonObject();

            settingsObj["usdExchangeRate"] = request.UsdExchangeRate.Value;
            using var doc = JsonDocument.Parse(settingsObj.ToJsonString());
            restaurant.Settings = doc.RootElement.Clone();
        }

        restaurant.UpdatedAt = DateTimeOffset.UtcNow;
        restaurant.UpdatedBy = actorId;

        await restaurantRepository.UpdateAsync(restaurant, ct);
        return Answer<RestaurantDto>.Ok(GetRestaurantUseCase.ToDto(restaurant));
    }
}
