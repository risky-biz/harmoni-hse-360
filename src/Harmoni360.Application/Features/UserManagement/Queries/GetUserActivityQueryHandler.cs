using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public class GetUserActivityQueryHandler : IRequestHandler<GetUserActivityQuery, List<UserActivityLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetUserActivityQueryHandler> _logger;

    public GetUserActivityQueryHandler(
        IApplicationDbContext context,
        ILogger<GetUserActivityQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserActivityLogDto>> Handle(GetUserActivityQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user activity for UserId: {UserId}", request.UserId);

        var query = _context.UserActivityLogs
            .Include(log => log.User)
            .Where(log => log.UserId == request.UserId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.ActivityType))
        {
            query = query.Where(log => log.ActivityType == request.ActivityType);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= request.ToDate.Value);
        }

        // Apply pagination
        var activities = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(log => new UserActivityLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.User.Name,
                UserEmail = log.User.Email,
                ActivityType = log.ActivityType,
                Description = log.Description,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} activity logs for UserId: {UserId}", activities.Count, request.UserId);

        return activities ?? new List<UserActivityLogDto>();
    }
}