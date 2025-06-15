using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.Configuration.Commands;

public class DeleteDepartmentCommand : IRequest<Unit>
{
    public int Id { get; set; }
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null)
        {
            throw new ArgumentException($"Department with ID {request.Id} not found");
        }

        // Check if department is being used in incidents
        var hasIncidents = await _context.Incidents
            .AnyAsync(i => i.DepartmentId == request.Id, cancellationToken);

        if (hasIncidents)
        {
            throw new InvalidOperationException("Cannot delete department that is referenced by existing incidents");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}