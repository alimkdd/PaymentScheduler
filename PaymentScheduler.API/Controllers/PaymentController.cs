using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentScheduler.Application.DTO.Request;
using PaymentScheduler.Application.DTO.Response;
using PaymentScheduler.Application.Interface;

namespace PaymentScheduler.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RecurringPaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;

    public RecurringPaymentsController(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    #region GET

    [HttpGet("GetAll")]
    public async Task<List<PaymentResponseModel>> GetAll()
        => await _paymentRepository.GetAllPayments();

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentResponseModel>> GetById(int id)
        => await _paymentRepository.GetPaymentById(id);

    #endregion

    #region POST PUT DELETE

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<string> Login([FromBody] LoginRequestModel request)
        => await _paymentRepository.Login(request);

    [HttpPost("Create")]
    public async Task<ActionResult<PaymentResponseModel>> Create([FromBody] CreatePaymentRequestModel request)
        => await _paymentRepository.CreatePayment(request);


    [HttpPut("Update")]
    public async Task<ActionResult<PaymentResponseModel>> Update([FromBody] UpdatePaymentRequestModel request)
        => await _paymentRepository.UpdatePayment(request);

    [HttpDelete("Delete")]
    public async Task<bool> Delete(int id)
        => await _paymentRepository.DeletePayment(id);

    #endregion
}