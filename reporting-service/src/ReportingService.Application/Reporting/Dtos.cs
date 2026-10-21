namespace ReportingService.Application.Reporting;

public sealed record DailySalesDto(
    long BranchId, DateOnly BusinessDate, decimal TotalSales, decimal TotalDiscount,
    decimal TotalTax, decimal TotalTips, int OrderCount, string ByPaymentMethod);

public sealed record TopProductDto(string ProductName, decimal Quantity, decimal Total);

public sealed record WaiterSalesDto(long WaiterId, decimal Total, decimal Tips, int OrderCount);

public sealed record TipsDto(DateOnly BusinessDate, decimal TotalTips);
