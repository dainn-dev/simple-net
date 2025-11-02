using DainnUserManagement.Application.Dtos;

namespace DainnUserManagement.Application.Interfaces;

/// <summary>
/// Interface for role management operations.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="dto">The role creation data.</param>
    /// <returns>The created role.</returns>
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="dto">The role assignment data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AssignRoleAsync(AssignRoleDto dto);

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of user roles.</returns>
    Task<List<string>> GetUserRolesAsync(Guid userId);

    /// <summary>
    /// Gets a role by name.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>The role DTO if found, null otherwise.</returns>
    Task<RoleDto?> GetRoleByNameAsync(string roleName);
}

