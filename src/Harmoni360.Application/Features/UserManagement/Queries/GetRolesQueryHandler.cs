using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetRolesQueryHandler> _logger;

    public GetRolesQueryHandler(
        IApplicationDbContext context,
        ILogger<GetRolesQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting roles, includeInactive: {IncludeInactive}", request.IncludeInactive);

        var query = _context.Roles
            .Include(r => r.UserRoles)
            .AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        var roles = await query
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            RoleType = r.RoleType,
            IsActive = r.IsActive,
            DisplayOrder = r.DisplayOrder,
            UserCount = r.UserRoles.Count
        }).ToList();
    }
}