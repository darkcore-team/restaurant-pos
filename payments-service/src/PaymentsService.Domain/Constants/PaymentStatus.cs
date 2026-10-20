using NpgsqlTypes;

namespace PaymentsService.Domain.Constants;

public enum PaymentStatus
{
    [PgName("PENDING")] Pending = 0,
    [PgName("COMPLETED")] Completed = 1,
    [PgName("CANCELLED")] Cancelled = 2,
    [PgName("REFUNDED")] Refunded = 3,
}