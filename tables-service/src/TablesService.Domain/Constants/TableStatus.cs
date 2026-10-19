using NpgsqlTypes;

namespace TablesService.Domain.Constants;

public enum TableStatus
{
    [PgName("FREE")] Free = 0,
    [PgName("OCCUPIED")] Occupied = 1,
    [PgName("BILLING")] Billing = 2,
    [PgName("RESERVED")] Reserved = 3,
    [PgName("DIRTY")] Dirty = 4,
    [PgName("OUT_OF_SERVICE")] OutOfService = 5,
}