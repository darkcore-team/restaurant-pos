namespace CatalogService.Application.Stations;

public sealed record StationDto(
    long Id,
    Guid PublicId,
    string Code,
    string Name,
    string? Description,
    string? Color,
    short Position);