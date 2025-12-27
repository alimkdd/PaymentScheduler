namespace PaymentScheduler.Application.Interface;

public interface IPaymentJob
{
    Task ProcessDuePayments();
}