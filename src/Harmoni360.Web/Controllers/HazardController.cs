using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Features.Hazards.Commands;
using Harmoni360.Application.Features.Hazards.Queries;
using Harmoni360.Application.Features.Hazards.DTOs;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using System.Collections.Generic;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HazardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<HazardController> _logger;
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public HazardController(
        IMediator mediator,
	ICurrentUserService currentUserService,
        ILogger<HazardController> logger,
        IApplicationDbContext context,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
        _context = context;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Get all hazards with filtering, pagination, and sorting
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<GetHazardsResponse>> GetHazards([FromQuery] GetHazardsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hazards");
            return StatusCode(500, "An error occurred while retrieving hazards");
        }
    }

    /// <summary>
    /// Get a specific hazard by ID with full details
    /// </summary>
    [HttpGet("{id:int}")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<HazardDetailDto>> GetHazard(int id, [FromQuery] bool includeAttachments = true, 
        [FromQuery] bool includeRiskAssessments = true, [FromQuery] bool includeMitigationActions = true)
    {
        try
        {
            var query = new GetHazardByIdQuery
            {
                Id = id,
                IncludeAttachments = includeAttachments,
                IncludeRiskAssessments = includeRiskAssessments,
                IncludeMitigationActions = includeMitigationActions
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Hazard with ID {id} not found or access denied");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hazard {HazardId}", id);
            return StatusCode(500, "An error occurred while retrieving the hazard");
        }
    }

    /// <summary>
    /// Create a new hazard report
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Create)]
    public async Task<ActionResult<HazardDto>> CreateHazard([FromForm] CreateHazardCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHazard), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data provided for hazard creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hazard");
            return StatusCode(500, "An error occurred while creating the hazard");
        }
    }

    /// <summary>
    /// Update an existing hazard
    /// </summary>
    [HttpPut("{id:int}")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Update)]
    public async Task<ActionResult<HazardDto>> UpdateHazard(int id, [FromForm] UpdateHazardCommand command)
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
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data provided for hazard update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hazard {HazardId}", id);
            return StatusCode(500, "An error occurred while updating the hazard");
        }
    }

    /// <summary>
    /// Get dashboard metrics and analytics
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "SuperAdmin,Developer,Admin,HSEManager")]
    public async Task<ActionResult<HazardDashboardDto>> GetDashboard([FromQuery] GetHazardDashboardQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hazard dashboard");
            return StatusCode(500, "An error occurred while retrieving dashboard data");
        }
    }

    /// <summary>
    /// Create a risk assessment for a hazard
    /// </summary>
    [HttpPost("{id:int}/risk-assessment")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Configure)]
    public async Task<ActionResult<RiskAssessmentDto>> CreateRiskAssessment(int id, [FromBody] CreateRiskAssessmentCommand command)
    {
        try
        {
            if (id != command.HazardId)
            {
                return BadRequest("Hazard ID in URL does not match hazard ID in request body");
            }

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHazard), new { id = command.HazardId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data provided for risk assessment creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating risk assessment for hazard {HazardId}", id);
            return StatusCode(500, "An error occurred while creating the risk assessment");
        }
    }

    /// <summary>
    /// Create a mitigation action for a hazard
    /// </summary>
    [HttpPost("{id:int}/mitigation-action")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Configure)]
    public async Task<ActionResult<HazardMitigationActionDto>> CreateMitigationAction(int id, [FromBody] CreateMitigationActionCommand command)
    {
        try
        {
            if (id != command.HazardId)
            {
                return BadRequest("Hazard ID in URL does not match hazard ID in request body");
            }

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHazard), new { id = command.HazardId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data provided for mitigation action creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mitigation action for hazard {HazardId}", id);
            return StatusCode(500, "An error occurred while creating the mitigation action");
        }
    }

    /// <summary>
    /// Get attachments for a specific hazard
    /// </summary>
    [HttpGet("{id:int}/attachments")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult> GetHazardAttachments(int id)
    {
        try
        {
            var query = new GetHazardByIdQuery
            {
                Id = id,
                IncludeAttachments = true,
                IncludeRiskAssessments = false,
                IncludeMitigationActions = false
            };

            var hazard = await _mediator.Send(query);
            
            if (hazard == null)
            {
                return NotFound($"Hazard with ID {id} not found or access denied");
            }

            return Ok(hazard.Attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for hazard {HazardId}", id);
            return StatusCode(500, "An error occurred while retrieving attachments");
        }
    }

    /// <summary>
    /// Download a specific attachment
    /// </summary>
    [HttpGet("{id:int}/attachments/{attachmentId:int}/download")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult> DownloadAttachment(int id, int attachmentId)
    {
        try
        {
            var attachment = await _context.HazardAttachments
                .Where(a => a.Id == attachmentId && a.HazardId == id)
                .FirstOrDefaultAsync();

            if (attachment == null)
            {
                return NotFound(new { message = "Attachment not found" });
            }

            var fileStream = await _fileStorageService.DownloadAsync(attachment.FilePath);
            var contentType = GetContentType(attachment.FileName);

            return File(fileStream, contentType, attachment.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {AttachmentId} for hazard {HazardId}", attachmentId, id);
            return StatusCode(500, "An error occurred while downloading the attachment");
        }
    }

    /// <summary>
    /// Get audit trail for a specific hazard
    /// </summary>
    [HttpGet("{id:int}/audit-trail")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<List<HazardAuditLogDto>>> GetHazardAuditTrail(int id)
    {
        try
        {
            var query = new GetHazardAuditTrailQuery { HazardId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for hazard {HazardId}", id);
            return StatusCode(500, "An error occurred while retrieving the audit trail");
        }
    }

    /// <summary>
    /// Get hazard locations for mapping
    /// </summary>
    [HttpGet("locations")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult> GetHazardLocations([FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHazardsQuery
            {
                Department = department,
                PageSize = 1000, // Get more results for mapping
                IncludeReporter = false
            };

            var result = await _mediator.Send(query);
            
            var locations = result.Hazards
                .Where(h => h.Latitude.HasValue && h.Longitude.HasValue)
                .Select(h => new
                {
                    h.Id,
                    h.Title,
                    h.Location,
                    h.Latitude,
                    h.Longitude,
                    h.Severity,
                    h.Status,
                    h.CurrentRiskLevel
                })
                .ToList();

            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hazard locations");
            return StatusCode(500, "An error occurred while retrieving hazard locations");
        }
    }

    /// <summary>
    /// Get hazards near a specific location
    /// </summary>
    [HttpGet("nearby")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult> GetNearbyHazards([FromQuery] double latitude, [FromQuery] double longitude, 
        [FromQuery] double radiusKm = 1.0)
    {
        try
        {
            var query = new GetHazardsQuery
            {
                Latitude = latitude,
                Longitude = longitude,
                RadiusKm = radiusKm,
                PageSize = 50
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nearby hazards");
            return StatusCode(500, "An error occurred while retrieving nearby hazards");
        }
    }

    /// <summary>
    /// Get hazards reported by current user
    /// </summary>
    [HttpGet("my-hazards")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(GetHazardsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetHazardsResponse>> GetMyHazards(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetMyHazardsQuery
            {
                SearchTerm = searchTerm,
                Status = status,
                Severity = severity,
                PageNumber = pageNumber,
                PageSize = pageSize,
                UserId = _currentUserService.UserId
            };

            _logger.LogInformation("Getting my hazards for user {UserId} with filters: {@Query}",
                _currentUserService.UserId, query);

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my hazards for user {UserId}", _currentUserService.UserId);
            return StatusCode(500, "An error occurred while retrieving your hazards");
        }
    }

    /// <summary>
    /// Get unassessed hazards requiring risk assessment
    /// </summary>
    [HttpGet("unassessed")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Configure)]
    public async Task<ActionResult<GetHazardsResponse>> GetUnassessedHazards([FromQuery] GetHazardsQuery query)
    {
        try
        {
            query = query with { OnlyUnassessed = true };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unassessed hazards");
            return StatusCode(500, "An error occurred while retrieving unassessed hazards");
        }
    }

    /// <summary>
    /// Get overdue hazards and actions
    /// </summary>
    [HttpGet("overdue")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Configure)]
    public async Task<ActionResult<GetHazardsResponse>> GetOverdueHazards([FromQuery] GetHazardsQuery query)
    {
        try
        {
            query = query with { OnlyOverdue = true };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue hazards");
            return StatusCode(500, "An error occurred while retrieving overdue hazards");
        }
    }

    /// <summary>
    /// Get high-risk hazards
    /// </summary>
    [HttpGet("high-risk")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Configure)]
    public async Task<ActionResult<GetHazardsResponse>> GetHighRiskHazards([FromQuery] GetHazardsQuery query)
    {
        try
        {
            query = query with { OnlyHighRisk = true };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving high-risk hazards");
            return StatusCode(500, "An error occurred while retrieving high-risk hazards");
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".mp4" => "video/mp4",
            ".avi" => "video/avi",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }
}
