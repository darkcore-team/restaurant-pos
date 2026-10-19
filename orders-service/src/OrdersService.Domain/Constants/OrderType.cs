using NpgsqlTypes;

namespace OrdersService.Domain.Constants;

public enum OrderType
{
    [PgName("DINE_IN")] DineIn = 0,
    [PgName("TAKEOUT")] Takeout = 1,
    [PgName("DELIVERY")] Delivery = 2,
}