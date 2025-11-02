using DainnUserManagement.Application.Dtos;
using System.Security.Claims;

namespace DainnUserManagement.Application.Interfaces;

/// <summary>
/// Interface for OAuth2 external authentication operations.
/// </summary>
public interface IOAuth2Service
{
    /// <summary>
    /// Processes an external login callback and returns authentication tokens and user profile.
    /// </summary>
    /// <param name="provider">The OAuth2 provider name (e.g., "Google", "Microsoft", "Facebook", "GitHub").</param>
    /// <param name="principal">The claims principal from the external authentication result.</param>
    /// <returns>The external login response containing tokens and user profile.</returns>
    Task<ExternalLoginResponseDto> ProcessExternalLoginAsync(string provider, ClaimsPrincipal principal);
}

