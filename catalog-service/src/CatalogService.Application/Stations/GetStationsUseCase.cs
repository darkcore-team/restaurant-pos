using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Tenancy;

namespace CatalogService.Application.Stations;

public sealed class GetStationsUseCase(IStationRepository repository, ITenantContext tenantContext)
{
    public async Task<IReadOnlyList<StationDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var restaurantId = tenantContext.RestaurantId
            ?? throw new InvalidOperationException($"Missing restaurant_id claim");
        
        var stations = await repository.GetActiveStationsAsync(restaurantId, cancellationToken);

        return stations.Select(s => new StationDto(
            s.Id,
            s.PublicId,
            s.Code,
            s.Name,
            s.Description,
            s.Color,
            s.Position
        )).ToList();
    }
}