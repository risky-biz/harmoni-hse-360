using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities.Inspections;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Inspections.Commands;

public class CompleteInspectionCommandHandler : IRequestHandler<CompleteInspectionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CompleteInspectionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(CompleteInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _context.Inspections
            .Include(i => i.Findings)
            .FirstOrDefaultAsync(i => i.Id == request.InspectionId, cancellationToken);

        if (inspection == null)
            throw new ArgumentException($"Inspection with ID {request.InspectionId} not found");

        // Add findings
        foreach (var findingCommand in request.Findings)
        {
            var finding = InspectionFinding.Create(
                inspection.Id,
                findingCommand.Description,
                findingCommand.Type,
                findingCommand.Severity,
                findingCommand.Location,
                findingCommand.Equipment);

            // Set additional finding details
            if (!string.IsNullOrEmpty(findingCommand.RootCause))
                finding.SetRootCause(findingCommand.RootCause);

            if (!string.IsNullOrEmpty(findingCommand.ImmediateAction))
                finding.SetImmediateAction(findingCommand.ImmediateAction);

            if (!string.IsNullOrEmpty(findingCommand.CorrectiveAction))
                finding.SetCorrectiveAction(
                    findingCommand.CorrectiveAction,
                    findingCommand.DueDate,
                    findingCommand.ResponsiblePersonId);

            if (!string.IsNullOrEmpty(findingCommand.Regulation))
                finding.SetRegulation(findingCommand.Regulation);

            inspection.AddFinding(finding);
        }

        // Complete inspection
        inspection.CompleteInspection(request.Summary, request.Recommendations);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}