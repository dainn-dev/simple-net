using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Dtos;

namespace DainnUserManagement.API.Controllers;

/// <summary>
/// Controller for password recovery and reset operations.
/// Provides endpoints for users to reset forgotten passwords and recover access to their accounts.
/// </summary>
/// <remarks>
/// This controller handles password recovery workflows:
/// - Forgot password: Initiates password reset by sending a reset token via email
/// - Reset password: Allows users to set a new password using a reset token
/// 
/// Security features:
/// - Reset tokens are time-limited (typically expire after 1-24 hours)
/// - Reset tokens are single-use and cannot be reused
/// - Email is always sent to registered email address (even if account doesn't exist) to prevent user enumeration
/// 
/// All endpoints in this controller are publicly accessible (no authentication required).
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Password Recovery")]
[Produces("application/json")]
[Consumes("application/json")]
public class PasswordController(IUserService userService) : ControllerBase
{

    /// <summary>
    /// Initiates a password reset flow by sending a reset token to the user's email address.
    /// </summary>
    /// <param name="dto">The forgot password data containing the user's email address.</param>
    /// <returns>Success response indicating a reset email has been sent (if account exists).</returns>
    /// <remarks>
    /// This endpoint initiates the password reset process for users who have forgotten their password.
    /// 
    /// Process:
    /// 1. User provides their registered email address
    /// 2. System validates the email format
    /// 3. If account exists, system generates a secure reset token
    /// 4. System sends an email with a password reset link containing the token
    /// 5. User clicks the link and is redirected to reset password page
    /// 
    /// Security considerations:
    /// - For privacy and security, the response is always the same whether the email exists or not
    /// - This prevents user enumeration attacks (checking which emails are registered)
    /// - Reset tokens expire after a limited time (typically 1-24 hours)
    /// - Reset tokens are single-use only
    /// 
    /// Email content:
    /// The email sent to the user contains:
    /// - A secure password reset link
    /// - Instructions for resetting the password
    /// - Security warnings about not sharing the link
    /// </remarks>
    /// <response code="200">If the email exists, a password reset email has been sent. The response is always the same to prevent user enumeration.</response>
    /// <response code="400">Invalid request data. The email address format is invalid or missing.</response>
    [HttpPost("forgot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await userService.ForgotPasswordAsync(dto);
        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// </summary>
    /// <param name="dto">The password reset data containing user ID, reset token, and new password.</param>
    /// <returns>Success response indicating the password has been reset.</returns>
    /// <remarks>
    /// This endpoint allows users to set a new password after receiving a reset token via email.
    /// 
    /// Reset flow:
    /// 1. User receives password reset email with a token
    /// 2. User navigates to password reset page (from email link)
    /// 3. User provides new password and confirmation
    /// 4. This endpoint validates the token and updates the password
    /// 
    /// Token validation:
    /// - Reset token must be valid and not expired
    /// - Reset token must match the user ID provided
    /// - Reset tokens are single-use and cannot be reused
    /// 
    /// Password requirements:
    /// - Minimum length: 8 characters
    /// - May include additional complexity requirements (uppercase, lowercase, numbers, symbols)
    /// - New password cannot be the same as the current password
    /// 
    /// After successful reset:
    /// - User can immediately login with the new password
    /// - All existing sessions may be invalidated for security
    /// - User may be required to re-authenticate on all devices
    /// </remarks>
    /// <response code="200">Password reset successfully. The user can now login with the new password.</response>
    /// <response code="400">Invalid request data or expired/invalid reset token. The token may have expired, been used already, or is malformed. Request a new password reset if needed.</response>
    /// <response code="404">User not found. The user ID provided does not exist in the system.</response>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await userService.ResetPasswordAsync(dto);
        return Ok(new { message = "Password reset successfully" });
    }
}

