using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class UpdateLicenseConditionCommandHandler : IRequestHandler<UpdateLicenseConditionCommand, LicenseConditionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateLicenseConditionCommandHandler> _logger;

    public UpdateLicenseConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateLicenseConditionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LicenseConditionDto> Handle(UpdateLicenseConditionCommand request, CancellationToken cancellationToken)
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

            // Get the license condition
            var condition = await _context.LicenseConditions
                .FirstOrDefaultAsync(c => c.Id == request.ConditionId && c.LicenseId == request.LicenseId, cancellationToken);

            if (condition == null)
            {
                throw new KeyNotFoundException($"License condition with ID {request.ConditionId} not found for license {request.LicenseId}.");
            }

            // Update condition properties
            condition.ConditionType = request.ConditionType;
            condition.Description = request.Description;
            condition.IsMandatory = request.IsMandatory;
            condition.DueDate = request.DueDate;
            condition.ResponsiblePerson = request.ResponsiblePerson;
            condition.LastModifiedAt = DateTime.UtcNow;
            condition.LastModifiedBy = currentUser.Name;

            // Update status if changed
            if (condition.Status != request.Status)
            {
                condition.UpdateStatus(request.Status, currentUser.Name, request.Notes);
            }
            else if (!string.IsNullOrEmpty(request.Notes))
            {
                condition.Notes = request.Notes;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License condition updated successfully. LicenseId: {LicenseId}, ConditionId: {ConditionId}", 
                request.LicenseId, request.ConditionId);

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
            _logger.LogError(ex, "Error updating condition {ConditionId} for license {LicenseId} by user {UserId}", 
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