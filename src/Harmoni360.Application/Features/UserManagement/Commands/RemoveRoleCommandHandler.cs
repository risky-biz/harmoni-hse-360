using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Authorization;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveRoleCommandHandler> _logger;

    public RemoveRoleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<RemoveRoleCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId} by {CurrentUser}",
            request.RoleId, request.UserId, _currentUserService.Email);

        // Get the user role assignment
        var userRole = await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

        if (userRole == null)
        {
            _logger.LogInformation("User {UserId} does not have role {RoleId}", request.UserId, request.RoleId);
            return true; // Already removed, consider it successful
        }

        // Get current user's roles to validate permission to remove this role
        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (currentUser == null)
        {
            throw new InvalidOperationException("Current user not found");
        }

        // Check if the current user can remove this role
        var currentUserRoleTypes = currentUser.UserRoles.Select(ur => ur.Role.RoleType);
        var canRemove = false;

        foreach (var currentRoleType in currentUserRoleTypes)
        {
            if (ModulePermissionMap.CanAssignRole(currentRoleType, userRole.Role.RoleType))
            {
                canRemove = true;
                break;
            }
        }

        if (!canRemove)
        {
            throw new UnauthorizedAccessException($"You do not have permission to remove the role {userRole.Role.Name}");
        }

        // Remove the role assignment
        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role {RoleId} removed successfully from user {UserId}", request.RoleId, request.UserId);

        return true;
    }
}