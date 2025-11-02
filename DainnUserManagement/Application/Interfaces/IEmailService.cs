namespace DainnUserManagement.Application.Interfaces;

/// <summary>
/// Interface for email sending operations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email confirmation message to a user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="confirmationToken">The email confirmation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendConfirmationEmailAsync(string email, string confirmationToken);

    /// <summary>
    /// Sends a password reset email to a user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendPasswordResetAsync(string email, string resetToken);
}

