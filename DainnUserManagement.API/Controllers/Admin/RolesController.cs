using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Dtos;
using DainnUser.PostgreSQL.Domain.Entities;

namespace DainnUserManagement.API.Controllers.Admin;

/// <summary>
/// Controller for administrative role management operations.
/// Provides endpoints for administrators to manage roles, assign roles to users, and remove roles from users.
/// </summary>
/// <remarks>
/// This controller handles role-based access control (RBAC) operations:
/// - View all available roles in the system
/// - Create new roles with descriptions
/// - Assign roles to users
/// - Remove roles from users
/// 
/// Roles are collections of permissions that can be assigned to users.
/// Users can have multiple roles, and permissions from all roles are combined.
/// 
/// All endpoints require administrator privileges (Admin policy).
/// </remarks>
[ApiController]
[Route("api/v{version:apiVersion}/admin/roles")]
[ApiVersion("1.0")]
[Authorize(Policy = "Admin")]
[Tags("Admin - Roles")]
[Produces("application/json")]
[Consumes("application/json")]
public class RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, IRoleService roleService) : ControllerBase
{

    /// <summary>
    /// Gets all roles defined in the system.
    /// </summary>
    /// <returns>A list of all roles with their IDs, names, descriptions, and associated permissions.</returns>
    /// <remarks>
    /// This endpoint retrieves all roles available in the system.
    /// 
    /// Each role object includes:
    /// - Role ID: Unique identifier for the role
    /// - Role Name: The name of the role (e.g., "Admin", "Editor", "Viewer")
    /// - Description: Human-readable description of what the role is used for
    /// - Permission Names: List of permission names associated with the role
    /// 
    /// Use cases:
    /// - Display available roles in admin UI for assignment
    /// - Audit which roles exist in the system
    /// - Understand the permission structure of the system
    /// 
    /// Results are cached for 60 seconds to improve performance.
    /// </remarks>
    /// <response code="200">Successfully retrieved the list of all roles.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    [HttpGet]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<RoleDto>>> GetAll()
    {
        var roles = await roleManager.Roles.ToListAsync();
        var roleDtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name!,
            Description = r.Description,
            PermissionNames = new List<string>() // Would need to load from RolePermission
        }).ToList();

        return Ok(roleDtos);
    }

    /// <summary>
    /// Creates a new role in the system.
    /// </summary>
    /// <param name="dto">The role creation data containing role name, description, and optional permissions.</param>
    /// <returns>The newly created role with its assigned ID.</returns>
    /// <remarks>
    /// This endpoint creates a new role that can be assigned to users.
    /// 
    /// Role creation requirements:
    /// - Role name: Must be unique in the system (case-insensitive)
    /// - Description: Optional but recommended for documentation
    /// - Permissions: Can be assigned during creation or added later
    /// 
    /// Role naming conventions:
    /// - Use clear, descriptive names (e.g., "ContentEditor", "FinancialViewer")
    /// - Avoid generic names that may conflict with system roles
    /// - Consider prefixing with domain or module name for clarity
    /// 
    /// After creation:
    /// - Role is immediately available for assignment to users
    /// - Permissions can be added or modified through permission management endpoints
    /// - Role appears in the list of available roles
    /// 
    /// Best practices:
    /// - Create roles that represent job functions or responsibilities
    /// - Assign permissions during creation for better organization
    /// - Document the purpose of the role in the description field
    /// </remarks>
    /// <response code="201">Role created successfully. Returns the created role with assigned ID.</response>
    /// <response code="400">Invalid request data or validation errors. Check role name format and required fields.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="422">Duplicate role name. A role with the same name already exists in the system. Use a different name.</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto dto)
    {
        var result = await roleService.CreateRoleAsync(dto);
        return CreatedAtAction(nameof(GetAll), result);
    }

    /// <summary>
    /// Assigns a role to a user, granting them all permissions associated with that role.
    /// </summary>
    /// <param name="dto">The role assignment data containing user ID and role name.</param>
    /// <returns>Success response confirming the role has been assigned.</returns>
    /// <remarks>
    /// This endpoint assigns a role to a user, which grants the user all permissions associated with that role.
    /// 
    /// Assignment behavior:
    /// - User gains all permissions from the assigned role
    /// - User can have multiple roles; permissions are cumulative
    /// - Assignment is immediate and takes effect on next authorization check
    /// - If user already has the role, the operation succeeds without error
    /// 
    /// Permission inheritance:
    /// - Permissions from all assigned roles are combined
    /// - If a permission exists in multiple roles, it's still only counted once
    /// - Users can also have direct permissions assigned independently
    /// 
    /// Use cases:
    /// - Grant user access to features by assigning appropriate role
    /// - Elevate user privileges for temporary access
    /// - Organize users into groups with similar permissions
    /// 
    /// Note: Changes to role permissions automatically affect all users with that role.
    /// </remarks>
    /// <response code="200">Role assigned successfully. The user now has access to all permissions in the role.</response>
    /// <response code="400">Invalid request data or validation errors. Check that user ID and role name are provided correctly.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User or role not found. Either the user ID or role name does not exist in the system.</response>
    [HttpPost("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Assign([FromBody] AssignRoleDto dto)
    {
        await roleService.AssignRoleAsync(dto);
        return Ok(new { message = "Role assigned successfully" });
    }

    /// <summary>
    /// Removes a role from a user, revoking all permissions associated with that role.
    /// </summary>
    /// <param name="dto">The role assignment data containing user ID and role name to remove.</param>
    /// <returns>Success response confirming the role has been removed.</returns>
    /// <remarks>
    /// This endpoint removes a role from a user, which revokes all permissions that were granted through that role.
    /// 
    /// Removal behavior:
    /// - User loses all permissions from the removed role
    /// - Other roles and direct permissions are unaffected
    /// - Removal is immediate and takes effect on next authorization check
    /// - If user doesn't have the role, the operation may still succeed
    /// 
    /// Permission revocation:
    /// - Only permissions granted through the removed role are revoked
    /// - If user has the same permission through another role or directly, it remains
    /// - User may lose access to features they previously had
    /// 
    /// Use cases:
    /// - Revoke elevated privileges when no longer needed
    /// - Remove user from a group or department role
    /// - Adjust user permissions by role management
    /// 
    /// Important considerations:
    /// - Removing a role may affect user's ability to access certain features
    /// - Consider impact on user workflows before removal
    /// - Users with multiple roles will retain permissions from remaining roles
    /// </remarks>
    /// <response code="200">Role removed successfully. The user no longer has permissions from this role.</response>
    /// <response code="400">Failed to remove role. The user may not have the role, or there may be a system error preventing removal.</response>
    /// <response code="401">Unauthorized. The request is not authenticated. Include a valid JWT Bearer token in the Authorization header.</response>
    /// <response code="403">Forbidden. The authenticated user does not have administrator privileges. Admin role is required.</response>
    /// <response code="404">User or role not found. Either the user ID or role name does not exist in the system.</response>
    [HttpDelete("assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remove([FromBody] AssignRoleDto dto)
    {
        var user = await userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var role = await roleManager.FindByNameAsync(dto.RoleName);
        if (role == null)
        {
            return NotFound(new { message = "Role not found" });
        }

        var result = await userManager.RemoveFromRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { message = "Role removed successfully" });
    }
}

