using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DainnUserManagement.Application.Dtos;
using DainnUserManagement.Application.Events;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Domain.Entities;
using DainnUserManagement.Infrastructure.Exceptions;
using DainnUserManagement.Infrastructure.Persistence;

namespace DainnUserManagement.Application.Services;

/// <summary>
/// Service for role management operations.
/// </summary>
public class RoleService : IRoleService
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        AppDbContext context,
        IEventPublisher eventPublisher,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="dto">The role creation data.</param>
    /// <returns>The created role.</returns>
    public virtual async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        // Check if role already exists
        var existingRole = await _roleManager.FindByNameAsync(dto.Name);
        if (existingRole != null)
        {
            throw new BusinessRuleException("A role with this name already exists.");
        }

        var role = new AppRole
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            throw new BusinessRuleException($"Failed to create role: {string.Join(", ", errors)}");
        }

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            PermissionNames = new List<string>()
        };
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="dto">The role assignment data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User", dto.UserId);
        }

        var role = await _roleManager.FindByNameAsync(dto.RoleName);
        if (role == null)
        {
            throw new NotFoundException("Role", dto.RoleName);
        }

        // Check if user already has this role
        var userRoles = await _userManager.GetRolesAsync(user);
        if (userRoles.Contains(dto.RoleName))
        {
            return; // Already assigned
        }

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            throw new BusinessRuleException($"Failed to assign role: {string.Join(", ", errors)}");
        }

        // Publish event
        await _eventPublisher.PublishAsync(new RoleAssignedEvent
        {
            UserId = user.Id,
            RoleName = role.Name!
        });
    }

    /// <summary>
    /// Gets all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of user roles.</returns>
    public virtual async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    /// <summary>
    /// Gets a role by name.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>The role DTO if found, null otherwise.</returns>
    public virtual async Task<RoleDto?> GetRoleByNameAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return null;
        }

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            Description = role.Description,
            PermissionNames = new List<string>()
        };
    }
}
