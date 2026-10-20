using System.ComponentModel.DataAnnotations;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.CashSessions;

public sealed class OpenCashSessionRequest
{
    [Required]
    public long CashRegisterId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OpeningFloat { get; set; }

    public string? Notes { get; set; }
}

public sealed class CloseCashSessionRequest
{
    [Required]
    public decimal CountedAmount { get; set; }

    public string? Notes { get; set; }
}

public sealed record CashSessionDto(
    long Id,
    Guid PublicId,
    long CashRegisterId,
    string? CashRegisterCode,
    string? CashRegisterName,
    string Status,
    decimal OpeningFloat,
    decimal ExpectedAmount,
    decimal? CountedAmount,
    decimal? Difference,
    string? Notes,
    long OpenedBy,
    DateTimeOffset OpenedAt,
    long? ClosedBy,
    DateTimeOffset? ClosedAt)
{
    public static CashSessionDto From(CashSession s, decimal expectedAmount) => new(
        s.Id, s.PublicId, s.CashRegisterId, s.CashRegister?.Code, s.CashRegister?.Name,
        s.Status.ToString(), s.OpeningFloat, expectedAmount,
        s.CountedAmount, s.Difference, s.Notes,
        s.OpenedBy, s.OpenedAt, s.ClosedBy, s.ClosedAt);
}

public sealed record OpenCashSessionResponse(long Id, long CashRegisterId, decimal OpeningFloat, DateTimeOffset OpenedAt);

public sealed record CloseCashSessionResponse(long Id, decimal ExpectedAmount, decimal CountedAmount, decimal Difference, string Status);