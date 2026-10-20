namespace PaymentsService.Application.CashRegisters;

public record CashRegisterDto(long Id, Guid PublicId, string Code, string Name, bool IsActive);