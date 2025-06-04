using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using MediatR;

namespace HarmoniHSE360.Application.Features.CorrectiveActions.Commands;

public record CreateCorrectiveActionCommand : IRequest<int>
{
    public int IncidentId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string AssignedToDepartment { get; init; } = string.Empty;
    public int? AssignedToId { get; init; }
    public DateTime DueDate { get; init; }
    public ActionPriority Priority { get; init; }
}

public class CreateCorrectiveActionCommandHandler : IRequestHandler<CreateCorrectiveActionCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIncidentAuditService _auditService;

    public CreateCorrectiveActionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IIncidentAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<int> Handle(CreateCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var incident = await _context.Incidents
            .FindAsync(new object[] { request.IncidentId }, cancellationToken);

        if (incident == null)
        {
            throw new InvalidOperationException($"Incident with ID {request.IncidentId} not found");
        }

        var currentUser = _currentUserService.Email ?? "System";

        var correctiveAction = CorrectiveAction.Create(
            request.IncidentId,
            request.Description,
            request.AssignedToDepartment,
            request.DueDate,
            request.Priority,
            currentUser,
            request.AssignedToId);

        _context.CorrectiveActions.Add(correctiveAction);
        
        // Log audit trail before saving
        await _auditService.LogCorrectiveActionAddedAsync(request.IncidentId, request.Description);
        
        await _context.SaveChangesAsync(cancellationToken);

        return correctiveAction.Id;
    }
}