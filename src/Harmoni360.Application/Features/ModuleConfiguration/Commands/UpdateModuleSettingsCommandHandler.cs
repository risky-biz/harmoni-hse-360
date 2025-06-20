using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class UpdateModuleSettingsCommandHandler : IRequestHandler<UpdateModuleSettingsCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateModuleSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateModuleSettingsCommand request, CancellationToken cancellationToken)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .FirstOrDefaultAsync(mc => mc.ModuleType == request.ModuleType, cancellationToken);

        if (moduleConfiguration == null)
        {
            return false;
        }

        // Store old settings for audit
        var oldSettings = moduleConfiguration.Settings;

        // Update settings
        moduleConfiguration.Settings = request.Settings;

        // Create audit log entry for settings change
        var auditLog = ModuleConfigurationAuditLog.CreateSettingsUpdatedLog(
            moduleConfiguration.ModuleType,
            oldSettings,
            request.Settings,
            _currentUserService.UserId
        );
        
        if (!string.IsNullOrEmpty(request.Context))
        {
            auditLog.Context = request.Context;
        }

        _context.ModuleConfigurationAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}