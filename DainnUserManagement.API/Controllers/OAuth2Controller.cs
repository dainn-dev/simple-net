using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Dtos;
using DainnCommon.Exceptions;

namespace DainnUserManagement.API.Controllers;

/// <summary>
/// Controller for OAuth2 external authentication providers.
/// Provides endpoints for authenticating users through third-party providers like Google, Microsoft, Facebook, GitHub, etc.
/// </summary>
/// <remarks>
/// This controller implements OAuth2 authentication flow for external identity providers:
/// - Google OAuth2
/// - Microsoft/Azure AD
/// - Facebook Login
/// - GitHub OAuth
/// - Apple Sign In (if configured)
/// 
/// Authentication flow:
/// 1. User initiates authentication by calling /{provider}/challenge
/// 2. System redirects to provider's login page
/// 3. User authenticates with provider
/// 4. Provider redirects back to /{provider}/callback
/// 5. System processes external login and returns JWT tokens
/// 
/// If the user doesn't exist, a new account is automatically created using information from the provider.
/// All endpoints in this controller are publicly accessible (no authentication required).
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Tags("OAuth2")]
[Produces("application/json")]
public class OAuth2Controller(
    IOAuth2Service oauth2Service,
    ILogger<OAuth2Controller> logger) : ControllerBase
{

    /// <summary>
    /// Initiates OAuth2 authentication flow with the specified external provider.
    /// </summary>
    /// <param name="provider">The OAuth2 provider name. Supported providers: "Google", "Microsoft", "Facebook", "GitHub", "Apple".</param>
    /// <returns>A challenge result that redirects the user to the provider's login page.</returns>
    /// <remarks>
    /// This endpoint initiates the OAuth2 authentication flow by redirecting the user to the external provider's login page.
    /// 
    /// Supported providers:
    /// - Google: Google OAuth2 authentication
    /// - Microsoft: Microsoft Account or Azure AD authentication
    /// - Facebook: Facebook Login authentication
    /// - GitHub: GitHub OAuth2 authentication
    /// - Apple: Apple Sign In (if configured)
    /// 
    /// Flow:
    /// 1. Client calls this endpoint with the provider name
    /// 2. Server redirects user to provider's login page
    /// 3. User authenticates with the provider
    /// 4. Provider redirects to /{provider}/callback
    /// 5. Callback endpoint processes the authentication and returns JWT tokens
    /// 
    /// Note: This endpoint performs a redirect and is typically used in browser-based flows.
    /// For mobile or API clients, consider implementing a different OAuth2 flow (e.g., Authorization Code with PKCE).
    /// </remarks>
    /// <response code="302">Redirects to the external provider's authentication page.</response>
    /// <response code="400">Invalid provider name. The specified provider is not configured or not supported.</response>
    [HttpGet("{provider}/challenge")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult Challenge([FromRoute] string provider)
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback), "OAuth2", new { provider }, Request.Scheme)
        };

        return Challenge(authenticationProperties, provider);
    }

    /// <summary>
    /// Handles the OAuth2 callback from external providers and authenticates the user.
    /// </summary>
    /// <param name="provider">The OAuth2 provider name that initiated the callback (e.g., "Google", "Microsoft", "Facebook", "GitHub").</param>
    /// <returns>JWT access token, refresh token, and user profile information.</returns>
    /// <remarks>
    /// This endpoint is called by the external OAuth2 provider after the user has authenticated.
    /// 
    /// Process:
    /// 1. External provider redirects to this endpoint after user authentication
    /// 2. System validates the OAuth2 response from the provider
    /// 3. System retrieves user information from the provider (email, name, etc.)
    /// 4. System creates a new user account if one doesn't exist, or authenticates existing user
    /// 5. System issues JWT tokens for the user
    /// 
    /// Account creation:
    /// - If user doesn't exist: A new account is created automatically using information from the provider
    /// - Email is automatically confirmed if provided by the provider
    /// - User profile is populated with available information (name, avatar, etc.)
    /// 
    /// Security:
    /// - This endpoint is rate-limited to prevent abuse
    /// - OAuth2 state parameter is validated to prevent CSRF attacks
    /// - Provider tokens are validated before processing
    /// 
    /// Note: This endpoint is typically called automatically by the provider and should not be called directly by clients.
    /// </remarks>
    /// <response code="200">Authentication successful. Returns JWT tokens and user profile. Use the access token in the Authorization header for subsequent requests.</response>
    /// <response code="400">Invalid authentication request. The OAuth2 response from the provider is invalid or the state parameter doesn't match.</response>
    /// <response code="401">Authentication failed. The user declined authorization or the provider authentication failed.</response>
    /// <response code="500">Internal server error. An error occurred during the authentication process.</response>
    [HttpGet("{provider}/callback")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginRegisterPolicy")]
    [ProducesResponseType(typeof(ExternalLoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExternalLoginResponseDto>> Callback([FromRoute] string provider)
        => await HandleProviderCallback(provider);

    /// <summary>
    /// Generic handler for provider OAuth2 callbacks.
    /// </summary>
    private async Task<ActionResult<ExternalLoginResponseDto>> HandleProviderCallback(string provider)
    {
        try
        {
            var authResult = await HttpContext.AuthenticateAsync(provider);
            if (!authResult.Succeeded)
            {
                logger.LogWarning("{Provider} OAuth2 authentication failed", provider);
                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication failed",
                    Detail = $"Failed to authenticate with {provider}",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            // Process external login using the service
            var response = await oauth2Service.ProcessExternalLoginAsync(provider, authResult.Principal!);

            // Sign out from the external authentication scheme
            await HttpContext.SignOutAsync(provider);

            return Ok(response);
        }
        catch (BusinessRuleException ex)
        {
            logger.LogError(ex, "Business rule violation during {Provider} OAuth2 callback", provider);
            throw; // Let middleware handle the response
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation during {Provider} OAuth2 callback", provider);
            throw new BusinessRuleException($"Invalid authentication: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during {Provider} OAuth2 callback", provider);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal server error",
                Detail = "An error occurred during authentication",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}

