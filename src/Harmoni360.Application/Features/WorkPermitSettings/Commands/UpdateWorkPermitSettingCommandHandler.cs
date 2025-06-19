using Harmoni360.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.WorkPermitSettings.Commands;

public class UpdateWorkPermitSettingCommandHandler : IRequestHandler<UpdateWorkPermitSettingCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateWorkPermitSettingCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateWorkPermitSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _context.WorkPermitSettings
            .FirstOrDefaultAsync(wps => wps.Id == request.Id, cancellationToken);

        if (setting == null)
        {
            throw new KeyNotFoundException($"Work Permit Setting with ID {request.Id} was not found.");
        }

        // If this setting is being set as active, deactivate any existing active setting
        if (request.IsActive && !setting.IsActive)
        {
            var existingActiveSetting = await _context.WorkPermitSettings
                .Where(wps => wps.IsActive && wps.Id != request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingActiveSetting != null)
            {
                existingActiveSetting.Deactivate(_currentUserService.Email);
            }
        }

        // Update the setting using domain methods
        setting.UpdateFormConfiguration(
            requireSafetyInduction: request.RequireSafetyInduction,
            enableFormValidation: request.EnableFormValidation,
            allowAttachments: request.AllowAttachments,
            maxAttachmentSize: request.MaxAttachmentSizeMB,
            formInstructions: request.FormInstructions,
            modifiedBy: _currentUserService.Email
        );

        if (request.IsActive && !setting.IsActive)
        {
            setting.Activate(_currentUserService.Email);
        }
        else if (!request.IsActive && setting.IsActive)
        {
            setting.Deactivate(_currentUserService.Email);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}