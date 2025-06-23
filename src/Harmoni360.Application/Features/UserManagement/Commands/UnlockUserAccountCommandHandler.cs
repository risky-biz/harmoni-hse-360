using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class UnlockUserAccountCommandHandler : IRequestHandler<UnlockUserAccountCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UnlockUserAccountCommandHandler> _logger;

    public UnlockUserAccountCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UnlockUserAccountCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UnlockUserAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unlocking user account: UserId={UserId} by {CurrentUser}",
            request.UserId, _currentUserService.Email);

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        user.UnlockAccount();
        await _context.SaveChangesAsync(cancellationToken);

        // Log the activity
        var activityLog = UserActivityLog.Create(
            request.UserId,
            "AccountUnlock",
            $"User account unlocked" + (string.IsNullOrWhiteSpace(request.Reason) ? "" : $". Reason: {request.Reason}"),
            "User",
            request.UserId
        );
        _context.UserActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User account unlocked successfully for UserId: {UserId}", request.UserId);

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