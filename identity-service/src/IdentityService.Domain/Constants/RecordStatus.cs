using NpgsqlTypes;

namespace IdentityService.Domain.Constants;

public enum RecordStatus
{
    [PgName("ACTIVE")] Active = 0,
    [PgName("INACTIVE")] Inactive = 1,
    [PgName("DELETED")] Deleted = 2,
}
