using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IApplicationDbContext context,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user by ID: {UserId}", request.UserId);

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", request.UserId);
            return null;
        }

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
            
            Roles = user.UserRoles.Select(ur => new UserRoleDto
            {
                RoleId = ur.Role.Id,
                RoleName = ur.Role.Name,
                RoleType = ur.Role.RoleType,
                Description = ur.Role.Description,
                AssignedAt = ur.CreatedAt
            }).ToList()
        };
    }
}