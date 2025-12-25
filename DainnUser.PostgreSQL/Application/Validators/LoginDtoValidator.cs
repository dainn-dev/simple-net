using DainnUser.PostgreSQL.Application.Dtos;
using FluentValidation;

namespace DainnUser.PostgreSQL.Application.Validators;

/// <summary>
/// Validator for LoginDto.
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

