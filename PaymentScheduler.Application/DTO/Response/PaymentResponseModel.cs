using PaymentScheduler.Domain.Enums;

namespace PaymentScheduler.Application.DTO.Response;

public record PaymentResponseModel(
    int Id,
    string PayeeName,
    string Description,
    decimal Amount,
    PaymentFrequency Frequency,
    DateTime NextExecutionDate,
    DateTime? LastExecutionDate,
    PaymentStatus Status,
    int ConsecutiveFailures,
    DateTime CreatedAt,
    DateTime UpdatedAt
);