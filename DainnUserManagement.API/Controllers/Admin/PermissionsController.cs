using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DainnUserManagement.Application.Interfaces;

namespace DainnUserManagement.API.Controllers.Admin;

/// <summary>
/// Controller for administrative permission management operations.
/// Provides endpoints for administrators to view and manage user permissions.
/// </summary>
/// <remarks>
/// This controller handles permission-related operations:
/// - View all permissions assigned to a specific user
/// - Permissions include both direct permissions and those inherited from roles
/// 
/// Permission model:
/// - Permissions are fine-grained access control units (e.g., "read:users", "write:content")
/// - Users can have permissions directly assigned or inherited through roles
/// - All permissions from roles and direct assignments are combined for the user
/// 
/// All endpoints require administrator privileges (Admin policy).
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/admin/permissions")]
[ApiVersion("1.0")]
[Authorize(Policy = "Admin")]
[Tags("Admin - Permissions")]
[Produces("application/json")]
[Consumes("application/json")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Gets all permissions for a specific user, including direct permissions and those inherited from roles.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the user whose permissions to retrieve.</param>
    /// <returns>A list of permission names that the user has access to.</returns>
    /// <remarks>
    /// This endpoint retrieves all permissions granted to a specific user.
    /// 
    /// Permission sources:
    /// - Direct permissions: Permissions assigned directly to the user
    /// - Role permissions: Permissions inherited from all roles assigned to the user
    /// - Combined: All permissions from both sources are merged into a single list
    /// 
    /// Permission format:
    /// - Permissions are typically strings in format like "resource:action" (e.g., "users:read", "content:write")
    /// - Each permission represents a specific capability or access right
    /// - Duplicate permissions are removed (user doesn't get duplicate permissions from multiple roles)
    /// 
    /// Use cases:
    /// - Audit what permissions a user has
    /// - Debug authorization issues
    /// - Verify user access before performing sensitive operations
    /// - Display user capabilities in admin UI
    /// 
    /// Performance:
    /// - Results are cached for 60 seconds to improve response times
    /// - Permission calculation includes role traversal and permission aggregation
    /// 
    /// Note: The returned list represents the effective permissions - all permissions the user actually has,
    /// regardless of whether they came from roles or direct assignment.
    /// </remarks>
    /// <response code="200">Successfully retrieved the list of user permissions.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User not found. The specified user ID does not exist in the system.</response>
    [HttpGet("user/{id}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<string>>> GetUserPermissions(Guid id)
    {
        var permissions = await _permissionService.GetUserPermissionsAsync(id);
        return Ok(permissions);
    }
}

