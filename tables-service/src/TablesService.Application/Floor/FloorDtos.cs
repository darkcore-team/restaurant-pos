namespace TablesService.Application.Floor;

public sealed record TableDto(
    long Id,
    Guid PublicId,
    string Code,
    string Name,
    short MinCapacity,
    short MaxCapacity,
    bool IsCombinable,
    string Status,
    short Position,
    string? Notes,
    bool IsActive,
    long ZoneId,
    long? AssignedWaiterId,
    DateTimeOffset? AssignedAt);

public sealed record ZoneDto(
    long Id,
    Guid PublicId,
    string Name,
    short Position,
    bool IsActive,
    IReadOnlyList<TableDto> Tables);