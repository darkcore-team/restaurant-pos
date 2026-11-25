using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Stations;

public sealed class SaveStationRequest
{
    [Required, MaxLength(40)] public string Code { get; set; } = null!;
    [Required, MaxLength(120)] public string Name { get; set; } = null!;
    [MaxLength(250)] public string? Description { get; set; }
    [MaxLength(30)] public string? Color { get; set; }
    public short Position { get; set; }
}

public sealed class CreateStationUseCase(IStationRepository stationRepository)
{
    public async Task<Answer<StationDto>> ExecuteAsync(
        long restaurantId, long actorId, SaveStationRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await stationRepository.CodeExistsAsync(code, restaurantId, null, ct))
            return Answer<StationDto>.Fail("STATION_CODE_EXISTS", $"Ya existe una estación con código '{code}'.");

        var now = DateTimeOffset.UtcNow;
        var station = new Station
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Color = request.Color?.Trim() ?? "#2563eb",
            Position = request.Position,
            Status = RecordStatus.Active,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        await stationRepository.CreateAsync(station, ct);
        return Answer<StationDto>.Ok(ToDto(station));
    }

    internal static StationDto ToDto(Station s)
        => new(s.Id, s.PublicId, s.Code, s.Name, s.Description, s.Color, s.Position);
}

public sealed class UpdateStationUseCase(IStationRepository stationRepository)
{
    public async Task<Answer<StationDto>> ExecuteAsync(
        long id, long restaurantId, long actorId, SaveStationRequest request, CancellationToken ct = default)
    {
        var station = await stationRepository.GetByIdAsync(id, restaurantId, ct);
        if (station is null)
            return Answer<StationDto>.Fail("STATION_NOT_FOUND", "Estación no encontrada.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await stationRepository.CodeExistsAsync(code, restaurantId, id, ct))
            return Answer<StationDto>.Fail("STATION_CODE_EXISTS", $"Ya existe otra estación con código '{code}'.");

        station.Code = code;
        station.Name = request.Name.Trim();
        station.Description = request.Description?.Trim();
        station.Color = request.Color?.Trim() ?? station.Color;
        station.Position = request.Position;
        station.UpdatedAt = DateTimeOffset.UtcNow;
        station.UpdatedBy = actorId;

        await stationRepository.UpdateAsync(station, ct);
        return Answer<StationDto>.Ok(CreateStationUseCase.ToDto(station));
    }
}

public sealed class DeleteStationUseCase(IStationRepository stationRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var station = await stationRepository.GetByIdAsync(id, restaurantId, ct);
        if (station is null)
            return Answer.Fail("STATION_NOT_FOUND", "Estación no encontrada.");

        var now = DateTimeOffset.UtcNow;
        station.Status = RecordStatus.Inactive;
        station.DeletedAt = now;
        station.UpdatedAt = now;
        station.UpdatedBy = actorId;

        await stationRepository.UpdateAsync(station, ct);
        return Answer.Ok(message: "Estación eliminada.");
    }
}
