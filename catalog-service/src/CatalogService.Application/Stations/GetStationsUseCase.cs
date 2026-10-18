using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;

namespace CatalogService.Application.Stations;

public sealed class GetStationsUseCase(IStationRepository repository)
{
    public async Task<IReadOnlyList<StationDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var stations = await repository.GetActiveStationsAsync(cancellationToken);

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