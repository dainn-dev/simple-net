using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Application.Dtos;
using DainnUserManagement.Domain.Entities;

namespace DainnUserManagement.API.Controllers.Admin;

/// <summary>
/// Controller for administrative user management operations.
/// Provides endpoints for administrators to manage user accounts, including viewing, updating, deleting, locking, and importing/exporting users.
/// </summary>
/// <remarks>
/// This controller provides comprehensive user management capabilities for administrators:
/// - View paginated list of all users
/// - View detailed user information
/// - Update user profiles
/// - Delete user accounts
/// - Lock/unlock user accounts
/// - Export users for backup
/// - Import users in bulk
/// 
/// All endpoints require administrator privileges (Admin policy).
/// Users managed through this controller may include roles and permissions information.
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/admin/users")]
[ApiVersion("1.0")]
[Authorize(Policy = "Admin")]
[Tags("Admin - Users")]
[Produces("application/json")]
[Consumes("application/json")]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;

    public UsersController(UserManager<AppUser> userManager, IUserService userService, IPermissionService permissionService, IRoleService roleService)
    {
        _userManager = userManager;
        _userService = userService;
        _permissionService = permissionService;
        _roleService = roleService;
    }

    /// <summary>
    /// Gets a paginated list of all users in the system.
    /// </summary>
    /// <param name="page">The page number to retrieve (starting from 1). Default is 1.</param>
    /// <param name="pageSize">The number of users per page. Default is 20. Maximum recommended is 100.</param>
    /// <returns>A paginated response containing the list of users and pagination metadata.</returns>
    /// <remarks>
    /// This endpoint retrieves a paginated list of all registered users in the system.
    /// 
    /// Each user object includes:
    /// - Basic information: ID, Email, FullName, AvatarUrl
    /// - Account status: IsActive, EmailConfirmed, TwoFactorEnabled
    /// - Timestamps: CreatedAt, UpdatedAt, LastLoginAt, EmailConfirmedAt
    /// - Security information: Roles and Permissions assigned to the user
    /// 
    /// Pagination:
    /// - Use the 'page' parameter to navigate through pages
    /// - Use the 'pageSize' parameter to control how many users are returned per page
    /// - The response includes total count and total pages for building pagination UI
    /// 
    /// Performance:
    /// - Results are cached for 60 seconds to improve response times
    /// - Large page sizes may impact performance; keep pageSize reasonable (≤100)
    /// </remarks>
    /// <response code="200">Successfully retrieved the paginated list of users.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    [HttpGet]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(PaginatedResponse<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResponse<UserProfileDto>>> GetAll(int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;
        var users = await _userManager.Users
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _userManager.Users.CountAsync();

        var userDtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = await _roleService.GetUserRolesAsync(user.Id);
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            
            userDtos.Add(new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                EmailConfirmedAt = user.EmailConfirmedAt,
                LastLoginAt = user.LastLoginAt,
                TwoFactorEnabled = user.TwoFactorEnabled,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = roles,
                Permissions = permissions
            });
        }

        return Ok(new PaginatedResponse<UserProfileDto>
        {
            Items = userDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>
    /// Gets detailed information for a specific user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user to retrieve.</param>
    /// <returns>The complete user profile including roles and permissions.</returns>
    /// <remarks>
    /// This endpoint retrieves comprehensive information about a specific user.
    /// 
    /// The response includes:
    /// - User identification and profile information
    /// - Account status and security settings
    /// - Complete list of roles assigned to the user
    /// - Complete list of permissions (direct and inherited from roles)
    /// - Account timestamps (creation, update, last login, email confirmation)
    /// 
    /// Use this endpoint when you need detailed user information for administrative purposes,
    /// such as troubleshooting account issues or reviewing user access levels.
    /// 
    /// Results are cached for 60 seconds to improve performance.
    /// </remarks>
    /// <response code="200">Successfully retrieved the user profile.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User not found. The specified user ID does not exist in the system.</response>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetById(Guid id)
    {
        var result = await _userService.GetUserProfileAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Updates a user's profile information as an administrator.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user to update.</param>
    /// <param name="dto">The profile update data containing full name and optional avatar URL.</param>
    /// <returns>The updated user profile with all current information.</returns>
    /// <remarks>
    /// This endpoint allows administrators to update any user's profile information.
    /// 
    /// Updateable fields:
    /// - FullName: The user's display name
    /// - AvatarUrl: Optional URL to the user's avatar image
    /// 
    /// Note: This is an administrative operation. Regular users should use the Account controller
    /// to update their own profiles. Email addresses typically cannot be changed through this endpoint.
    /// 
    /// After update:
    /// - The UpdatedAt timestamp is automatically updated
    /// - Changes are immediately reflected in the system
    /// - The user may need to refresh their session to see changes
    /// </remarks>
    /// <response code="200">User profile updated successfully. Returns the complete updated user profile.</response>
    /// <response code="400">Invalid request data or validation errors. Check the ProblemDetails for specific validation errors.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User not found. The specified user ID does not exist in the system.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> Update(Guid id, [FromBody] UpdateProfileDto dto)
    {
        var result = await _userService.UpdateProfileAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Permanently deletes a user account from the system.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user to delete.</param>
    /// <returns>Success response confirming the user has been deleted.</returns>
    /// <remarks>
    /// This endpoint permanently removes a user account from the system.
    /// 
    /// ⚠️ Warning: This is a destructive operation that cannot be undone.
    /// 
    /// What gets deleted:
    /// - User account and profile information
    /// - User roles and permissions associations
    /// - All user-related data (depending on database cascade rules)
    /// 
    /// What happens:
    /// - The user will no longer be able to login
    /// - All sessions for this user are invalidated
    /// - Related data may be deleted or orphaned based on system configuration
    /// 
    /// Before deleting:
    /// - Consider exporting user data for backup using the export endpoint
    /// - Verify the user ID is correct
    /// - Ensure you have proper authorization and business justification
    /// 
    /// Note: Some systems implement soft delete (account deactivation) instead of hard delete.
    /// Check if there's a lock/deactivate endpoint that might be more appropriate.
    /// </remarks>
    /// <response code="200">User deleted successfully. The account has been permanently removed from the system.</response>
    /// <response code="400">Failed to delete user. There may be database constraints or related data preventing deletion. Check the ProblemDetails for specific errors.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User not found. The specified user ID does not exist in the system.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "User deleted successfully" });
    }

    /// <summary>
    /// Locks a user account to prevent login and access.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user to lock.</param>
    /// <param name="dto">The lock data containing lockout reason and optional expiration date.</param>
    /// <returns>Success response confirming the user account has been locked.</returns>
    /// <remarks>
    /// This endpoint locks a user account, preventing the user from logging in or accessing the system.
    /// 
    /// Lock behavior:
    /// - User cannot login with their credentials
    /// - Existing sessions may be invalidated
    /// - Lock can be temporary (with expiration) or permanent
    /// - Lockout reason is typically logged for audit purposes
    /// 
    /// Common use cases:
    /// - Security incident or suspected account compromise
    /// - User request for account suspension
    /// - Policy violation
    /// - Temporary suspension pending investigation
    /// 
    /// Lock types:
    /// - Temporary: Account is locked until a specified date/time
    /// - Permanent: Account is locked indefinitely until manually unlocked
    /// 
    /// Note: Locked accounts can typically be unlocked by administrators using an unlock endpoint (if available).
    /// </remarks>
    /// <response code="200">User locked successfully. The account is now locked and the user cannot login.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User not found. The specified user ID does not exist in the system.</response>
    [HttpPost("{id}/lock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> LockUser(Guid id, [FromBody] LockUserDto dto)
    {
        await _userService.LockUserAsync(dto);
        return Ok(new { message = "User locked successfully" });
    }

    /// <summary>
    /// Exports all users to JSON format for backup, migration, or data analysis.
    /// </summary>
    /// <returns>A list of user data in export format, typically returned as JSON.</returns>
    /// <remarks>
    /// This endpoint exports all user accounts in the system to a standardized format.
    /// 
    /// Export includes:
    /// - User profile information (ID, Email, FullName, etc.)
    /// - Account status and timestamps
    /// - Security-related information (excluding sensitive data like passwords)
    /// - Roles and permissions assigned to users
    /// 
    /// Use cases:
    /// - Data backup and disaster recovery
    /// - System migration to another platform
    /// - Data analysis and reporting
    /// - Compliance and audit requirements
    /// 
    /// Security notes:
    /// - Passwords and password hashes are NOT included in the export
    /// - Sensitive authentication tokens are excluded
    /// - Export may contain personal information - handle according to privacy policies
    /// - Large user databases may result in large export files
    /// 
    /// Format:
    /// The export is returned as a JSON array of user objects in a standardized format
    /// that can be imported back using the import endpoint.
    /// </remarks>
    /// <response code="200">Export successful. Returns a JSON array of all users in export format.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    [HttpGet("export")]
    [ProducesResponseType(typeof(List<ExportUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ExportUserDto>>> ExportUsers()
    {
        var users = await _userService.ExportUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Imports users from a JSON array for bulk user creation.
    /// </summary>
    /// <param name="users">The list of users to import, formatted according to the ImportUserDto schema.</param>
    /// <param name="skipExisting">Whether to skip users that already exist based on email address. Default is true. If false, existing users may be updated.</param>
    /// <returns>A summary of imported users including the count and list of successfully imported users.</returns>
    /// <remarks>
    /// This endpoint allows bulk import of users into the system, typically from a previous export or migration source.
    /// 
    /// Import process:
    /// 1. System validates each user in the import list
    /// 2. For each user:
    ///    - If skipExisting is true: Checks if user with same email exists, skips if found
    ///    - If skipExisting is false: May update existing users (behavior depends on implementation)
    ///    - Creates new users that don't exist
    /// 3. Returns summary of imported users
    /// 
    /// Import format:
    /// - Users must be provided as JSON array in the request body
    /// - Each user must match the ImportUserDto schema
    /// - Email addresses are used as unique identifiers
    /// 
    /// Validation:
    /// - Email format is validated
    /// - Password requirements must be met (if passwords are included)
    /// - Required fields must be provided
    /// - Invalid users are skipped with errors logged
    /// 
    /// Performance:
    /// - Large imports (1000+ users) may take significant time
    /// - Consider importing in batches for better performance
    /// - Transaction behavior: May be all-or-nothing or per-user depending on configuration
    /// 
    /// Security:
    /// - Passwords should be provided in a secure format or reset after import
    /// - Imported users may need email confirmation depending on system settings
    /// </remarks>
    /// <response code="200">Users imported successfully. Returns the count and list of imported users.</response>
    /// <response code="400">Invalid request data. The import format is invalid, or validation errors occurred. Check the ProblemDetails for specific errors.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(List<ExportUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ExportUserDto>>> ImportUsers([FromBody] List<ImportUserDto> users, [FromQuery] bool skipExisting = true)
    {
        var importedUsers = await _userService.ImportUsersAsync(users, skipExisting);
        return Ok(new { imported = importedUsers.Count, users = importedUsers });
    }
}

/// <summary>
/// Represents a paginated response.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Gets or sets the items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages.
    /// </summary>
    public int TotalPages { get; set; }
}

