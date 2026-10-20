namespace PaymentsService.Domain.Events;

public record PaymentRegistrationResult(long PaymentId, Guid PublicId, string? TicketNumber);