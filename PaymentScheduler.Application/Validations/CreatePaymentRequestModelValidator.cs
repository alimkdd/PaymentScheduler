using FluentValidation;
using PaymentScheduler.Application.DTO.Request;

namespace PaymentScheduler.Application.Validations;

public class CreatePaymentRequestModelValidator : AbstractValidator<CreatePaymentRequestModel>
{
    public CreatePaymentRequestModelValidator()
    {
        RuleFor(x => x.UserSid)
            .NotEmpty()
            .WithMessage("UserSid is required.");

        RuleFor(x => x.PayeeName)
            .NotEmpty()
            .WithMessage("PayeeName is required.")
            .MaximumLength(100)
            .WithMessage("PayeeName cannot exceed 100 characters.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.DestinationAccount)
            .NotEmpty()
            .WithMessage("DestinationAccount is required.")
            .MaximumLength(50)
            .WithMessage("DestinationAccount cannot exceed 50 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow.AddDays(-1))
            .WithMessage("StartDate must be in the future.");
    }
}