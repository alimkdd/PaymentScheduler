namespace PaymentScheduler.Application.DTO.Response;

public record PaymentResultResponseModel(
    bool Success,
    string FailureReason
);