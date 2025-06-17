using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class CompleteLicenseConditionCommandHandler : IRequestHandler<CompleteLicenseConditionCommand, LicenseConditionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CompleteLicenseConditionCommandHandler> _logger;

    public CompleteLicenseConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CompleteLicenseConditionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LicenseConditionDto> Handle(CompleteLicenseConditionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Get the license with conditions for audit trail
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
                throw new KeyNotFoundException($"License condition with ID {request.ConditionId} not found for license {request.LicenseId}.");
            }

            // Store condition details for audit
            var conditionDetails = $"Type: {condition.ConditionType}, Description: {condition.Description}";
            
            // Complete the condition
            condition.Complete(currentUser.Name, request.ComplianceEvidence, request.Notes);
            condition.LastModifiedAt = DateTime.UtcNow;
            condition.LastModifiedBy = currentUser.Name;

            // Add audit log
            license.LogAuditAction(
                LicenseAuditAction.ConditionUpdated,
                $"Completed condition: {conditionDetails}. Evidence: {request.ComplianceEvidence ?? "None provided"}",
                currentUser.Name);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License condition completed successfully. LicenseId: {LicenseId}, ConditionId: {ConditionId}, CompletedBy: {CompletedBy}", 
                request.LicenseId, request.ConditionId, currentUser.Name);

            // Return DTO
            return new LicenseConditionDto
            {
                Id = condition.Id,
                ConditionType = condition.ConditionType,
                Description = condition.Description,
                IsMandatory = condition.IsMandatory,
                DueDate = condition.DueDate,
                Status = condition.Status.ToString(),
                StatusDisplay = GetConditionStatusDisplay(condition.Status),
                ComplianceEvidence = condition.ComplianceEvidence,
                ComplianceDate = condition.ComplianceDate,
                VerifiedBy = condition.VerifiedBy,
                ResponsiblePerson = condition.ResponsiblePerson,
                Notes = condition.Notes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing condition {ConditionId} for license {LicenseId} by user {UserId}", 
                request.ConditionId, request.LicenseId, _currentUserService.UserId);
            throw;
        }
    }

    private static string GetConditionStatusDisplay(Domain.Enums.LicenseConditionStatus status) => status switch
    {
        Domain.Enums.LicenseConditionStatus.Pending => "Pending",
        Domain.Enums.LicenseConditionStatus.InProgress => "In Progress",
        Domain.Enums.LicenseConditionStatus.Completed => "Completed",
        Domain.Enums.LicenseConditionStatus.Overdue => "Overdue",
        Domain.Enums.LicenseConditionStatus.Waived => "Waived",
        _ => status.ToString()
    };
}