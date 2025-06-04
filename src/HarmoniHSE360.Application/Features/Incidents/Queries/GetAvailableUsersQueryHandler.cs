using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.Incidents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetAvailableUsersQueryHandler : IRequestHandler<GetAvailableUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetAvailableUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Where(u => u.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => 
                u.Name.Contains(request.SearchTerm) || 
                u.Email.Contains(request.SearchTerm));
        }

        var users = await query
            .OrderBy(u => u.Name)
            .Select(u => new 
            {
                u.Id,
                u.Name,
                u.Email
            })
            .Take(50) // Limit results
            .ToListAsync(cancellationToken);

        return users.Select(u => 
        {
            var nameParts = u.Name.Split(' ');
            return new UserDto
            {
                Id = u.Id,
                FirstName = nameParts.FirstOrDefault() ?? "",
                LastName = string.Join(" ", nameParts.Skip(1)),
                Email = u.Email,
                FullName = u.Name
            };
        }).ToList();
    }
}