namespace DainnUser.PostgreSQL.Application.Interfaces;

/// <summary>
/// Interface for permission management operations.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permissions assigned to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of user permission names.</returns>
    Task<List<string>> GetUserPermissionsAsync(Guid userId);

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    Task<bool> HasPermissionAsync(Guid userId, string permissionName);
}

