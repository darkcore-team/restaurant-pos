using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Categories;

public sealed class GetCategoriesUseCase(ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<CategoryDto>> ExecuteAsync(long restaurantId, long branchId, CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetByBranchAsync(restaurantId, branchId, ct);
        return categories.Select(ToDto).ToList();
    }

    internal static CategoryDto ToDto(Category c)
        => new(c.Id, c.PublicId, c.Name, c.NameEn, c.ImageUrl, c.Position);
}

public sealed class CreateCategoryUseCase(ICategoryRepository categoryRepository)
{
    public async Task<Answer<CategoryDto>> ExecuteAsync(
        long restaurantId, long branchId, long actorId, SaveCategoryRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var category = new Category
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId = branchId,
            Name = request.Name.Trim(),
            NameEn = request.NameEn?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            Position = request.Position,
            Status = RecordStatus.Active,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        await categoryRepository.CreateAsync(category, ct);
        return Answer<CategoryDto>.Ok(GetCategoriesUseCase.ToDto(category));
    }
}

public sealed class UpdateCategoryUseCase(ICategoryRepository categoryRepository)
{
    public async Task<Answer<CategoryDto>> ExecuteAsync(
        long id, long restaurantId, long actorId, SaveCategoryRequest request, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, restaurantId, ct);
        if (category is null)
            return Answer<CategoryDto>.Fail("CATEGORY_NOT_FOUND", "Categoría no encontrada.");

        category.Name = request.Name.Trim();
        category.NameEn = request.NameEn?.Trim();
        category.ImageUrl = request.ImageUrl?.Trim();
        category.Position = request.Position;
        category.UpdatedAt = DateTimeOffset.UtcNow;
        category.UpdatedBy = actorId;

        await categoryRepository.UpdateAsync(category, ct);
        return Answer<CategoryDto>.Ok(GetCategoriesUseCase.ToDto(category));
    }
}

public sealed class DeleteCategoryUseCase(ICategoryRepository categoryRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, restaurantId, ct);
        if (category is null)
            return Answer.Fail("CATEGORY_NOT_FOUND", "Categoría no encontrada.");

        var now = DateTimeOffset.UtcNow;
        category.Status = RecordStatus.Inactive;
        category.DeletedAt = now;
        category.UpdatedAt = now;
        category.UpdatedBy = actorId;

        await categoryRepository.UpdateAsync(category, ct);
        return Answer.Ok(message: "Categoría eliminada.");
    }
}
