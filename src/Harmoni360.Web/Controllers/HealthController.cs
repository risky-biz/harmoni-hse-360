using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Harmoni360.Application.Features.Health.Commands;
using Harmoni360.Application.Features.Health.Queries;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Web.Hubs;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HealthController> _logger;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public HealthController(
        IMediator mediator,
        ILogger<HealthController> logger,
        IHubContext<NotificationHub> notificationHub)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationHub = notificationHub;
    }

    #region Health Records Management

    /// <summary>
    /// Get paginated list of health records with filtering and search
    /// </summary>
    [HttpGet("records")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    [EnableRateLimiting("HealthApi")]
    public async Task<ActionResult<GetHealthRecordsResponse>> GetHealthRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] string? department = null,
        [FromQuery] bool? hasCriticalConditions = null,
        [FromQuery] bool? hasExpiringVaccinations = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            var query = new GetHealthRecordsQuery
            {
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100),
                SearchTerm = searchTerm,
                PersonType = personType,
                Department = department,
                HasCriticalConditions = hasCriticalConditions,
                HasExpiringVaccinations = hasExpiringVaccinations,
                IncludeInactive = includeInactive,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health records");
            return StatusCode(500, new { message = "An error occurred while retrieving health records" });
        }
    }

    /// <summary>
    /// Get health record by ID with full details
    /// </summary>
    [HttpGet("records/{id:int}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    [EnableRateLimiting("HealthApi")]
    public async Task<ActionResult<HealthRecordDetailDto>> GetHealthRecord(
        int id,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new GetHealthRecordByIdQuery
            {
                Id = id,
                IncludeInactive = includeInactive
            };

            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { message = "Health record not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health record {HealthRecordId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the health record" });
        }
    }

    /// <summary>
    /// Create a new health record
    /// </summary>
    [HttpPost("records")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Create)]
    [EnableRateLimiting("HealthApi")]
    public async Task<ActionResult<HealthRecordDto>> CreateHealthRecord([FromBody] CreateHealthRecordCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("HealthRecordCreated", new { HealthRecordId = result.Id });
            
            return CreatedAtAction(nameof(GetHealthRecord), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating health record");
            return StatusCode(500, new { message = "An error occurred while creating the health record" });
        }
    }

    /// <summary>
    /// Update an existing health record
    /// </summary>
    [HttpPut("records/{id:int}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    [EnableRateLimiting("HealthApi")]
    public async Task<ActionResult> UpdateHealthRecord(int id, [FromBody] UpdateHealthRecordCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "Health record ID mismatch" });
            }

            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("HealthRecordUpdated", new { HealthRecordId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating health record {HealthRecordId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the health record" });
        }
    }

    /// <summary>
    /// Deactivate a health record
    /// </summary>
    [HttpDelete("records/{id:int}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Delete)]
    public async Task<ActionResult> DeactivateHealthRecord(int id)
    {
        try
        {
            var command = new DeactivateHealthRecordCommand { Id = id };
            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("HealthRecordDeactivated", new { HealthRecordId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating health record {HealthRecordId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating the health record" });
        }
    }

    #endregion

    #region Medical Conditions Management

    /// <summary>
    /// Add a medical condition to a health record
    /// </summary>
    [HttpPost("records/{healthRecordId:int}/conditions")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult<int>> AddMedicalCondition(
        int healthRecordId,
        [FromBody] AddMedicalConditionCommand command)
    {
        try
        {
            // TODO: Fix ID type mismatch - healthRecordId is int, command.HealthRecordId is Guid
            // command.HealthRecordId = Guid.NewGuid(); // Cannot assign to init-only property

            var result = await _mediator.Send(command);
            
            // Notify via SignalR if critical condition
            if (command.RequiresEmergencyAction)
            {
                await _notificationHub.Clients.All.SendAsync("CriticalMedicalConditionAdded", 
                    new { HealthRecordId = healthRecordId, ConditionId = result });
            }
            
            return CreatedAtAction(nameof(GetHealthRecord), new { id = healthRecordId }, new { Id = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding medical condition to health record {HealthRecordId}", healthRecordId);
            return StatusCode(500, new { message = "An error occurred while adding the medical condition" });
        }
    }

    /// <summary>
    /// Update a medical condition
    /// </summary>
    [HttpPut("conditions/{id:int}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult> UpdateMedicalCondition(int id, [FromBody] UpdateMedicalConditionCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "Medical condition ID mismatch" });
            }

            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medical condition {ConditionId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the medical condition" });
        }
    }

    /// <summary>
    /// Remove a medical condition
    /// </summary>
    [HttpDelete("conditions/{id:int}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Delete)]
    public async Task<ActionResult> RemoveMedicalCondition(int id)
    {
        try
        {
            var command = new RemoveMedicalConditionCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing medical condition {ConditionId}", id);
            return StatusCode(500, new { message = "An error occurred while removing the medical condition" });
        }
    }

    #endregion

    #region Vaccination Management

    /// <summary>
    /// Record a vaccination
    /// </summary>
    [HttpPost("records/{healthRecordId:int}/vaccinations")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult<int>> RecordVaccination(
        int healthRecordId,
        [FromBody] RecordVaccinationCommand command)
    {
        try
        {
            // TODO: Fix ID type mismatch - healthRecordId is int, command.HealthRecordId is Guid
            // command.HealthRecordId = Guid.NewGuid(); // Cannot assign to init-only property

            var result = await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("VaccinationRecorded", 
                new { HealthRecordId = healthRecordId, VaccinationId = result });
            
            return CreatedAtAction(nameof(GetHealthRecord), new { id = healthRecordId }, new { Id = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording vaccination for health record {HealthRecordId}", healthRecordId);
            return StatusCode(500, new { message = "An error occurred while recording the vaccination" });
        }
    }

    /// <summary>
    /// Update a vaccination record
    /// </summary>
    [HttpPut("vaccinations/{id:int}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult> UpdateVaccination(int id, [FromBody] UpdateVaccinationCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "Vaccination ID mismatch" });
            }

            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vaccination {VaccinationId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the vaccination" });
        }
    }

    /// <summary>
    /// Set vaccination exemption
    /// </summary>
    [HttpPost("vaccinations/{id:int}/exemption")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    public async Task<ActionResult> SetVaccinationExemption(int id, [FromBody] SetVaccinationExemptionCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "Vaccination ID mismatch" });
            }

            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting vaccination exemption for {VaccinationId}", id);
            return StatusCode(500, new { message = "An error occurred while setting the vaccination exemption" });
        }
    }

    #endregion

    #region Emergency Contact Management

    /// <summary>
    /// Add an emergency contact to a health record
    /// </summary>
    [HttpPost("records/{healthRecordId:int}/emergency-contacts")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult<int>> AddEmergencyContact(
        int healthRecordId,
        [FromBody] AddEmergencyContactCommand command)
    {
        try
        {
            // TODO: Fix ID type mismatch - healthRecordId is int, command.HealthRecordId is Guid
            // command.HealthRecordId = Guid.NewGuid(); // Cannot assign to init-only property

            var result = await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("EmergencyContactAdded", 
                new { HealthRecordId = healthRecordId, ContactId = result });
            
            return CreatedAtAction(nameof(GetHealthRecord), new { id = healthRecordId }, new { Id = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding emergency contact to health record {HealthRecordId}", healthRecordId);
            return StatusCode(500, new { message = "An error occurred while adding the emergency contact" });
        }
    }

    /// <summary>
    /// Update an emergency contact
    /// </summary>
    [HttpPut("emergency-contacts/{id:guid}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult> UpdateEmergencyContact(Guid id, [FromBody] UpdateEmergencyContactCommand command)
    {
        try
        {
            command.Id = id;

            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("EmergencyContactUpdated", new { ContactId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the emergency contact" });
        }
    }

    /// <summary>
    /// Remove an emergency contact
    /// </summary>
    [HttpDelete("emergency-contacts/{id:guid}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Delete)]
    public async Task<ActionResult> RemoveEmergencyContact(Guid id)
    {
        try
        {
            var command = new RemoveEmergencyContactCommand { Id = id };
            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("EmergencyContactRemoved", new { ContactId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while removing the emergency contact" });
        }
    }

    /// <summary>
    /// Get emergency contacts for a health record
    /// </summary>
    [HttpGet("records/{healthRecordId:int}/emergency-contacts")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    public Task<ActionResult<List<EmergencyContactDto>>> GetEmergencyContacts(
        int healthRecordId,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            // TODO: Implement GetEmergencyContactsByHealthRecordQuery
            var contacts = new List<EmergencyContactDto>();
            return Task.FromResult<ActionResult<List<EmergencyContactDto>>>(Ok(contacts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency contacts for health record {HealthRecordId}", healthRecordId);
            return Task.FromResult<ActionResult<List<EmergencyContactDto>>>(StatusCode(500, new { message = "An error occurred while retrieving emergency contacts" }));
        }
    }

    /// <summary>
    /// Validate emergency contact information
    /// </summary>
    [HttpPost("emergency-contacts/{id:guid}/validate")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult<EmergencyContactValidationResult>> ValidateEmergencyContact(Guid id)
    {
        try
        {
            var command = new ValidateEmergencyContactCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while validating the emergency contact" });
        }
    }

    #endregion

    #region Dashboard and Analytics

    /// <summary>
    /// Get health dashboard with comprehensive metrics
    /// </summary>
    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    [EnableRateLimiting("HealthDashboard")]
    public async Task<ActionResult<HealthDashboardDto>> GetHealthDashboard(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new GetHealthDashboardQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                Department = department,
                PersonType = personType,
                IncludeInactive = includeInactive
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health dashboard");
            return StatusCode(500, new { message = "An error occurred while retrieving the health dashboard" });
        }
    }

    /// <summary>
    /// Get vaccination compliance analysis
    /// </summary>
    [HttpGet("analytics/vaccination-compliance")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    [EnableRateLimiting("HealthAnalytics")]
    public async Task<ActionResult<VaccinationComplianceDto>> GetVaccinationCompliance(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] string? vaccineName = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includeExemptions = true)
    {
        try
        {
            var query = new GetVaccinationComplianceQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                Department = department,
                PersonType = personType,
                VaccineName = vaccineName,
                IncludeInactive = includeInactive,
                IncludeExemptions = includeExemptions
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vaccination compliance data");
            return StatusCode(500, new { message = "An error occurred while retrieving vaccination compliance data" });
        }
    }

    /// <summary>
    /// Get health incident trends and safety analysis
    /// </summary>
    [HttpGet("analytics/incident-trends")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    [EnableRateLimiting("HealthAnalytics")]
    public async Task<ActionResult<HealthIncidentTrendsDto>> GetHealthIncidentTrends(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] HealthIncidentType? incidentType = null,
        [FromQuery] HealthIncidentSeverity? severity = null,
        [FromQuery] TrendPeriod period = TrendPeriod.Monthly,
        [FromQuery] bool includeResolved = true)
    {
        try
        {
            var query = new GetHealthIncidentTrendsQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                Department = department,
                PersonType = personType,
                IncidentType = incidentType,
                Severity = severity,
                Period = period,
                IncludeResolved = includeResolved
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health incident trends");
            return StatusCode(500, new { message = "An error occurred while retrieving health incident trends" });
        }
    }

    /// <summary>
    /// Get emergency contact validation and data quality analysis
    /// </summary>
    [HttpGet("analytics/emergency-contact-validation")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    [EnableRateLimiting("HealthAnalytics")]
    public async Task<ActionResult<EmergencyContactValidationDto>> GetEmergencyContactValidation(
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] ValidationLevel level = ValidationLevel.Standard)
    {
        try
        {
            var query = new GetEmergencyContactValidationQuery
            {
                Department = department,
                PersonType = personType,
                IncludeInactive = includeInactive,
                Level = level
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency contact validation data");
            return StatusCode(500, new { message = "An error occurred while retrieving emergency contact validation data" });
        }
    }

    /// <summary>
    /// Get comprehensive health risk assessment
    /// </summary>
    [HttpGet("analytics/risk-assessment")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    [EnableRateLimiting("HealthAnalytics")]
    public async Task<ActionResult<HealthRiskAssessmentDto>> GetHealthRiskAssessment(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] RiskAssessmentScope scope = RiskAssessmentScope.Standard,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includePredictiveAnalysis = true)
    {
        try
        {
            var query = new GetHealthRiskAssessmentQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                Department = department,
                PersonType = personType,
                Scope = scope,
                IncludeInactive = includeInactive,
                IncludePredictiveAnalysis = includePredictiveAnalysis
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health risk assessment");
            return StatusCode(500, new { message = "An error occurred while retrieving health risk assessment" });
        }
    }

    #endregion

    #region Health Alerts and Notifications

    /// <summary>
    /// Get active health alerts for a person or department
    /// </summary>
    [HttpGet("alerts")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    [EnableRateLimiting("HealthApi")]
    public Task<ActionResult<List<HealthAlert>>> GetHealthAlerts(
        [FromQuery] int? personId = null,
        [FromQuery] string? department = null,
        [FromQuery] AlertSeverity? severity = null,
        [FromQuery] bool activeOnly = true)
    {
        try
        {
            // This would be implemented as a separate query/command
            // For now, return a placeholder response
            var alerts = new List<HealthAlert>();
            return Task.FromResult<ActionResult<List<HealthAlert>>>(Ok(alerts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health alerts");
            return Task.FromResult<ActionResult<List<HealthAlert>>>(StatusCode(500, new { message = "An error occurred while retrieving health alerts" }));
        }
    }

    /// <summary>
    /// Trigger health emergency notification
    /// </summary>
    [HttpPost("emergency-notification")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    [EnableRateLimiting("HealthEmergency")]
    public async Task<ActionResult> TriggerEmergencyNotification([FromBody] EmergencyNotificationRequest request)
    {
        try
        {
            // Notify via SignalR for immediate alert
            await _notificationHub.Clients.All.SendAsync("HealthEmergency", request);
            
            _logger.LogWarning("Health emergency notification triggered for person {PersonId}: {Message}", 
                request.PersonId, request.Message);
            
            return Ok(new { message = "Emergency notification sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending emergency notification");
            return StatusCode(500, new { message = "An error occurred while sending the emergency notification" });
        }
    }

    #endregion
}

// Supporting DTOs for health alerts and notifications
public class HealthAlert
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class EmergencyNotificationRequest
{
    public int PersonId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<int> EmergencyContactIds { get; set; } = new();
}

public enum AlertType
{
    CriticalCondition,
    VaccinationExpiring,
    VaccinationOverdue,
    HealthIncident,
    EmergencyContactMissing,
    DataValidationIssue
}

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

// Emergency Contact validation DTOs
public class EmergencyContactValidationResult
{
    public int ContactId { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public ContactValidationStatus PhoneValidation { get; set; } = new();
    public ContactValidationStatus EmailValidation { get; set; } = new();
    public DateTime LastValidated { get; set; }
}

public class ContactValidationStatus
{
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public DateTime? LastChecked { get; set; }
}