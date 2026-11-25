using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Categories;

public sealed record CategoryDto(
    long Id,
    Guid PublicId,
    string Name,
    string? NameEn,
    string? ImageUrl,
    short Position);

public sealed class SaveCategoryRequest
{
    [Required, MaxLength(120)] public string Name { get; set; } = null!;
    [MaxLength(120)] public string? NameEn { get; set; }
    public string? ImageUrl { get; set; }
    public short Position { get; set; }
}
