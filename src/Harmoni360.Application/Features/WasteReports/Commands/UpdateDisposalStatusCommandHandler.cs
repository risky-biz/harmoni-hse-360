using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public class UpdateDisposalStatusCommandHandler : IRequestHandler<UpdateDisposalStatusCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateDisposalStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDisposalStatusCommand request, CancellationToken cancellationToken)
    {
        var report = await _context.WasteReports.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (report is null)
        {
            return;
        }
        report.UpdateDisposalStatus(request.Status);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
