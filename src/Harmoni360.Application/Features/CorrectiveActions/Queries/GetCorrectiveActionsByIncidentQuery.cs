using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Incidents.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.CorrectiveActions.Queries;

public record GetCorrectiveActionsByIncidentQuery(int IncidentId) : IRequest<List<CorrectiveActionDto>>;

public class GetCorrectiveActionsByIncidentQueryHandler : IRequestHandler<GetCorrectiveActionsByIncidentQuery, List<CorrectiveActionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCorrectiveActionsByIncidentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CorrectiveActionDto>> Handle(GetCorrectiveActionsByIncidentQuery request, CancellationToken cancellationToken)
    {
        var correctiveActions = await _context.CorrectiveActions
            .Where(ca => ca.IncidentId == request.IncidentId)
            .Include(ca => ca.AssignedTo)
            .OrderBy(ca => ca.DueDate)
            .Select(ca => new CorrectiveActionDto
            {
                Id = ca.Id,
                Description = ca.Description,
                AssignedTo = ca.AssignedTo != null ? new UserDto
                {
                    Id = ca.AssignedTo.Id,
                    FirstName = "", // User entity doesn't have FirstName
                    LastName = "", // User entity doesn't have LastName
                    Email = ca.AssignedTo.Email,
                    FullName = ca.AssignedTo.Name,
                    Department = ca.AssignedTo.Department,
                    Position = ca.AssignedTo.Position
                } : new UserDto { FullName = ca.AssignedToDepartment },
                DueDate = ca.DueDate,
                CompletedDate = ca.CompletedDate,
                Status = ca.Status.ToString(),
                Priority = ca.Priority.ToString(),
                CompletionNotes = ca.CompletionNotes,
                AssignedToDepartment = ca.AssignedToDepartment
            })
            .ToListAsync(cancellationToken);

        return correctiveActions ?? new List<CorrectiveActionDto>();
    }
}