using PaymentScheduler.Domain.Enums;

namespace PaymentScheduler.Application.DTO.Request;

public record UpdatePaymentRequestModel(
   int PaymentId,
   Guid UserSid,
   string PayeeName,
   string Title,
   string Description,
   string DestinationAccount,
   decimal? Amount,
   PaymentFrequency? Frequency,
   DateTime? NextExecutionDate
);