using DainnUser.PostgreSQL.Application.Dtos;
using FluentValidation;

namespace DainnUser.PostgreSQL.Application.Validators;

/// <summary>
/// Validator for ChangePasswordDto.
/// </summary>
public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");
    }
}

