using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Dtos;

namespace DainnUserManagement.API.Controllers;

/// <summary>
/// Controller for email verification operations.
/// Provides endpoints for confirming user email addresses after registration.
/// </summary>
/// <remarks>
/// This controller handles email confirmation workflows:
/// - Users receive an email with a confirmation token after registration
/// - Users call this endpoint with the token to verify their email address
/// - Email confirmation is required for full account activation in some systems
/// 
/// All endpoints in this controller are publicly accessible (no authentication required).
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Email Verification")]
[Produces("application/json")]
[Consumes("application/json")]
public class EmailController(IUserService userService) : ControllerBase
{

    /// <summary>
    /// Confirms a user's email address using a confirmation token.
    /// </summary>
    /// <param name="dto">The email confirmation data containing user ID and confirmation token.</param>
    /// <returns>Success response indicating the email has been confirmed.</returns>
    /// <remarks>
    /// This endpoint verifies a user's email address using a confirmation token that was sent to their email.
    /// 
    /// Email confirmation flow:
    /// 1. User registers an account
    /// 2. System sends a confirmation email with a unique token
    /// 3. User clicks the link in the email or manually provides the token
    /// 4. This endpoint validates the token and confirms the email
    /// 
    /// After successful confirmation:
    /// - The user's EmailConfirmed status is set to true
    /// - The EmailConfirmedAt timestamp is recorded
    /// - The user gains full access to all account features
    /// 
    /// Security notes:
    /// - Confirmation tokens are time-limited (typically expire after 24-48 hours)
    /// - Tokens are single-use and cannot be reused
    /// - Invalid or expired tokens will result in an error
    /// </remarks>
    /// <response code="200">Email confirmed successfully. The user's email address has been verified and the account is fully activated.</response>
    /// <response code="400">Invalid or expired confirmation token. The token may have expired, already been used, or is malformed. Request a new confirmation email if needed.</response>
    /// <response code="404">User not found. The user ID provided does not exist in the system.</response>
    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        await userService.ConfirmEmailAsync(dto);
        return Ok(new { message = "Email confirmed successfully" });
    }
}

