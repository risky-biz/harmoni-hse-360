using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class DeleteWorkPermitSettingCommandHandler : IRequestHandler<DeleteWorkPermitSettingCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkPermitSettingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteWorkPermitSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _context.WorkPermitSettings
            .Include(wps => wps.SafetyVideos)
            .FirstOrDefaultAsync(wps => wps.Id == request.Id, cancellationToken);

        if (setting == null)
        {
            throw new KeyNotFoundException($"Work Permit Setting with ID {request.Id} was not found.");
        }

        // Business rule: Cannot delete the active setting if it's the only one
        if (setting.IsActive)
        {
            var otherSettingsCount = await _context.WorkPermitSettings
                .CountAsync(wps => wps.Id != request.Id, cancellationToken);

            if (otherSettingsCount == 0)
            {
                throw new InvalidOperationException("Cannot delete the last Work Permit Setting. At least one setting must exist.");
            }
        }

        _context.WorkPermitSettings.Remove(setting);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}