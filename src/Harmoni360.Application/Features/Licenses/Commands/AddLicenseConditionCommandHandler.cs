using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class AddLicenseConditionCommandHandler : IRequestHandler<AddLicenseConditionCommand, LicenseConditionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddLicenseConditionCommandHandler> _logger;

    public AddLicenseConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AddLicenseConditionCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<LicenseConditionDto> Handle(AddLicenseConditionCommand request, CancellationToken cancellationToken)
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

            // Verify license exists
            var license = await _context.Licenses
                .FirstOrDefaultAsync(l => l.Id == request.LicenseId, cancellationToken);

            if (license == null)
            {
                throw new KeyNotFoundException($"License with ID {request.LicenseId} not found.");
            }

            // Create license condition
            var condition = LicenseCondition.Create(
                licenseId: request.LicenseId,
                conditionType: request.ConditionType,
                description: request.Description,
                isMandatory: request.IsMandatory,
                dueDate: request.DueDate,
                responsiblePerson: request.ResponsiblePerson
            );

            condition.CreatedBy = currentUser.Name;

            _context.LicenseConditions.Add(condition);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("License condition added successfully. LicenseId: {LicenseId}, ConditionId: {ConditionId}, Type: {ConditionType}", 
                request.LicenseId, condition.Id, request.ConditionType);

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
            _logger.LogError(ex, "Error adding condition to license {LicenseId} by user {UserId}", request.LicenseId, _currentUserService.UserId);
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