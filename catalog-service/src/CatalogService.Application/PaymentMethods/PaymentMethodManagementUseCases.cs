using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.PaymentMethods;

public sealed class SavePaymentMethodRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = null!;
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    public bool RequiresReference { get; set; }
    public bool RequiresAmountTendered { get; set; }
    public bool RequiresCashSession { get; set; }
    public short Position { get; set; }
}

public sealed class CreatePaymentMethodUseCase(IPaymentMethodRepository repository)
{
    public async Task<Answer<PaymentMethodDto>> ExecuteAsync(
        long actorId, SavePaymentMethodRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new PaymentMethod
        {
            PublicId = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            RequiresReference = request.RequiresReference,
            RequiresAmountTendered = request.RequiresAmountTendered,
            RequiresCashSession = request.RequiresCashSession,
            Position = request.Position,
            Status = RecordStatus.Active,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        await repository.CreateAsync(entity, ct);
        return Answer<PaymentMethodDto>.Ok(ToDto(entity));
    }

    internal static PaymentMethodDto ToDto(PaymentMethod p) => new(
        p.Id, p.PublicId, p.Code, p.Name,
        p.RequiresReference, p.RequiresAmountTendered, p.RequiresCashSession, p.Position);
}

public sealed class UpdatePaymentMethodUseCase(IPaymentMethodRepository repository)
{
    public async Task<Answer<PaymentMethodDto>> ExecuteAsync(
        long id, long actorId, SavePaymentMethodRequest request, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Answer<PaymentMethodDto>.Fail("PAYMENT_METHOD_NOT_FOUND", "Método de pago no encontrado.");

        entity.Code = request.Code.Trim().ToUpperInvariant();
        entity.Name = request.Name.Trim();
        entity.RequiresReference = request.RequiresReference;
        entity.RequiresAmountTendered = request.RequiresAmountTendered;
        entity.RequiresCashSession = request.RequiresCashSession;
        entity.Position = request.Position;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = actorId;

        await repository.UpdateAsync(entity, ct);
        return Answer<PaymentMethodDto>.Ok(CreatePaymentMethodUseCase.ToDto(entity));
    }
}

public sealed class DeletePaymentMethodUseCase(IPaymentMethodRepository repository)
{
    public async Task<Answer> ExecuteAsync(long id, long actorId, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Answer.Fail("PAYMENT_METHOD_NOT_FOUND", "Método de pago no encontrado.");

        var now = DateTimeOffset.UtcNow;
        entity.Status = RecordStatus.Deleted;
        entity.DeletedAt = now;
        entity.UpdatedAt = now;
        entity.UpdatedBy = actorId;

        await repository.UpdateAsync(entity, ct);
        return Answer.Ok(message: "Método de pago eliminado.");
    }
}
