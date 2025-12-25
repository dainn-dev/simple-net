using DainnUser.PostgreSQL.Application.Dtos;

namespace DainnUser.PostgreSQL.Application.Interfaces;

/// <summary>
/// Interface for user management operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The registration data.</param>
    /// <returns>The registered user profile.</returns>
    Task<UserProfileDto> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Authenticates a user and performs login.
    /// </summary>
    /// <param name="dto">The login credentials.</param>
    /// <returns>The authentication token response.</returns>
    Task<TokenResponseDto> LoginAsync(LoginDto dto);

    /// <summary>
    /// Refreshes an authentication token using a refresh token.
    /// </summary>
    /// <param name="dto">The refresh token data.</param>
    /// <returns>The new authentication token response.</returns>
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto);

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dto">The profile update data.</param>
    /// <returns>The updated user profile.</returns>
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dto">The password change data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

    /// <summary>
    /// Locks a user account.
    /// </summary>
    /// <param name="dto">The lock user data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LockUserAsync(LockUserDto dto);

    /// <summary>
    /// Gets a user's profile information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The user profile.</returns>
    Task<UserProfileDto> GetUserProfileAsync(Guid userId);

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <param name="dto">The email confirmation data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConfirmEmailAsync(ConfirmEmailDto dto);

    /// <summary>
    /// Initiates a password reset flow.
    /// </summary>
    /// <param name="dto">The forgot password data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ForgotPasswordAsync(ForgotPasswordDto dto);

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="dto">The password reset data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetPasswordAsync(ResetPasswordDto dto);

    /// <summary>
    /// Exports all users to a list for backup or migration purposes.
    /// </summary>
    /// <returns>A list of exported user data.</returns>
    Task<List<ExportUserDto>> ExportUsersAsync();

    /// <summary>
    /// Imports users from a list of user data.
    /// </summary>
    /// <param name="users">The list of users to import.</param>
    /// <param name="skipExisting">Whether to skip users that already exist (true) or throw an exception (false).</param>
    /// <returns>A list of successfully imported users.</returns>
    Task<List<ExportUserDto>> ImportUsersAsync(List<ImportUserDto> users, bool skipExisting = true);
}

