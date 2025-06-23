using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Authorization;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHashService passwordHashService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHashService = passwordHashService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user: {Email} by {CurrentUser}",
            request.Email, _currentUserService.Email);

        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }

        // Check if employee ID already exists
        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            var existingEmployeeId = await _context.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId, cancellationToken);
            
            if (existingEmployeeId != null)
            {
                throw new InvalidOperationException($"User with employee ID {request.EmployeeId} already exists");
            }
        }

        // Hash the password
        var passwordHash = _passwordHashService.HashPassword(request.Password);

        // Create the user
        var user = User.Create(
            request.Email,
            passwordHash,
            request.Name,
            request.EmployeeId,
            request.Department,
            request.Position,
            request.PhoneNumber,
            request.WorkLocation,
            request.CostCenter,
            request.HireDate
        );
        
        // Set additional HSSE fields
        if (!string.IsNullOrWhiteSpace(request.EmergencyContactName) || !string.IsNullOrWhiteSpace(request.EmergencyContactPhone))
        {
            user.UpdateEmergencyContact(request.EmergencyContactName, request.EmergencyContactPhone);
        }
        
        if (!string.IsNullOrWhiteSpace(request.SupervisorEmployeeId))
        {
            user.UpdateSupervisor(request.SupervisorEmployeeId);
        }
        
        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage) || !string.IsNullOrWhiteSpace(request.TimeZone))
        {
            user.UpdatePreferences(request.PreferredLanguage, request.TimeZone);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

        // Get current user's roles to validate role assignments
        var currentUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

        if (currentUser == null)
        {
            throw new InvalidOperationException("Current user not found");
        }

        // Assign roles if provided
        if (request.RoleIds.Any())
        {
            var roles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id) && r.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var role in roles)
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

            await _context.SaveChangesAsync(cancellationToken);
        }

        // Return DTO with roles
        var userWithRoles = await _context.Users
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
            PhoneNumber = user.PhoneNumber,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            SupervisorEmployeeId = user.SupervisorEmployeeId,
            HireDate = user.HireDate,
            WorkLocation = user.WorkLocation,
            CostCenter = user.CostCenter,
            RequiresMFA = user.RequiresMFA,
            LastPasswordChange = user.LastPasswordChange,
            LastLoginAt = user.LastLoginAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            AccountLockedUntil = user.AccountLockedUntil,
            PreferredLanguage = user.PreferredLanguage,
            TimeZone = user.TimeZone,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            LastModifiedAt = user.LastModifiedAt,
            LastModifiedBy = user.LastModifiedBy,
            Roles = userWithRoles?.UserRoles.Select(ur => new UserRoleDto
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