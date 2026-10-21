using System.Data;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ReportingService.Application.Reporting;

namespace ReportingService.Infrastructure.Repositories;

public class ReportingReadRepository(IConfiguration configuration) : IReportingReadRepository
{
    static ReportingReadRepository() { SqlMapper.AddTypeHandler(new DateOnlyTypeHandler()); }
    private readonly string _conn = configuration.GetConnectionString("ReportingDb")
        ?? throw new InvalidOperationException("Connection string 'ReportingDb' not found");

    private NpgsqlConnection Open() => new(_conn);

    public async Task<DailySalesDto?> GetDailySalesAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default)
    {
        using var c = Open();
        return await c.QueryFirstOrDefaultAsync<DailySalesDto>(
            """
            SELECT branch_id AS "BranchId", business_date AS "BusinessDate", total_sales AS "TotalSales",
                   total_discount AS "TotalDiscount", total_tax AS "TotalTax", total_tips AS "TotalTips",
                   order_count AS "OrderCount", by_payment_method AS "ByPaymentMethod"
            FROM reporting.daily_sales
            WHERE restaurant_id = @restaurantId AND branch_id = @branchId AND business_date = @date
            """,
            new { restaurantId, branchId, date }, commandType: CommandType.Text);
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(long restaurantId, long branchId, DateOnly date, int limit, CancellationToken ct = default)
    {
        using var c = Open();
        var rows = await c.QueryAsync<TopProductDto>(
            """
            SELECT product_name AS "ProductName", SUM(quantity) AS "Quantity", SUM(total) AS "Total"
            FROM reporting.daily_product_sales
            WHERE restaurant_id = @restaurantId AND branch_id = @branchId AND business_date = @date
            GROUP BY product_name
            ORDER BY SUM(total) DESC
            LIMIT @limit
            """,
            new { restaurantId, branchId, date, limit });
        return rows.AsList();
    }

    public async Task<IReadOnlyList<WaiterSalesDto>> GetByWaiterAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default)
    {
        using var c = Open();
        var rows = await c.QueryAsync<WaiterSalesDto>(
            """
            SELECT waiter_id AS "WaiterId", total AS "Total", tips AS "Tips", order_count AS "OrderCount"
            FROM reporting.daily_waiter_sales
            WHERE restaurant_id = @restaurantId AND branch_id = @branchId AND business_date = @date
            ORDER BY total DESC
            """,
            new { restaurantId, branchId, date });
        return rows.AsList();
    }

    public async Task<TipsDto?> GetTipsAsync(long restaurantId, long branchId, DateOnly date, CancellationToken ct = default)
    {
        using var c = Open();
        return await c.QueryFirstOrDefaultAsync<TipsDto>(
            """
            SELECT business_date AS "BusinessDate", total_tips AS "TotalTips"
            FROM reporting.daily_sales
            WHERE restaurant_id = @restaurantId AND branch_id = @branchId AND business_date = @date
            """,
            new { restaurantId, branchId, date });
    }
}


public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) => value switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => DateOnly.FromDateTime(Convert.ToDateTime(value))
    };
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}
