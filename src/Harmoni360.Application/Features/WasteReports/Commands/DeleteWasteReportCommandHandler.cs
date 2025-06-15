using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class DeleteWasteReportCommandHandler : IRequestHandler<DeleteWasteReportCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteWasteReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteWasteReportCommand request, CancellationToken cancellationToken)
    {
        var wasteReport = await _context.WasteReports
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (wasteReport == null)
        {
            throw new InvalidOperationException($"Waste report with ID {request.Id} not found");
        }

        _context.WasteReports.Remove(wasteReport);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}