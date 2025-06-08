using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Authorization;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AssignRoleCommandHandler> _logger;

    public AssignRoleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AssignRoleCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning role {RoleId} to user {UserId} by {CurrentUser}",
            request.RoleId, request.UserId, _currentUserService.Email);

        // Get the user and role
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && r.IsActive, cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException($"Role with ID {request.RoleId} not found or is inactive");
        }

        // Check if user already has this role
        if (user.UserRoles.Any(ur => ur.RoleId == request.RoleId))
        {
            _logger.LogInformation("User {UserId} already has role {RoleId}", request.UserId, request.RoleId);
            return true; // Already assigned, consider it successful
        }

        // Get current user's roles to validate permission to assign this role
        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (currentUser == null)
        {
            throw new InvalidOperationException("Current user not found");
        }

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
            throw new UnauthorizedAccessException($"You do not have permission to assign the role {role.Name}");
        }

        // Assign the role
        user.AssignRole(role);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} assigned successfully to user {UserId}", request.RoleId, request.UserId);

        return true;
    }
}