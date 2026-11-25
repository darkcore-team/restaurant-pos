using System.ComponentModel.DataAnnotations;
using CatalogService.Domain.Constants;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace CatalogService.Application.Taxes;

public sealed class SaveTaxRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = null!;
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    [Range(0, 100)] public decimal Rate { get; set; }
    public bool IsInclusive { get; set; }
}

public sealed class CreateTaxUseCase(ITaxRepository repository)
{
    public async Task<Answer<TaxDto>> ExecuteAsync(long actorId, SaveTaxRequest request, CancellationToken ct = default)
    {
        var countryId = await repository.GetDefaultCountryIdAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var entity = new Tax
        {
            PublicId = Guid.NewGuid(),
            CountryId = countryId,
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Rate = request.Rate,
            IsInclusive = request.IsInclusive,
            Status = RecordStatus.Active,
            CreatedAt = now,
            CreatedBy = actorId,
            UpdatedAt = now,
            UpdatedBy = actorId,
        };

        await repository.CreateAsync(entity, ct);
        return Answer<TaxDto>.Ok(new TaxDto(entity.Id, entity.PublicId, entity.Code, entity.Name, entity.Rate, entity.IsInclusive));
    }
}

public sealed class UpdateTaxUseCase(ITaxRepository repository)
{
    public async Task<Answer<TaxDto>> ExecuteAsync(long id, long actorId, SaveTaxRequest request, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Answer<TaxDto>.Fail("TAX_NOT_FOUND", "Impuesto no encontrado.");

        entity.Code = request.Code.Trim().ToUpperInvariant();
        entity.Name = request.Name.Trim();
        entity.Rate = request.Rate;
        entity.IsInclusive = request.IsInclusive;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = actorId;

        await repository.UpdateAsync(entity, ct);
        return Answer<TaxDto>.Ok(new TaxDto(entity.Id, entity.PublicId, entity.Code, entity.Name, entity.Rate, entity.IsInclusive));
    }
}

public sealed class DeleteTaxUseCase(ITaxRepository repository)
{
    public async Task<Answer> ExecuteAsync(long id, long actorId, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null)
            return Answer.Fail("TAX_NOT_FOUND", "Impuesto no encontrado.");

        var now = DateTimeOffset.UtcNow;
        entity.Status = RecordStatus.Deleted;
        entity.DeletedAt = now;
        entity.UpdatedAt = now;
        entity.UpdatedBy = actorId;

        await repository.UpdateAsync(entity, ct);
        return Answer.Ok(message: "Impuesto eliminado.");
    }
}
