using NpgsqlTypes;

namespace OrdersService.Domain.Constants;

public enum OrderStatus
{
    [PgName("OPEN")] Open = 0,
    [PgName("IN_PREPARATION")] InPreparation = 1,
    [PgName("READY")] Ready = 2,
    [PgName("SERVED")] Served = 3,
    [PgName("BILLING")] Billing = 4,
    [PgName("CLOSED")] Closed = 5,
    [PgName("CANCELLED")] Cancelled = 6,
}