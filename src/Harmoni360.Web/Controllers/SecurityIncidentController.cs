using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.SecurityIncidents.Commands;
using Harmoni360.Application.Features.SecurityIncidents.DTOs;
using Harmoni360.Application.Features.SecurityIncidents.Queries;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityIncidentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISecurityIncidentService _securityIncidentService;
    private readonly ILogger<SecurityIncidentController> _logger;

    public SecurityIncidentController(
        IMediator mediator,
        ISecurityIncidentService securityIncidentService,
        ILogger<SecurityIncidentController> logger)
    {
        _mediator = mediator;
        _securityIncidentService = securityIncidentService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of security incidents
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<PagedList<SecurityIncidentListDto>>> GetSecurityIncidents(
        [FromQuery] GetSecurityIncidentsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security incidents");
            return StatusCode(500, "An error occurred while retrieving security incidents");
        }
    }

    /// <summary>
    /// Get security incident by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<SecurityIncidentDetailDto>> GetSecurityIncident(int id)
    {
        try
        {
            var result = await _securityIncidentService.GetIncidentDetailAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while retrieving the security incident");
        }
    }

    /// <summary>
    /// Create new security incident
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Create)]
    public async Task<ActionResult<SecurityIncidentDto>> CreateSecurityIncident(
        [FromBody] CreateSecurityIncidentCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSecurityIncident), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security incident");
            return StatusCode(500, "An error occurred while creating the security incident");
        }
    }

    /// <summary>
    /// Update security incident
    /// </summary>
    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Update)]
    public async Task<ActionResult<SecurityIncidentDto>> UpdateSecurityIncident(
        int id, 
        [FromBody] UpdateSecurityIncidentCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while updating the security incident");
        }
    }

    /// <summary>
    /// Delete security incident
    /// </summary>
    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Delete)]
    public async Task<ActionResult> DeleteSecurityIncident(int id)
    {
        try
        {
            // Security incidents should typically not be deleted, but marked as cancelled
            var command = new UpdateSecurityIncidentCommand 
            { 
                Id = id,
                Status = SecurityIncidentStatus.Closed 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while deleting the security incident");
        }
    }

    /// <summary>
    /// Get security dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<SecurityDashboardDto>> GetSecurityDashboard(
        [FromQuery] GetSecurityDashboardQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security dashboard");
            return StatusCode(500, "An error occurred while retrieving the security dashboard");
        }
    }

    /// <summary>
    /// Create threat assessment for incident
    /// </summary>
    [HttpPost("{id}/threat-assessment")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Update)]
    public async Task<ActionResult<ThreatAssessmentDto>> CreateThreatAssessment(
        int id,
        [FromBody] CreateThreatAssessmentCommand command)
    {
        try
        {
            if (id != command.SecurityIncidentId)
            {
                return BadRequest("Incident ID in URL does not match ID in request body");
            }

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetThreatAssessment), new { id, assessmentId = result.Id }, result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating threat assessment for incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while creating the threat assessment");
        }
    }

    /// <summary>
    /// Get threat assessment for incident
    /// </summary>
    [HttpGet("{id}/threat-assessment/{assessmentId}")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public Task<ActionResult<ThreatAssessmentDto>> GetThreatAssessment(int id, int assessmentId)
    {
        try
        {
            // This would typically be implemented with a specific query
            // For now, return NotImplemented as placeholder
            return Task.FromResult<ActionResult<ThreatAssessmentDto>>(StatusCode(501, "Threat assessment retrieval not yet implemented"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving threat assessment {AssessmentId} for incident {IncidentId}", assessmentId, id);
            return Task.FromResult<ActionResult<ThreatAssessmentDto>>(StatusCode(500, "An error occurred while retrieving the threat assessment"));
        }
    }

    /// <summary>
    /// Escalate security incident
    /// </summary>
    [HttpPost("{id}/escalate")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Approve)]
    public async Task<ActionResult> EscalateIncident(int id, [FromBody] EscalateIncidentRequest request)
    {
        try
        {
            var result = await _securityIncidentService.EscalateIncidentAsync(id, request.Reason, request.EscalatedById);
            if (result)
            {
                return Ok(new { message = "Incident escalated successfully", newStatus = "Escalated" });
            }
            return BadRequest("Failed to escalate incident");
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while escalating the security incident");
        }
    }

    /// <summary>
    /// Assign security incident
    /// </summary>
    [HttpPost("{id}/assign")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Update)]
    public async Task<ActionResult> AssignIncident(int id, [FromBody] AssignIncidentRequest request)
    {
        try
        {
            var result = await _securityIncidentService.AssignIncidentAsync(id, request.AssigneeId, request.AssignedById);
            if (result)
            {
                return Ok(new { message = "Incident assigned successfully" });
            }
            return BadRequest("Failed to assign incident");
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while assigning the security incident");
        }
    }

    /// <summary>
    /// Close security incident
    /// </summary>
    [HttpPost("{id}/close")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Approve)]
    public async Task<ActionResult<SecurityIncidentDto>> CloseIncident(int id, [FromBody] CloseIncidentRequest request)
    {
        try
        {
            var result = await _securityIncidentService.CloseIncidentAsync(id, request.RootCause, request.ClosedById);
            if (result)
            {
                var incident = await _securityIncidentService.GetIncidentDetailAsync(id);
                return Ok(incident);
            }
            return BadRequest("Failed to close incident");
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while closing the security incident");
        }
    }

    /// <summary>
    /// Get recommended security controls for incident
    /// </summary>
    [HttpGet("{id}/controls/recommendations")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<List<SecurityControlDto>>> GetRecommendedControls(int id)
    {
        try
        {
            var result = await _securityIncidentService.RecommendControlsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended controls for security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while getting recommended controls");
        }
    }

    /// <summary>
    /// Get related incidents
    /// </summary>
    [HttpGet("{id}/related")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<List<SecurityIncidentDto>>> GetRelatedIncidents(int id)
    {
        try
        {
            var result = await _securityIncidentService.GetRelatedIncidentsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting related incidents for security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while getting related incidents");
        }
    }

    /// <summary>
    /// Generate compliance report for incident
    /// </summary>
    [HttpPost("{id}/compliance-report")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Export)]
    public async Task<ActionResult<SecurityComplianceReportDto>> GenerateComplianceReport(int id)
    {
        try
        {
            var result = await _securityIncidentService.GenerateComplianceReportAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating compliance report for security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while generating the compliance report");
        }
    }

    /// <summary>
    /// Get incident analytics
    /// </summary>
    [HttpGet("{id}/analytics")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<SecurityIncidentAnalyticsDto>> GetIncidentAnalytics(int id)
    {
        try
        {
            var result = await _securityIncidentService.GetIncidentAnalyticsAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while getting incident analytics");
        }
    }

    /// <summary>
    /// Link security incident to safety incident
    /// </summary>
    [HttpPost("{id}/link-safety-incident")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Update)]
    public async Task<ActionResult<SecurityIncidentDto>> LinkToSafetyIncident(
        int id,
        [FromBody] LinkToSafetyIncidentRequest request)
    {
        try
        {
            var result = await _securityIncidentService.LinkToSafetyIncidentAsync(id, request.SafetyIncidentId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking security incident {IncidentId} to safety incident {SafetyIncidentId}", 
                id, request.SafetyIncidentId);
            return StatusCode(500, "An error occurred while linking to safety incident");
        }
    }

    /// <summary>
    /// Send notification for incident
    /// </summary>
    [HttpPost("{id}/notify")]
    [RequireModulePermission(ModuleType.SecurityIncidentManagement, PermissionType.Update)]
    public async Task<ActionResult> SendNotification(int id, [FromBody] SendNotificationRequest request)
    {
        try
        {
            var result = await _securityIncidentService.SendNotificationAsync(id, request.Message, request.RecipientIds);
            if (result)
            {
                return Ok(new { message = "Notification sent successfully" });
            }
            return BadRequest("Failed to send notification");
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Security incident with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification for security incident {IncidentId}", id);
            return StatusCode(500, "An error occurred while sending the notification");
        }
    }
}

#region Request Models

public class EscalateIncidentRequest
{
    public string Reason { get; set; } = string.Empty;
    public int EscalatedById { get; set; }
}

public class AssignIncidentRequest
{
    public int AssigneeId { get; set; }
    public int AssignedById { get; set; }
}

public class CloseIncidentRequest
{
    public string RootCause { get; set; } = string.Empty;
    public int ClosedById { get; set; }
}

public class LinkToSafetyIncidentRequest
{
    public int SafetyIncidentId { get; set; }
}

public class SendNotificationRequest
{
    public string Message { get; set; } = string.Empty;
    public List<int> RecipientIds { get; set; } = new();
}

#endregion