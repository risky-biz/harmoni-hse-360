using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class DisableModuleCommandHandler : IRequestHandler<DisableModuleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DisableModuleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DisableModuleCommand request, CancellationToken cancellationToken)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.SubModules)
            .Include(mc => mc.DependentModules)
            .ThenInclude(d => d.Module)
            .FirstOrDefaultAsync(mc => mc.ModuleType == request.ModuleType, cancellationToken);

        if (moduleConfiguration == null)
        {
            return false;
        }

        // Check if already disabled
        if (!moduleConfiguration.IsEnabled)
        {
            return true;
        }

        // Check if module can be disabled (unless forced)
        if (!request.ForceDisable && !moduleConfiguration.ValidateDisable())
        {
            return false;
        }

        // Log the change for audit purposes
        var oldState = moduleConfiguration.IsEnabled;

        try
        {
            // Disable the module using domain method
            if (request.ForceDisable)
            {
                // For force disable, manually set the state and handle dependents
                moduleConfiguration.IsEnabled = false;
                
                // Force disable all sub-modules
                foreach (var subModule in moduleConfiguration.SubModules)
                {
                    subModule.IsEnabled = false;
                }
            }
            else
            {
                // Use domain validation
                moduleConfiguration.Disable();
            }

            // Create audit log entry
            var auditLog = ModuleConfigurationAuditLog.CreateDisabledLog(
                moduleConfiguration.ModuleType,
                _currentUserService.UserId
            );
            
            if (!string.IsNullOrEmpty(request.Context))
            {
                auditLog.Context = request.Context;
            }
            else if (request.ForceDisable)
            {
                auditLog.Context = "Module force disabled via admin interface";
            }

            _context.ModuleConfigurationAuditLogs.Add(auditLog);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (InvalidOperationException)
        {
            // Module cannot be disabled due to business rules
            return false;
        }
    }
}