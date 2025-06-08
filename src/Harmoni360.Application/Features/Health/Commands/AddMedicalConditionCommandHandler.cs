using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.Commands;

public class AddMedicalConditionCommandHandler : IRequestHandler<AddMedicalConditionCommand, MedicalConditionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddMedicalConditionCommandHandler> _logger;
    private readonly ICacheService _cache;

    public AddMedicalConditionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AddMedicalConditionCommandHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<MedicalConditionDto> Handle(AddMedicalConditionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding medical condition to health record ID: {HealthRecordId} by user {UserEmail}",
            request.HealthRecordId, _currentUserService.Email);

        // Verify the health record exists and is active
        var healthRecord = await _context.HealthRecords
            .FirstOrDefaultAsync(hr => hr.Id == request.HealthRecordId && hr.IsActive, cancellationToken);

        if (healthRecord == null)
        {
            throw new ArgumentException($"Active health record with ID {request.HealthRecordId} not found", nameof(request.HealthRecordId));
        }

        // Check for duplicate medical conditions
        var existingCondition = await _context.MedicalConditions
            .FirstOrDefaultAsync(mc => mc.HealthRecordId == request.HealthRecordId 
                && mc.Name.ToLower() == request.Name.ToLower() 
                && mc.IsActive, cancellationToken);

        if (existingCondition != null)
        {
            throw new InvalidOperationException($"A medical condition with the name '{request.Name}' already exists for this health record");
        }

        // Create medical condition
        var medicalCondition = MedicalCondition.Create(
            request.HealthRecordId,
            request.Type,
            request.Name,
            request.Description,
            request.Severity,
            request.TreatmentPlan,
            request.DiagnosedDate,
            request.RequiresEmergencyAction,
            request.EmergencyInstructions
        );

        _context.MedicalConditions.Add(medicalCondition);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Medical condition added successfully with ID: {ConditionId}. Emergency action required: {RequiresEmergencyAction}", 
            medicalCondition.Id, request.RequiresEmergencyAction);

        // Log critical condition alert
        if (medicalCondition.RequiresEmergencyAction)
        {
            _logger.LogWarning("CRITICAL MEDICAL CONDITION ADDED: {ConditionName} for health record {HealthRecordId}. Emergency instructions: {EmergencyInstructions}",
                medicalCondition.Name, request.HealthRecordId, medicalCondition.EmergencyInstructions);
        }

        // Invalidate health-related caches
        await InvalidateHealthCaches(request.HealthRecordId);

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
        _logger.LogInformation("Cache invalidated for medical condition addition to health record: {HealthRecordId}", healthRecordId);
    }
}