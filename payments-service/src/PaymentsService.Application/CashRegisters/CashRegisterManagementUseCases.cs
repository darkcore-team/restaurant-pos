using System.ComponentModel.DataAnnotations;
using PaymentsService.Domain.Entities;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.CashRegisters;

public sealed class SaveCashRegisterRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = null!;
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
}

public sealed class CreateCashRegisterUseCase(ICashRegisterRepository repository)
{
    public async Task<Answer<CashRegisterDto>> ExecuteAsync(
        long restaurantId, long branchId, long actorId, SaveCashRegisterRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(code, branchId, restaurantId, null, ct))
            return Answer<CashRegisterDto>.Fail("DUPLICATE_CODE", $"Ya existe una caja con código '{code}'.");

        var now = DateTimeOffset.UtcNow;
        var entity = new CashRegister
        {
            PublicId = Guid.NewGuid(),
            RestaurantId = restaurantId,
            BranchId = branchId,
            Code = code,
            Name = request.Name.Trim(),
            IsActive = true,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        await repository.CreateAsync(entity, ct);
        return Answer<CashRegisterDto>.Ok(
            new CashRegisterDto(entity.Id, entity.PublicId, entity.Code, entity.Name, entity.IsActive));
    }
}

public sealed class UpdateCashRegisterUseCase(ICashRegisterRepository repository)
{
    public async Task<Answer<CashRegisterDto>> ExecuteAsync(
        long id, long restaurantId, long branchId, long actorId, SaveCashRegisterRequest request, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, restaurantId, ct);
        if (entity is null)
            return Answer<CashRegisterDto>.Fail("CASH_REGISTER_NOT_FOUND", "Caja registradora no encontrada.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(code, branchId, restaurantId, id, ct))
            return Answer<CashRegisterDto>.Fail("DUPLICATE_CODE", $"Ya existe otra caja con código '{code}'.");

        entity.Code = code;
        entity.Name = request.Name.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = actorId;

        await repository.UpdateAsync(entity, ct);
        return Answer<CashRegisterDto>.Ok(
            new CashRegisterDto(entity.Id, entity.PublicId, entity.Code, entity.Name, entity.IsActive));
    }
}

public sealed class DeleteCashRegisterUseCase(
    ICashRegisterRepository repository,
    ICashSessionRepository sessionRepository)
{
    public async Task<Answer> ExecuteAsync(long id, long restaurantId, long actorId, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, restaurantId, ct);
        if (entity is null)
            return Answer.Fail("CASH_REGISTER_NOT_FOUND", "Caja registradora no encontrada.");

        if (await sessionRepository.HasOpenSessionForRegisterAsync(id, restaurantId, ct))
            return Answer.Fail("REGISTER_HAS_OPEN_SESSION", "No se puede eliminar una caja con un turno abierto.");

        var now = DateTimeOffset.UtcNow;
        entity.IsActive = false;
        entity.DeletedAt = now;
        entity.UpdatedAt = now;
        entity.UpdatedBy = actorId;

        await repository.UpdateAsync(entity, ct);
        return Answer.Ok(message: "Caja registradora eliminada.");
    }
}
