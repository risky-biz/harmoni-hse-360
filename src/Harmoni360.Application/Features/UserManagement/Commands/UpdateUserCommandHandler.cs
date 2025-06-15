using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Domain.Authorization;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user: {UserId} by {CurrentUser}",
            request.UserId, _currentUserService.Email);

        // Get the user to update
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        // Get current user's roles to validate role assignments
        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (currentUser == null)
        {
            throw new InvalidOperationException("Current user not found");
        }

        // Update user profile
        user.UpdateProfile(request.Name, request.Department, request.Position);

        // Update active status
        if (request.IsActive != user.IsActive)
        {
            if (request.IsActive)
                user.Activate();
            else
                user.Deactivate();
        }

        // Handle role assignments
        if (request.RoleIds.Any())
        {
            // Remove existing roles (we'll reassign them)
            var existingUserRoles = user.UserRoles.ToList();
            foreach (var existingRole in existingUserRoles)
            {
                _context.UserRoles.Remove(existingRole);
            }

            // Get new roles
            var newRoles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id) && r.IsActive)
                .ToListAsync(cancellationToken);

            // Assign new roles (with permission validation)
            foreach (var role in newRoles)
            {
                // Check if the current user can assign this role
                var currentUserRoleTypes = currentUser.UserRoles.Select(ur => ur.Role.RoleType);
                var canAssign = false;

                foreach (var currentRoleType in currentUserRoleTypes)
                {
                    if (ModulePermissionMap.CanAssignRole(currentRoleType, role.RoleType))
                    {
                        canAssign = true;
                        break;
                    }
                }

                if (!canAssign)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to assign role {RoleType} without permission", 
                        _currentUserService.UserId, role.RoleType);
                    continue; // Skip this role assignment
                }

                user.AssignRole(role);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User updated successfully with ID: {UserId}", user.Id);

        // Reload user with updated roles
        var updatedUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            EmployeeId = user.EmployeeId,
            Department = user.Department,
            Position = user.Position,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastModifiedAt = user.LastModifiedAt,
            Roles = updatedUser?.UserRoles.Select(ur => new UserRoleDto
            {
                RoleId = ur.Role.Id,
                RoleName = ur.Role.Name,
                RoleType = ur.Role.RoleType,
                Description = ur.Role.Description,
                AssignedAt = ur.CreatedAt
            }).ToList() ?? new List<UserRoleDto>()
        };
    }
}