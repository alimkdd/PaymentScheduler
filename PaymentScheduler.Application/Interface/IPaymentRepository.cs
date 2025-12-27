using PaymentScheduler.Application.DTO.Request;
using PaymentScheduler.Application.DTO.Response;

namespace PaymentScheduler.Application.Interface;

public interface IPaymentRepository
{
    Task<List<PaymentResponseModel>> GetAllPayments();

    Task<PaymentResponseModel> GetPaymentById(int paymentId);

    Task<string> Login(LoginRequestModel reqeust);

    Task<PaymentResponseModel> CreatePayment(CreatePaymentRequestModel request);

    Task<PaymentResponseModel> UpdatePayment(UpdatePaymentRequestModel request);

    Task<bool> DeletePayment(int paymentId);
}