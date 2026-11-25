using System.ComponentModel.DataAnnotations;
using IdentityService.Domain.Constants;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace IdentityService.Application.Branches;

public sealed record BranchDetailDto(
    long Id,
    Guid PublicId,
    string Code,
    string Name,
    string? Address,
    string? Phone,
    bool IsDefault,
    string? OpensAt,
    string? ClosesAt,
    string Status);

public sealed class SaveBranchRequest
{
    [Required, MaxLength(20)] public string Code { get; set; } = null!;
    [Required, MaxLength(120)] public string Name { get; set; } = null!;
    [MaxLength(250)] public string? Address { get; set; }
    [MaxLength(40)] public string? Phone { get; set; }
    public bool IsDefault { get; set; }
    public string? OpensAt { get; set; }
    public string? ClosesAt { get; set; }
}

public sealed class GetRestaurantBranchesUseCase(IBranchRepository branchRepository)
{
    public async Task<IReadOnlyList<BranchDetailDto>> ExecuteAsync(long restaurantId, CancellationToken ct = default)
    {
        var branches = await branchRepository.GetByRestaurantAsync(restaurantId, ct);
        return branches.Select(ToDto).ToList();
    }

    internal static BranchDetailDto ToDto(Branch b) => new(
        b.Id,
        b.PublicId,
        b.Code,
        b.Name,
        b.Address,
        b.Phone,
        b.IsDefault,
        b.OpensAt?.ToString("HH:mm"),
        b.ClosesAt?.ToString("HH:mm"),
        b.Status.ToString());
}

public sealed class CreateBranchUseCase(IBranchRepository branchRepository)
{
    public async Task<Answer<BranchDetailDto>> ExecuteAsync(
        long restaurantId, long actorId, SaveBranchRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await branchRepository.CodeExistsAsync(code, restaurantId, null, ct))
            return Answer<BranchDetailDto>.Fail("DUPLICATE_CODE", $"Ya existe una sucursal con código '{code}'.");

        var cityId = await branchRepository.GetDefaultCityIdAsync(restaurantId, ct);
        var now = DateTimeOffset.UtcNow;

        var branch = new Branch
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            CityId = cityId,
            Code = code,
            Name = request.Name.Trim(),
            Address = request.Address?.Trim(),
            Phone = request.Phone?.Trim(),
            IsDefault = request.IsDefault,
            OpensAt = TimeOnly.TryParse(request.OpensAt, out var open) ? open : null,
            ClosesAt = TimeOnly.TryParse(request.ClosesAt, out var close) ? close : null,
            Status = RecordStatus.Active,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actorId,
            UpdatedBy = actorId,
        };

        await branchRepository.CreateAsync(branch, ct);
        return Answer<BranchDetailDto>.Ok(GetRestaurantBranchesUseCase.ToDto(branch));
    }
}

public sealed class UpdateBranchUseCase(IBranchRepository branchRepository)
{
    public async Task<Answer<BranchDetailDto>> ExecuteAsync(
        long id, long restaurantId, long actorId, SaveBranchRequest request, CancellationToken ct = default)
    {
        var branch = await branchRepository.GetByIdAsync(id, restaurantId, ct);
        if (branch is null)
            return Answer<BranchDetailDto>.Fail("BRANCH_NOT_FOUND", "Sucursal no encontrada.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await branchRepository.CodeExistsAsync(code, restaurantId, id, ct))
            return Answer<BranchDetailDto>.Fail("DUPLICATE_CODE", $"Ya existe otra sucursal con código '{code}'.");

        branch.Code = code;
        branch.Name = request.Name.Trim();
        branch.Address = request.Address?.Trim();
        branch.Phone = request.Phone?.Trim();
        branch.IsDefault = request.IsDefault;
        branch.OpensAt = TimeOnly.TryParse(request.OpensAt, out var open) ? open : null;
        branch.ClosesAt = TimeOnly.TryParse(request.ClosesAt, out var close) ? close : null;
        branch.UpdatedAt = DateTimeOffset.UtcNow;
        branch.UpdatedBy = actorId;

        await branchRepository.UpdateAsync(branch, ct);
        return Answer<BranchDetailDto>.Ok(GetRestaurantBranchesUseCase.ToDto(branch));
    }
}

public sealed class DeleteBranchUseCase(IBranchRepository branchRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var branch = await branchRepository.GetByIdAsync(id, restaurantId, ct);
        if (branch is null)
            return Answer.Fail("BRANCH_NOT_FOUND", "Sucursal no encontrada.");

        if (branch.IsDefault)
            return Answer.Fail("CANNOT_DELETE_DEFAULT", "No se puede eliminar la sucursal principal.");

        branch.Status = RecordStatus.Deleted;
        branch.UpdatedAt = DateTimeOffset.UtcNow;
        branch.UpdatedBy = actorId;

        await branchRepository.UpdateAsync(branch, ct);
        return Answer.Ok(message: "Sucursal eliminada.");
    }
}
