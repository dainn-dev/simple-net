using DainnUserManagement.Application.Dtos;
using FluentValidation;

namespace DainnUserManagement.Application.Validators;

/// <summary>
/// Validator for RegisterDto.
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required");
    }
}

