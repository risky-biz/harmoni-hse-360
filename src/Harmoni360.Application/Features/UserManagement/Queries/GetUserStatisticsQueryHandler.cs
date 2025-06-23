using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, UserStatisticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetUserStatisticsQueryHandler> _logger;

    public GetUserStatisticsQueryHandler(
        IApplicationDbContext context,
        ILogger<GetUserStatisticsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserStatisticsDto> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user statistics");

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var users = await _context.Users.ToListAsync(cancellationToken);

        var statistics = new UserStatisticsDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.Status == UserStatus.Active),
            InactiveUsers = users.Count(u => u.Status == UserStatus.Inactive),
            SuspendedUsers = users.Count(u => u.Status == UserStatus.Suspended),
            LockedAccounts = users.Count(u => u.AccountLockedUntil.HasValue && u.AccountLockedUntil > now),
            UsersWithMFA = users.Count(u => u.RequiresMFA),
            NewUsersThisMonth = users.Count(u => u.CreatedAt >= startOfMonth),
            UsersByDepartment = users
                .GroupBy(u => u.Department)
                .ToDictionary(g => g.Key, g => g.Count()),
            UsersByLocation = users
                .Where(u => !string.IsNullOrEmpty(u.WorkLocation))
                .GroupBy(u => u.WorkLocation!)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // Get role statistics
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .ToListAsync(cancellationToken);

        statistics.UsersByRole = userRoles
            .GroupBy(ur => ur.Role.Name)
            .ToDictionary(g => g.Key, g => g.Count());

        _logger.LogInformation("User statistics retrieved successfully");

        return statistics;
    }
}