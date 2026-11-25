using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.ModifierGroups;

public sealed class GetModifierGroupsUseCase(IOptionGroupRepository optionGroupRepository)
{
    public async Task<IReadOnlyList<ModifierGroupDto>> ExecuteAsync(long restaurantId, CancellationToken ct = default)
    {
        var groups = await optionGroupRepository.GetByRestaurantAsync(restaurantId, ct);
        return groups.Select(ToDto).ToList();
    }

    internal static ModifierGroupDto ToDto(OptionGroup g) => new(
        g.Id,
        g.PublicId,
        g.Name,
        g.MinSelect,
        g.MaxSelect,
        g.IsRequired,
        g.Position,
        g.Options
            .Where(o => o.Status == RecordStatus.Active)
            .OrderBy(o => o.Position)
            .ThenBy(o => o.Name)
            .Select(o => new ModifierOptionDto(o.Id, o.PublicId, o.Name, o.PriceDelta, o.IsDefault, o.Position))
            .ToList());
}

public sealed class CreateModifierGroupUseCase(IOptionGroupRepository optionGroupRepository)
{
    public async Task<Answer<ModifierGroupDto>> ExecuteAsync(
        long restaurantId, long branchId, long actorId, SaveModifierGroupRequest request, CancellationToken ct = default)
    {
        if (request.MaxSelect < request.MinSelect)
            return Answer<ModifierGroupDto>.Fail("INVALID_RANGE", "El máximo de selección no puede ser menor al mínimo.");

        var now = DateTimeOffset.UtcNow;
        var group = new OptionGroup
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId = branchId,
            Name = request.Name.Trim(),
            MinSelect = request.MinSelect,
            MaxSelect = request.MaxSelect,
            IsRequired = request.IsRequired,
            Position = request.Position,
            Status = RecordStatus.Active,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        short pos = 0;
        foreach (var opt in request.Options.Where(o => !string.IsNullOrWhiteSpace(o.Name)))
        {
            group.Options.Add(new ProductOption
            {
                PublicId = Guid.NewGuid(),
                RestaurantId = restaurantId,
                Name = opt.Name.Trim(),
                PriceDelta = opt.PriceDelta,
                IsDefault = opt.IsDefault,
                Position = opt.Position > 0 ? opt.Position : pos++,
                Status = RecordStatus.Active,
                CreatedAt = now,
                CreatedBy = actorId,
                UpdatedAt = now,
                UpdatedBy = actorId,
            });
        }

        await optionGroupRepository.CreateAsync(group, ct);
        return Answer<ModifierGroupDto>.Ok(GetModifierGroupsUseCase.ToDto(group));
    }
}

public sealed class UpdateModifierGroupUseCase(IOptionGroupRepository optionGroupRepository)
{
    public async Task<Answer<ModifierGroupDto>> ExecuteAsync(
        long id, long restaurantId, long actorId, SaveModifierGroupRequest request, CancellationToken ct = default)
    {
        if (request.MaxSelect < request.MinSelect)
            return Answer<ModifierGroupDto>.Fail("INVALID_RANGE", "El máximo de selección no puede ser menor al mínimo.");

        var group = await optionGroupRepository.GetByIdWithOptionsAsync(id, restaurantId, ct);
        if (group is null)
            return Answer<ModifierGroupDto>.Fail("MODIFIER_GROUP_NOT_FOUND", "Grupo de modificadores no encontrado.");

        var now = DateTimeOffset.UtcNow;
        group.Name = request.Name.Trim();
        group.MinSelect = request.MinSelect;
        group.MaxSelect = request.MaxSelect;
        group.IsRequired = request.IsRequired;
        group.Position = request.Position;
        group.UpdatedAt = now;
        group.UpdatedBy = actorId;

        var incomingIds = request.Options.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToHashSet();

        // Soft-delete removed options
        foreach (var existing in group.Options.Where(o => !incomingIds.Contains(o.Id)))
        {
            existing.Status = RecordStatus.Inactive;
            existing.DeletedAt = now;
            existing.UpdatedAt = now;
            existing.UpdatedBy = actorId;
        }

        // Update existing or add new options
        short pos = 0;
        foreach (var opt in request.Options.Where(o => !string.IsNullOrWhiteSpace(o.Name)))
        {
            var existing = opt.Id.HasValue ? group.Options.FirstOrDefault(o => o.Id == opt.Id.Value) : null;
            if (existing is not null)
            {
                existing.Name = opt.Name.Trim();
                existing.PriceDelta = opt.PriceDelta;
                existing.IsDefault = opt.IsDefault;
                existing.Position = opt.Position > 0 ? opt.Position : pos++;
                existing.UpdatedAt = now;
                existing.UpdatedBy = actorId;
            }
            else
            {
                group.Options.Add(new ProductOption
                {
                    PublicId = Guid.NewGuid(),
                    RestaurantId = restaurantId,
                    OptionGroupId = group.Id,
                    Name = opt.Name.Trim(),
                    PriceDelta = opt.PriceDelta,
                    IsDefault = opt.IsDefault,
                    Position = opt.Position > 0 ? opt.Position : pos++,
                    Status = RecordStatus.Active,
                    CreatedAt = now,
                    CreatedBy = actorId,
                    UpdatedAt = now,
                    UpdatedBy = actorId,
                });
            }
        }

        await optionGroupRepository.UpdateAsync(group, ct);
        return Answer<ModifierGroupDto>.Ok(GetModifierGroupsUseCase.ToDto(group));
    }
}

public sealed class DeleteModifierGroupUseCase(IOptionGroupRepository optionGroupRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var group = await optionGroupRepository.GetByIdWithOptionsAsync(id, restaurantId, ct);
        if (group is null)
            return Answer.Fail("MODIFIER_GROUP_NOT_FOUND", "Grupo de modificadores no encontrado.");

        var now = DateTimeOffset.UtcNow;
        group.Status = RecordStatus.Inactive;
        group.DeletedAt = now;
        group.UpdatedAt = now;
        group.UpdatedBy = actorId;

        foreach (var opt in group.Options)
        {
            opt.Status = RecordStatus.Inactive;
            opt.DeletedAt = now;
            opt.UpdatedAt = now;
            opt.UpdatedBy = actorId;
        }

        await optionGroupRepository.UpdateAsync(group, ct);
        return Answer.Ok(message: "Grupo de modificadores eliminado.");
    }
}
