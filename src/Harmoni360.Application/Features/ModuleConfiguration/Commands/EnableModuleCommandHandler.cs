using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.ModuleConfiguration.Commands;

public class EnableModuleCommandHandler : IRequestHandler<EnableModuleCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EnableModuleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(EnableModuleCommand request, CancellationToken cancellationToken)
    {
        var moduleConfiguration = await _context.ModuleConfigurations
            .Include(mc => mc.Dependencies)
            .ThenInclude(d => d.DependsOnModule)
            .FirstOrDefaultAsync(mc => mc.ModuleType == request.ModuleType, cancellationToken);

        if (moduleConfiguration == null)
        {
            return false;
        }

        // Check if already enabled
        if (moduleConfiguration.IsEnabled)
        {
            return true;
        }

        // Log the change for audit purposes
        var oldState = moduleConfiguration.IsEnabled;

        // Enable the module using domain method
        moduleConfiguration.Enable();

        // Create audit log entry
        var auditLog = ModuleConfigurationAuditLog.CreateEnabledLog(
            moduleConfiguration.ModuleType,
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