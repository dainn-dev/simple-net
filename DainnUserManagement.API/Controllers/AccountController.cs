using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Dtos;
using DainnUser.PostgreSQL.Domain.Entities;

namespace DainnUserManagement.API.Controllers;

/// <summary>
/// Controller for account management operations.
/// Provides endpoints for users to manage their own account information, profile updates, password changes, and two-factor authentication setup.
/// </summary>
/// <remarks>
/// All endpoints in this controller require authentication via JWT Bearer token.
/// Users can only access and modify their own account information.
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Account")]
[Produces("application/json")]
[Consumes("application/json")]
public class AccountController(IUserService userService, ITwoFactorService twoFactorService, UserManager<AppUser> userManager) : ControllerBase
{

    /// <summary>
    /// Gets the current authenticated user's profile information.
    /// </summary>
    /// <returns>The complete user profile including email, full name, avatar URL, roles, and permissions.</returns>
    /// <remarks>
    /// This endpoint retrieves the profile information for the currently authenticated user.
    /// The user is identified from the JWT token claims.
    /// 
    /// The response includes:
    /// - User identification (ID, Email)
    /// - Profile information (FullName, AvatarUrl)
    /// - Account status (IsActive, EmailConfirmed, TwoFactorEnabled)
    /// - Security information (LastLoginAt, CreatedAt, UpdatedAt)
    /// - Roles and permissions assigned to the user
    /// 
    /// The response is cached for 60 seconds to improve performance.
    /// </remarks>
    /// <response code="200">Successfully retrieved the user profile.</response>
    /// <response code="401">The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="404">The user ID from the token does not exist in the system.</response>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await userService.GetUserProfileAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>
    /// Updates the current authenticated user's profile information.
    /// </summary>
    /// <param name="dto">The profile update data containing full name and optional avatar URL.</param>
    /// <returns>The updated user profile with all current information.</returns>
    /// <remarks>
    /// This endpoint allows users to update their profile information. Only the fields provided in the request body will be updated.
    /// 
    /// Updateable fields:
    /// - FullName: The user's display name
    /// - AvatarUrl: Optional URL to the user's avatar image
    /// 
    /// Note: Email addresses cannot be changed through this endpoint. Use a separate email change endpoint if available.
    /// 
    /// The user must be authenticated and can only update their own profile.
    /// </remarks>
    /// <response code="200">Profile updated successfully. Returns the complete updated user profile.</response>
    /// <response code="400">Invalid request data or validation errors. Check the ProblemDetails for specific validation errors.</response>
    /// <response code="401">The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="404">The user ID from the token does not exist in the system.</response>
    [HttpPut("profile")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await userService.UpdateProfileAsync(userId.Value, dto);
        return Ok(result);
    }

    /// <summary>
    /// Changes the current authenticated user's password.
    /// </summary>
    /// <param name="dto">The password change data containing current password, new password, and confirmation.</param>
    /// <returns>Success response indicating the password was changed.</returns>
    /// <remarks>
    /// This endpoint allows authenticated users to change their password. The user must provide:
    /// - Current password for verification
    /// - New password (must meet minimum security requirements)
    /// - Confirmation of the new password
    /// 
    /// Password requirements:
    /// - Minimum length: 8 characters
    /// - May include additional complexity requirements based on system configuration
    /// 
    /// After a successful password change, the user may need to re-authenticate with the new password.
    /// </remarks>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Invalid request data or validation errors. Check that the new password meets all requirements.</response>
    /// <response code="401">The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="422">The current password provided is incorrect. User must provide the correct current password to proceed.</response>
    [HttpPut("password")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        await userService.ChangePasswordAsync(userId.Value, dto);
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Initiates two-factor authentication (2FA) setup for the current authenticated user.
    /// </summary>
    /// <returns>The 2FA setup information including QR code data and manual entry key.</returns>
    /// <remarks>
    /// This endpoint initiates the two-factor authentication setup process. It returns:
    /// - A QR code in base64 format that can be scanned with authenticator apps (Google Authenticator, Microsoft Authenticator, etc.)
    /// - A manual entry key that can be entered manually if QR code scanning is not possible
    /// 
    /// After calling this endpoint:
    /// 1. Scan the QR code with your authenticator app OR manually enter the key
    /// 2. The authenticator app will generate time-based one-time passwords (TOTP)
    /// 3. Call the /verify endpoint with a code from your authenticator app to complete the setup
    /// 
    /// Note: This endpoint enables 2FA on the user account. The 2FA will be fully activated after successful verification.
    /// </remarks>
    /// <response code="200">2FA setup information returned successfully. Use this data to configure your authenticator app.</response>
    /// <response code="401">The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="404">The user ID from the token does not exist in the system.</response>
    [HttpPost("2fa/setup")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(TwoFactorSetupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TwoFactorSetupDto>> Setup2FA()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var result = await twoFactorService.Setup2FAAsync(user);
        
        // Enable 2FA on the user
        user.TwoFactorEnabled = true;
        await userManager.UpdateAsync(user);

        return Ok(result);
    }

    /// <summary>
    /// Verifies and completes two-factor authentication setup for the current authenticated user.
    /// </summary>
    /// <param name="dto">The 2FA verification data containing the TOTP code from the authenticator app.</param>
    /// <returns>Success response indicating 2FA has been verified and enabled.</returns>
    /// <remarks>
    /// This endpoint verifies that the user has correctly configured their authenticator app by validating a code generated by the app.
    /// 
    /// Steps to use:
    /// 1. First call /setup to get the QR code and setup information
    /// 2. Scan the QR code or manually enter the key into your authenticator app
    /// 3. Get a 6-digit code from your authenticator app
    /// 4. Call this endpoint with the code to verify and complete setup
    /// 
    /// After successful verification, two-factor authentication will be fully enabled for the user account.
    /// The user will be required to provide a 2FA code during future login attempts.
    /// 
    /// The verification code is time-sensitive (typically valid for 30-60 seconds) and can only be used once.
    /// </remarks>
    /// <response code="200">2FA verified and enabled successfully. The user must now provide 2FA codes during login.</response>
    /// <response code="400">Invalid 2FA code. The code may be expired, already used, or incorrectly entered. Generate a new code from your authenticator app.</response>
    /// <response code="401">The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="404">The user ID from the token does not exist in the system.</response>
    [HttpPost("2fa/verify")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Verify2FA([FromBody] Verify2FADto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var isValid = await twoFactorService.Verify2FACodeAsync(user, dto.Code);
        if (!isValid)
        {
            return BadRequest(new { message = "Invalid 2FA code" });
        }

        return Ok(new { message = "2FA verified and enabled successfully" });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}

