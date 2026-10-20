using Microsoft.Extensions.Localization;
using PaymentsService.Domain.Repositories;
using RestaurantPos.Shared.Abstractions.Common;

namespace PaymentsService.Application.CashRegisters;

public class GetCashRegistersUseCase(ICashRegisterRepository cashRegisterRepository, IStringLocalizer localizer)
{
    public async Task<Answer<IReadOnlyList<CashRegisterDto>>> ExecuteAsync(long restaurantId, long branchId, CancellationToken ct)
    {
        var registers = await cashRegisterRepository.GetByBranchAsync(branchId, restaurantId, ct);

        var dtos = registers
            .Select(c => new CashRegisterDto(c.Id, c.PublicId, c.Code, c.Name, c.IsActive))
            .ToList();
        
        return Answer<IReadOnlyList<CashRegisterDto>>.Ok(dtos, localizer["Ok"]);
    }
}