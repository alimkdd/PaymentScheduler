namespace PaymentScheduler.Application.DTO.Response;

public record PaymentExecutionResponse(
     Guid Id,
     decimal Amount,
     DateTime ExecutedAt,
     bool Success,
     string FailureReason
);