using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentScheduler.Application.Interface;
using PaymentScheduler.Application.Interface.Common;
using PaymentScheduler.Domain.Enums;
using PaymentScheduler.Domain.Models;
using PaymentScheduler.Infrastructure.Context;

namespace PaymentScheduler.Application.Services.Hangfire;

public class PaymentJob : IPaymentJob
{
    private readonly AppDbContext _dbContext;
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<PaymentJob> _logger;

    public PaymentJob(AppDbContext dbContext, IProviderRepository providerRepository, ILogger<PaymentJob> logger)
    {
        _dbContext = dbContext;
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public async Task ProcessDuePayments()
    {
        var duePayments = await _dbContext.Payments
                                .Where(p => p.Status.Name == PaymentStatus.Active.ToString() &&
                                            p.NextExecutionDate <= DateTime.UtcNow)
                                .ToListAsync();

        _logger.LogInformation(
            "Found {Count} due payments to process at {Time}. Payment IDs: {PaymentIds}",
            duePayments.Count,
            DateTime.UtcNow,
            string.Join(",", duePayments.Select(p => p.Id))
        );

        foreach (var payment in duePayments)
        {
            await ProcessPayment(payment);
        }
    }

    private async Task ProcessPayment(Payment payment)
    {
        var execution = new PaymentExecution
        {
            RecurringPaymentId = payment.Id,
            PaymentId = payment.Id,
            Amount = payment.Amount,
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            var userAccount = await _dbContext.UserAccounts
                .FirstOrDefaultAsync(u => u.Sid == payment.UserSid)
                ?? throw new Exception("User account not found.");

            // Check for sufficient funds
            if (userAccount.Balance < payment.Amount)
            {
                execution.Success = false;
                execution.FailureReason = "Insufficient funds";
                payment.ConsecutiveFailures++;

                // Retry next day (adjust for holidays/weekends)
                var retryDate = DateTime.UtcNow.Date.AddDays(1);
                payment.NextExecutionDate = _providerRepository.AdjustForHolidaysAndWeekends(retryDate);

                _logger.LogWarning(
                    "Payment {PaymentId} for user {UserId} failed: insufficient funds",
                    payment.Id,
                    payment.UserSid
                );
            }
            else
            {
                // Debit user and send payment
                userAccount.Balance -= payment.Amount;
                execution.Success = true;
                payment.ConsecutiveFailures = 0;
                payment.LastExecutionDate = DateTime.UtcNow;

                // Calculate next execution date based on frequency
                payment.NextExecutionDate = _providerRepository.CalculateNextExecutionDate(
                    payment.NextExecutionDate,
                    (PaymentFrequency)payment.FrequencyId
                );
                payment.NextExecutionDate = _providerRepository.AdjustForHolidaysAndWeekends(payment.NextExecutionDate);

                _logger.LogInformation(
                    "Payment {PaymentId} for user {UserId} executed successfully",
                    payment.Id,
                    payment.UserSid
                );
            }

            // Pause after 3 consecutive failures
            if (payment.ConsecutiveFailures >= 3)
            {
                payment.Status.Name = PaymentStatus.Paused.ToString();
                _logger.LogWarning(
                    "Payment {PaymentId} for user {UserId} paused after 3 consecutive failures",
                    payment.Id,
                    payment.UserSid
                );
            }

            _dbContext.PaymentExecutions.Add(execution);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            execution.Success = false;
            execution.FailureReason = $"System error: {ex.Message}";
            payment.ConsecutiveFailures++;

            if (payment.ConsecutiveFailures >= 3)
                payment.Status.Name = PaymentStatus.Paused.ToString();

            _dbContext.PaymentExecutions.Add(execution);
            await _dbContext.SaveChangesAsync();

            _logger.LogError(
                ex,
                "Error processing payment {PaymentId} for user {UserId}",
                payment.Id,
                payment.UserSid
            );
        }
    }
}