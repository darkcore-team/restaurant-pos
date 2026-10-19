using NpgsqlTypes;

namespace OrdersService.Domain.Constants;

public enum OrderItemStatus
{
    [PgName("QUEUED")] Queued = 0,
    [PgName("IN_PREPARATION")] InPreparation = 1,
    [PgName("READY")] Ready = 2,
    [PgName("SERVED")] Served = 3,
    [PgName("CANCELLED")] Cancelled = 4,
}