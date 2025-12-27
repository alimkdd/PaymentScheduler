using Mapster;
using PaymentScheduler.Application.DTO.Response;
using PaymentScheduler.Domain.Enums;
using PaymentScheduler.Domain.Models;

namespace PaymentScheduler.Application.Mapper;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Payment, PaymentResponseModel>()
            .MapWith(src => new PaymentResponseModel(
                src.Id,
                src.PayeeName,
                src.Description,
                src.Amount,
                (PaymentFrequency)src.FrequencyId,
                src.NextExecutionDate,
                src.LastExecutionDate,
                (PaymentStatus)src.StatusId,
                src.ConsecutiveFailures,
                src.CreatedAt,
                src.UpdatedAt ?? DateTime.MinValue
            ));
    }
}