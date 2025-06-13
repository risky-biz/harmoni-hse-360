using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, UserListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IApplicationDbContext context,
        ILogger<GetUsersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserListDto> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting users with page {Page}, pageSize {PageSize}, searchTerm: {SearchTerm}",
            request.Page, request.PageSize, request.SearchTerm);

        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u => 
                u.Name.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                u.EmployeeId.ToLower().Contains(searchTerm) ||
                u.Department.ToLower().Contains(searchTerm) ||
                u.Position.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(u => u.Department.ToLower().Contains(request.Department.ToLower()));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        if (request.RoleId.HasValue)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == request.RoleId.Value));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "employeeid" => request.SortDescending ? query.OrderByDescending(u => u.EmployeeId) : query.OrderBy(u => u.EmployeeId),
            "department" => request.SortDescending ? query.OrderByDescending(u => u.Department) : query.OrderBy(u => u.Department),
            "position" => request.SortDescending ? query.OrderByDescending(u => u.Position) : query.OrderBy(u => u.Position),
            "createdat" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            "isactive" => request.SortDescending ? query.OrderByDescending(u => u.IsActive) : query.OrderBy(u => u.IsActive),
            _ => request.SortDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            EmployeeId = u.EmployeeId,
            Department = u.Department,
            Position = u.Position,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastModifiedAt = u.LastModifiedAt,
            Roles = u.UserRoles.Select(ur => new UserRoleDto
            {
                RoleId = ur.Role.Id,
                RoleName = ur.Role.Name,
                RoleType = ur.Role.RoleType,
                Description = ur.Role.Description,
                AssignedAt = ur.CreatedAt
            }).ToList()
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new UserListDto
        {
            Users = userDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }
}