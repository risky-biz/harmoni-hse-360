using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Domain.Authorization;
using Harmoni360.Domain.Enums;

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

        // Update user profile with HSSE fields
        user.UpdateProfile(
            request.Name, 
            request.Department, 
            request.Position,
            request.PhoneNumber,
            request.WorkLocation,
            request.CostCenter);

        // Update emergency contact
        user.UpdateEmergencyContact(request.EmergencyContactName, request.EmergencyContactPhone);
        
        // Update supervisor
        user.UpdateSupervisor(request.SupervisorEmployeeId);
        
        // Update preferences
        user.UpdatePreferences(request.PreferredLanguage, request.TimeZone);
        
        // Update hire date
        user.UpdateHireDate(request.HireDate);

        // Update MFA requirement
        if (request.RequiresMFA)
            user.EnableMFA();
        else
            user.DisableMFA();

        // Update status
        user.ChangeStatus(request.Status);

        // Handle role assignments
        if (request.RoleIds.Any())
        {
            _logger.LogInformation("Updating roles for user {UserId}. New roles: {RoleIds}", 
                request.UserId, string.Join(", ", request.RoleIds));

            // Remove existing roles (we'll reassign them)
            var existingUserRoles = user.UserRoles.ToList();
            foreach (var existingRole in existingUserRoles)
            {
                user.RemoveRole(existingRole.RoleId);
            }

            // Get new roles
            var newRoles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id) && r.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {NewRoleCount} active roles to assign", newRoles.Count);

            // Assign new roles (with permission validation)
            var currentUserRoleTypes = currentUser.UserRoles.Select(ur => ur.Role.RoleType).ToList();
            var isCurrentUserSuperAdmin = currentUserRoleTypes.Contains(RoleType.SuperAdmin);

            foreach (var role in newRoles)
            {
                // SuperAdmin can assign any role, others need permission checks
                var canAssign = isCurrentUserSuperAdmin;

                if (!canAssign)
                {
                    foreach (var currentRoleType in currentUserRoleTypes)
                    {
                        if (ModulePermissionMap.CanAssignRole(currentRoleType, role.RoleType))
                        {
                            canAssign = true;
                            break;
                        }
                    }
                }

                if (!canAssign)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to assign role {RoleType} without permission", 
                        _currentUserService.UserId, role.RoleType);
                    continue; // Skip this role assignment
                }

                _logger.LogInformation("Assigning role {RoleName} (ID: {RoleId}) to user {UserId}", 
                    role.Name, role.Id, request.UserId);
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
            Status = user.Status,
            
            // HSSE-specific fields
            PhoneNumber = user.PhoneNumber,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            SupervisorEmployeeId = user.SupervisorEmployeeId,
            HireDate = user.HireDate,
            WorkLocation = user.WorkLocation,
            CostCenter = user.CostCenter,
            
            // Security fields
            RequiresMFA = user.RequiresMFA,
            LastPasswordChange = user.LastPasswordChange,
            LastLoginAt = user.LastLoginAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            AccountLockedUntil = user.AccountLockedUntil,
            
            // User preferences
            PreferredLanguage = user.PreferredLanguage,
            TimeZone = user.TimeZone,
            
            // Audit fields
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            LastModifiedAt = user.LastModifiedAt,
            LastModifiedBy = user.LastModifiedBy,
            
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