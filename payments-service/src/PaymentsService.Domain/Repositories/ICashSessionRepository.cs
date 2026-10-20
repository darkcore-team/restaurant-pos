using PaymentsService.Domain.Entities;

namespace PaymentsService.Domain.Repositories;

public interface ICashSessionRepository
{
    Task<CashSession?> GetByIdAsync(long id, long restaurantId, CancellationToken ct = default);
    Task<CashSession?> GetActiveAsync(long branchId, long restaurantId, CancellationToken ct = default);
    Task<bool> HasOpenSessionForRegisterAsync(long cashRegisterId, long restaurantId, CancellationToken ct = default);
    Task CreateAsync(CashSession session, CancellationToken ct = default);
    Task UpdateAsync(CashSession session, CancellationToken ct = default);
}