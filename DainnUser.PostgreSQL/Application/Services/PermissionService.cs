using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Domain.Entities;
using DainnCommon.Exceptions;
using DainnUser.PostgreSQL.Infrastructure.Persistence;

namespace DainnUser.PostgreSQL.Application.Services;

/// <summary>
/// Service for permission management operations.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        UserManager<AppUser> userManager,
        AppDbContext context,
        ILogger<PermissionService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all permissions assigned to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of user permission names.</returns>
    public virtual async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Get permissions from roles
        var permissions = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .Where(rp => userRoles.Contains(rp.Role.Name!))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public virtual async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionName);
    }
}
