using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PaymentScheduler.Application.DTO.Request;
using PaymentScheduler.Application.DTO.Response;
using PaymentScheduler.Application.Interface;
using PaymentScheduler.Application.Interface.Common;
using PaymentScheduler.Domain.Enums;
using PaymentScheduler.Domain.Models;
using PaymentScheduler.Infrastructure.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaymentScheduler.Application.Services;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IProviderRepository _providerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(
        AppDbContext dbContext,
        IConfiguration configuration,
        IProviderRepository providerRepository,
        IMapper mapper,
        ILogger<PaymentRepository> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _providerRepository = providerRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<PaymentResponseModel>> GetAllPayments()
    {
        try
        {
            var payments = await _dbContext.Payments
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

            return _mapper.Map<List<PaymentResponseModel>>(payments);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<PaymentResponseModel> GetPaymentById(int paymentId)
    {
        try
        {
            var payment = await _dbContext.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

            return _mapper.Map<PaymentResponseModel>(payment);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<string> Login(LoginRequestModel request)
    {
        try
        {
            // Attempt to find user with matching credentials.
            var user = await _dbContext.UserAccounts
                            .Where(u => u.UserName == request.Username && u.Password == request.Password)
                            .FirstOrDefaultAsync();

            if (user == null) return null;

            // Retrieve JWT configuration settings.
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            // Create claims for the token.
            var claims = new[] { new Claim(ClaimTypes.Sid, user.Sid.ToString()) };

            // Generate signing credentials.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create JWT token.
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            // Return serialized JWT token.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<PaymentResponseModel> CreatePayment(CreatePaymentRequestModel request)
    {
        try
        {
            // Define allowed amount range.
            const decimal minAmount = 1m;
            const decimal maxAmount = 10000m;

            // Validate amount boundaries.
            if (request.Amount < minAmount)
                throw new ArgumentException($"Amount must be at least {minAmount}.");

            if (request.Amount > maxAmount)
                throw new ArgumentException($"Amount cannot exceed {maxAmount}.");

            // Validate payment frequency.
            if (!Enum.IsDefined(typeof(PaymentFrequency), request.Frequency))
                throw new ArgumentException("Unsupported frequency.");

            // Validate start date (cannot be in the past).
            if (request.StartDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Start date cannot be in the past.");

            // Create payment entity.
            var paymentEntity = new Payment
            {
                UserSid = request.UserSid,
                PayeeName = request.PayeeName,
                Title = request.Title,
                Description = request.Description,
                DestinationAccount = request.DestinationAccount,
                Amount = request.Amount,
                FrequencyId = (int)request.Frequency,
                NextExecutionDate = _providerRepository.AdjustForHolidaysAndWeekends(request.StartDate),
                StatusId = (int)PaymentStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            // Add and persist new payment.
            _dbContext.Payments.Add(paymentEntity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<PaymentResponseModel>(paymentEntity);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<PaymentResponseModel> UpdatePayment(UpdatePaymentRequestModel request)
    {
        try
        {
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId);

            if (payment == null) return null;

            // Prevent updates to cancelled payments.
            if (payment.StatusId == (int)PaymentStatus.Cancelled)
                throw new InvalidOperationException("Cannot update cancelled payment.");

            const decimal minAmount = 1m;
            const decimal maxAmount = 10000m;

            // Validate and update amount if provided.
            if (request.Amount.HasValue)
            {
                if (request.Amount.Value < minAmount)
                    throw new ArgumentException($"Amount must be at least {minAmount}.");

                if (request.Amount.Value > maxAmount)
                    throw new ArgumentException($"Amount cannot exceed {maxAmount}.");

                payment.Amount = request.Amount.Value;
            }

            // Update optional fields if provided.
            payment.PayeeName = request.PayeeName ?? payment.PayeeName;
            payment.Description = request.Description ?? payment.Description;

            // Update frequency if provided and valid.
            if (request.Frequency.HasValue)
            {
                if (!Enum.IsDefined(typeof(PaymentFrequency), request.Frequency.Value))
                    throw new ArgumentException("Unsupported frequency.");

                payment.FrequencyId = (int)request.Frequency.Value;
            }

            // Validate and update next execution date.
            if (request.NextExecutionDate.HasValue)
            {
                if (request.NextExecutionDate.Value < DateTime.UtcNow.Date)
                    throw new ArgumentException("Next execution date cannot be in the past.");

                payment.NextExecutionDate = _providerRepository.AdjustForHolidaysAndWeekends(request.NextExecutionDate.Value);
            }

            // Update timestamp.
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated recurring payment {PaymentId} for user {UserId}", payment.Id);

            return _mapper.Map<PaymentResponseModel>(payment);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> DeletePayment(int paymentId)
    {
        try
        {
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null) return false;

            // Soft delete: mark as cancelled.
            payment.StatusId = (int)PaymentStatus.Cancelled;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Cancelled recurring payment {PaymentId}", payment.Id);

            return true;
        }
        catch (Exception)
        {

            throw;
        }
    }
}