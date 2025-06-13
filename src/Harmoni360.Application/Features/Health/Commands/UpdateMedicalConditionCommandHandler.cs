using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Commands;

public class UpdateMedicalConditionCommandHandler : IRequestHandler<UpdateMedicalConditionCommand, MedicalConditionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateMedicalConditionCommandHandler> _logger;
    private readonly ICacheService _cache;

    public UpdateMedicalConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<UpdateMedicalConditionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<MedicalConditionDto> Handle(UpdateMedicalConditionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating medical condition ID: {ConditionId} by user {UserEmail}",
            request.Id, _currentUserService.Email);

        var medicalCondition = await _context.MedicalConditions
            .FirstOrDefaultAsync(mc => mc.Id == request.Id, cancellationToken);

        if (medicalCondition == null)
        {
            throw new ArgumentException($"Medical condition with ID {request.Id} not found", nameof(request.Id));
        }

        if (!medicalCondition.IsActive)
        {
            throw new InvalidOperationException("Cannot update an inactive medical condition");
        }

        // Check for duplicate names (excluding current condition)
        var existingCondition = await _context.MedicalConditions
            .FirstOrDefaultAsync(mc => mc.HealthRecordId == medicalCondition.HealthRecordId 
                && mc.Name.ToLower() == request.Name.ToLower() 
                && mc.Id != request.Id
                && mc.IsActive, cancellationToken);

        if (existingCondition != null)
        {
            throw new InvalidOperationException($"A medical condition with the name '{request.Name}' already exists for this health record");
        }

        // Log severity changes for critical conditions
        var previousSeverity = medicalCondition.Severity;
        var previousEmergencyAction = medicalCondition.RequiresEmergencyAction;

        // Update the medical condition
        medicalCondition.Update(
            request.Type,
            request.Name,
            request.Description,
            request.Severity,
            request.TreatmentPlan,
            request.DiagnosedDate,
            request.RequiresEmergencyAction,
            request.EmergencyInstructions
        );

        await _context.SaveChangesAsync(cancellationToken);

        // Log important changes
        if (previousSeverity != request.Severity)
        {
            _logger.LogInformation("Medical condition severity changed from {PreviousSeverity} to {NewSeverity} for condition {ConditionId}",
                previousSeverity, request.Severity, request.Id);
        }

        if (previousEmergencyAction != request.RequiresEmergencyAction)
        {
            _logger.LogWarning("Emergency action requirement changed from {PreviousValue} to {NewValue} for condition {ConditionId}",
                previousEmergencyAction, request.RequiresEmergencyAction, request.Id);
        }

        if (medicalCondition.RequiresEmergencyAction)
        {
            _logger.LogWarning("CRITICAL MEDICAL CONDITION UPDATED: {ConditionName} for health record {HealthRecordId}. Emergency instructions: {EmergencyInstructions}",
                medicalCondition.Name, medicalCondition.HealthRecordId, medicalCondition.EmergencyInstructions);
        }

        _logger.LogInformation("Medical condition updated successfully: {ConditionId}", medicalCondition.Id);

        // Invalidate health-related caches
        await InvalidateHealthCaches(medicalCondition.HealthRecordId);

        // Return DTO
        return new MedicalConditionDto
        {
            Id = medicalCondition.Id,
            HealthRecordId = medicalCondition.HealthRecordId,
            Type = medicalCondition.Type.ToString(),
            Name = medicalCondition.Name,
            Description = medicalCondition.Description,
            Severity = medicalCondition.Severity.ToString(),
            TreatmentPlan = medicalCondition.TreatmentPlan,
            DiagnosedDate = medicalCondition.DiagnosedDate,
            RequiresEmergencyAction = medicalCondition.RequiresEmergencyAction,
            EmergencyInstructions = medicalCondition.EmergencyInstructions,
            IsActive = medicalCondition.IsActive,
            CreatedAt = medicalCondition.CreatedAt,
            CreatedBy = medicalCondition.CreatedBy,
            LastModifiedAt = medicalCondition.LastModifiedAt,
            LastModifiedBy = medicalCondition.LastModifiedBy
        };
    }

    private async Task InvalidateHealthCaches(int healthRecordId)
    {
        await _cache.RemoveByTagAsync("health");
        await _cache.RemoveByTagAsync("health-records");
        await _cache.RemoveAsync($"health-record-{healthRecordId}");
        await _cache.RemoveByTagAsync("medical-conditions");
        _logger.LogInformation("Cache invalidated for medical condition update in health record: {HealthRecordId}", healthRecordId);
    }
}