using DainnUserManagement.Application.Interfaces;

namespace DainnUserManagement.Application.Services;

/// <summary>
/// Service for sending emails.
/// </summary>
public class EmailService : IEmailService
{
    /// <summary>
    /// Sends an email confirmation message to a user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="confirmationToken">The email confirmation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task SendConfirmationEmailAsync(string email, string confirmationToken)
    {
        // Stub implementation - consumer applications should override this
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a password reset email to a user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task SendPasswordResetAsync(string email, string resetToken)
    {
        // Stub implementation - consumer applications should override this
        return Task.CompletedTask;
    }
}

