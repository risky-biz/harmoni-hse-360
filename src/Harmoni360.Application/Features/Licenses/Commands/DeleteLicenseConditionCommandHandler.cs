using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class DeleteLicenseConditionCommandHandler : IRequestHandler<DeleteLicenseConditionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteLicenseConditionCommandHandler> _logger;

    public DeleteLicenseConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DeleteLicenseConditionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteLicenseConditionCommand request, CancellationToken cancellationToken)
    {
        var license = await _context.Licenses
            .Include(l => l.LicenseConditions)
            .FirstOrDefaultAsync(l => l.Id == request.LicenseId, cancellationToken);

        if (license == null)
        {
            throw new KeyNotFoundException($"License with ID {request.LicenseId} not found.");
        }

        var condition = license.LicenseConditions.FirstOrDefault(c => c.Id == request.ConditionId);
        if (condition == null)
        {
            throw new KeyNotFoundException($"License condition with ID {request.ConditionId} not found.");
        }

        // Check if license can be edited
        if (license.Status != LicenseStatus.Draft && license.Status != LicenseStatus.Rejected)
        {
            throw new InvalidOperationException($"Cannot modify conditions of license in {license.Status} status. Only Draft or Rejected licenses can be modified.");
        }

        // Store condition details for audit before removal
        var conditionDetails = $"Type: {condition.ConditionType}, Description: {condition.Description}";
        
        // Remove the condition using domain method
        license.RemoveCondition(request.ConditionId);

        // Add audit log
        var currentUser = _currentUserService.Email ?? "System";
        license.LogAuditAction(
            LicenseAuditAction.ConditionUpdated,
            $"Removed condition: {conditionDetails}",
            currentUser);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "License condition {ConditionId} removed from license {LicenseId} by {User}",
            request.ConditionId,
            request.LicenseId,
            currentUser);

        return Unit.Value;
    }
}