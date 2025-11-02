using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Application.Dtos;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DainnUserManagement.API.Controllers;

/// <summary>
/// Controller for authentication and user registration operations.
/// Provides endpoints for user registration, login, token refresh, and logout functionality.
/// </summary>
/// <remarks>
/// This controller handles the core authentication flow:
/// - User registration with email and password
/// - User login with credentials to obtain JWT tokens
/// - Token refresh to obtain new access tokens
/// - User logout
/// 
/// Rate limiting is applied to registration and login endpoints to prevent abuse.
/// All endpoints are publicly accessible (no authentication required) except logout.
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("Authentication")]
[Produces("application/json")]
[Consumes("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account in the system.
    /// </summary>
    /// <param name="dto">The registration data containing email, password, and full name.</param>
    /// <returns>The registered user profile with account information.</returns>
    /// <remarks>
    /// This endpoint creates a new user account in the system. 
    /// 
    /// Registration requirements:
    /// - Email: Must be a valid email address format and unique in the system
    /// - Password: Must be at least 8 characters long
    /// - FullName: Required display name for the user
    /// 
    /// After successful registration:
    /// - The user account is created with an unconfirmed email status
    /// - The user should check their email for a confirmation link
    /// - The user can login immediately, but some features may be restricted until email is confirmed
    /// 
    /// This endpoint is rate-limited to prevent abuse and spam account creation.
    /// </remarks>
    /// <response code="200">User registered successfully. Returns the created user profile.</response>
    /// <response code="400">Invalid request data or validation errors. Check the ProblemDetails for specific field validation errors.</response>
    /// <response code="422">Business rule violation. Typically occurs when the email address already exists in the system. Check the ProblemDetails for details.</response>
    [HttpPost("register")]
    [EnableRateLimiting("LoginRegisterPolicy")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<UserProfileDto>> Register([FromBody] RegisterDto dto)
    {
        _logger.LogDebug("Register request received: Email={Email}, FullName={FullName}", dto?.Email, dto?.FullName);
        
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}") ?? Enumerable.Empty<string>());
            _logger.LogWarning("ModelState is invalid. Errors: {Errors}", string.Join(", ", errors));
            return BadRequest(ModelState);
        }
        
        var result = await _userService.RegisterAsync(dto!);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user with email and password credentials and returns JWT tokens.
    /// </summary>
    /// <param name="dto">The login credentials containing email and password.</param>
    /// <returns>JWT access token and refresh token for authenticated requests.</returns>
    /// <remarks>
    /// This endpoint authenticates a user and issues JWT tokens for accessing protected endpoints.
    /// 
    /// Authentication flow:
    /// 1. User provides email and password
    /// 2. System validates credentials
    /// 3. If valid, system issues:
    ///    - Access token: Short-lived token (typically 15-60 minutes) for API requests
    ///    - Refresh token: Long-lived token for obtaining new access tokens
    /// 
    /// Security features:
    /// - Account lockout: After multiple failed login attempts, the account may be temporarily locked
    /// - Two-factor authentication: If 2FA is enabled, an additional verification step may be required
    /// - Rate limiting: This endpoint is rate-limited to prevent brute force attacks
    /// 
    /// Token usage:
    /// - Include the access token in the Authorization header as: "Bearer {access_token}"
    /// - Use the refresh token to obtain new access tokens when the current one expires
    /// </remarks>
    /// <response code="200">Login successful. Returns JWT access token and refresh token. Use the access token in the Authorization header for subsequent requests.</response>
    /// <response code="400">Invalid request data or validation errors. Check email and password format.</response>
    /// <response code="401">Authentication failed. Invalid email or password credentials provided.</response>
    /// <response code="423">Account is locked. The account has been temporarily locked due to too many failed login attempts. Wait for the lockout period to expire or contact support.</response>
    [HttpPost("login")]
    [EnableRateLimiting("LoginRegisterPolicy")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await _userService.LoginAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="dto">The refresh token obtained from a previous login or token refresh.</param>
    /// <returns>New JWT access token and refresh token pair.</returns>
    /// <remarks>
    /// This endpoint allows users to obtain a new access token without re-authenticating with credentials.
    /// 
    /// When to use:
    /// - Access token has expired (typically after 15-60 minutes)
    /// - User wants to extend their session without logging in again
    /// 
    /// How it works:
    /// 1. Send the refresh token received during login or previous refresh
    /// 2. System validates the refresh token
    /// 3. If valid, system issues new access token and refresh token
    /// 
    /// Security notes:
    /// - Refresh tokens have a longer expiration period (typically days or weeks)
    /// - Refresh tokens should be stored securely (e.g., httpOnly cookies in web applications)
    /// - Each refresh may invalidate the previous refresh token (rotation) for enhanced security
    /// - Invalid or expired refresh tokens will result in authentication failure
    /// </remarks>
    /// <response code="200">Token refreshed successfully. Returns new access token and refresh token. Update your stored tokens with the new values.</response>
    /// <response code="400">Invalid or expired refresh token. The refresh token may have expired, been revoked, or is malformed. User must login again.</response>
    /// <response code="401">Unauthorized. The refresh token is invalid or the request format is incorrect.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _userService.RefreshTokenAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Logs out the current authenticated user.
    /// </summary>
    /// <returns>Success response confirming logout.</returns>
    /// <remarks>
    /// This endpoint invalidates the current user session and revokes the access token.
    /// 
    /// Note: Since JWT tokens are stateless, this endpoint primarily serves as a signal to the client
    /// that the user has logged out. The tokens themselves remain valid until they expire naturally.
    /// For true token invalidation, the system may maintain a token blacklist or revocation list.
    /// 
    /// Best practices:
    /// - Clients should discard stored tokens after calling this endpoint
    /// - Clients should not use the access token for subsequent requests after logout
    /// - For enhanced security, refresh tokens may also be revoked on logout
    /// </remarks>
    /// <response code="200">Logout successful. The user session has been terminated. Discard stored tokens.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }
}

