using PaymentsService.Domain.Entities;

namespace PaymentsService.Domain.Repositories;

public interface ICashRegisterRepository
{
    Task<CashRegister?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);

    Task<IReadOnlyList<CashRegister>>
        GetByBranchAsync(long branchId, long restaurantId, CancellationToken ct = default);

    Task<bool> CodeExistsAsync(string code, long branchId, long restaurantId, long? excludeId, CancellationToken ct = default);
    Task CreateAsync(CashRegister cashRegister, CancellationToken ct = default);
    Task UpdateAsync(CashRegister cashRegister, CancellationToken ct = default);
}