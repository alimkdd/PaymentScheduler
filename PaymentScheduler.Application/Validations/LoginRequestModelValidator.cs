using FluentValidation;
using PaymentScheduler.Application.DTO.Request;

namespace PaymentScheduler.Application.Validations;

public class LoginRequestModelValidator : AbstractValidator<LoginRequestModel>
{
    public LoginRequestModelValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.")
            .MaximumLength(100)
            .WithMessage("Password cannot exceed 100 characters.");
    }
}