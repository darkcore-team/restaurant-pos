namespace CatalogService.Application.Currencies;

public sealed record CurrencyDto(long Id, Guid PublicId, string Code, string Name, string Symbol, short DecimalPlaces);