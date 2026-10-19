namespace CatalogService.Application.Menu;

public sealed record MenuDto(                                                                                       
    long BranchId,                                                                                                  
    IReadOnlyList<MenuCategoryDto> Categories);
    
public sealed record MenuCategoryDto(
    long Id,
    Guid PublicId,
    string Name,
    string? NameEn,
    string? ImageUrl,
    short Position,
    IReadOnlyList<MenuProductDto> Products);
    
public sealed record MenuProductDto(
    long Id,
    Guid PublicId,
    string Name,
    string? NameEn,
    string? Description,
    decimal BasePrice,
    string? ImageUrl,
    bool IsAvailable,
    short Position,
    long StationId,
    string StationCode,
    long? TaxId,
    bool IsTaxExempt,
    IReadOnlyList<MenuOptionGroupDto> OptionGroup);
    
public sealed record MenuOptionGroupDto(
    long Id,
    Guid PublicId,
    string Name,
    short MinSelect,
    short MaxSelect,
    bool IsRequired,
    short Position,
    IReadOnlyList<MenuOptionItemDto> Options);
    
public sealed record MenuOptionItemDto(
    long Id,
    Guid PublicId,
    string Name,
    string? NameEn,
    decimal PriceDelta,
    bool IsDefault,
    short Position);