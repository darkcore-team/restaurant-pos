using NpgsqlTypes;

namespace PaymentsService.Domain.Constants;

public enum CashSessionStatus
{
    [PgName("OPEN")] Open = 0,
    [PgName("CLOSED")] Closed = 1,
}