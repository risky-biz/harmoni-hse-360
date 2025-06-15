using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using Harmoni360.Application.Features.Incidents.Commands;
using Harmoni360.Application.Features.Incidents.Queries;
using Harmoni360.Application.Features.Incidents.DTOs;
using Harmoni360.Application.Features.IncidentAudit.Queries;
using Harmoni360.Application.Features.IncidentAudit.DTOs;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Hubs;
using Harmoni360.Web.Authorization;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IncidentController> _logger;
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<IncidentHub> _incidentHub;
    private readonly IIncidentAuditService _auditService;
    private readonly IIncidentCacheService _cacheService;

    public IncidentController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<IncidentController> logger,
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        IHubContext<IncidentHub> incidentHub,
        IIncidentAuditService auditService,
        IIncidentCacheService cacheService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
        _context = context;
        _fileStorageService = fileStorageService;
        _incidentHub = incidentHub;
        _auditService = auditService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Create a new incident report
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Create)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        try
        {
            // Parse enum value from string
            if (!Enum.TryParse<IncidentSeverity>(request.Severity, out var severity))
            {
                return BadRequest(new { message = $"Invalid severity value: {request.Severity}" });
            }

            var command = new CreateIncidentCommand
            {
                Title = request.Title,
                Description = request.Description,
                Severity = severity,
                IncidentDate = request.IncidentDate,
                Location = request.Location,
                CategoryId = request.CategoryId,
                DepartmentId = request.DepartmentId,
                LocationId = request.LocationId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                WitnessNames = request.WitnessNames,
                ImmediateActionsTaken = request.ImmediateActionsTaken,
                ReporterId = _currentUserService.UserId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Incident created successfully with ID: {IncidentId}", result.Id);

            // Notify all connected clients about the new incident
            await _incidentHub.Clients.All.SendAsync("IncidentCreated", result.Id);
            
            // Notify dashboard should be updated
            await _incidentHub.Clients.All.SendAsync("DashboardUpdate");

            // Also notify location-specific group if location is specified
            if (!string.IsNullOrEmpty(result.Location))
            {
                await _incidentHub.Clients.Group($"location-{result.Location}")
                    .SendAsync("IncidentCreated", result.Id);
            }

            // Ensure server-side cache is also invalidated for immediate consistency
            await _cacheService.InvalidateAllIncidentCachesAsync();
            _logger.LogInformation("Server-side incident caches invalidated after creating incident {IncidentId}", result.Id);

            return CreatedAtAction(nameof(GetIncident), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating incident for user {UserId}", _currentUserService.UserId);
            return BadRequest(new { message = "An error occurred while creating the incident" });
        }
    }

    /// <summary>
    /// Get incident by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> GetIncident(int id)
    {
        try
        {
            _logger.LogInformation("Getting incident {IncidentId} for user {UserId}", id, _currentUserService.UserId);

            var query = new GetIncidentByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Incident not found or access denied" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while retrieving the incident" });
        }
    }

    /// <summary>
    /// Get incident details with full information
    /// </summary>
    [HttpGet("{id}/detail")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDetailDto>> GetIncidentDetail(int id)
    {
        try
        {
            _logger.LogInformation("Getting incident detail {IncidentId} for user {UserId}", id, _currentUserService.UserId);

            var query = new GetIncidentDetailQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Incident not found or access denied" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident detail {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while retrieving the incident details" });
        }
    }

    /// <summary>
    /// Get all incidents (with pagination and filtering)
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(GetIncidentsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetIncidentsResponse>> GetIncidents(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetIncidentsQuery
            {
                SearchTerm = searchTerm,
                Status = status,
                Severity = severity,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Getting incidents for user {UserId} with filters: {@Query}",
                _currentUserService.UserId, query);

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incidents for user {UserId}", _currentUserService.UserId);
            return BadRequest(new { message = "An error occurred while retrieving incidents" });
        }
    }

    /// <summary>
    /// Update incident status or details
    /// </summary>
    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IncidentDto>> UpdateIncident(int id, [FromBody] UpdateIncidentRequest request)
    {
        try
        {
            _logger.LogInformation("Updating incident {IncidentId} by user {UserId}", id, _currentUserService.UserId);
            _logger.LogInformation("Request data: Title={Title}, Description={Description}, Severity={Severity}, Status={Status}, Location={Location}",
                request?.Title, request?.Description, request?.Severity, request?.Status, request?.Location);

            if (request == null)
            {
                return BadRequest(new { message = "Request body is required" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState validation failed: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            // Parse enum values from strings
            if (!Enum.TryParse<IncidentSeverity>(request.Severity, out var severity))
            {
                severity = IncidentSeverity.Minor;
            }

            if (!Enum.TryParse<IncidentStatus>(request.Status, out var status))
            {
                status = IncidentStatus.Reported;
            }

            var command = new UpdateIncidentCommand
            {
                Id = id,
                Title = request.Title ?? string.Empty,
                Description = request.Description ?? string.Empty,
                Severity = severity,
                Status = status,
                Location = request.Location ?? string.Empty
            };

            var result = await _mediator.Send(command);

            if (result == null)
            {
                return NotFound(new { message = "Incident not found or access denied" });
            }

            // Notify all connected clients about the updated incident
            await _incidentHub.Clients.All.SendAsync("IncidentUpdated", result.Id);
            
            // Notify dashboard should be updated
            await _incidentHub.Clients.All.SendAsync("DashboardUpdate");

            // If status changed, send specific notification
            if (request.Status != null)
            {
                await _incidentHub.Clients.All.SendAsync("IncidentStatusChanged", new
                {
                    incidentId = result.Id,
                    newStatus = result.Status
                });
            }

            // Ensure server-side cache is also invalidated for immediate consistency
            await _cacheService.InvalidateAllIncidentCachesAsync();
            _logger.LogInformation("Server-side incident caches invalidated after updating incident {IncidentId}", result.Id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while updating the incident" });
        }
    }

    /// <summary>
    /// Update incident status
    /// </summary>
    [HttpPut("{id}/status")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateIncidentStatus(int id, [FromBody] UpdateIncidentStatusRequest request)
    {
        try
        {
            var command = new UpdateIncidentStatusCommand
            {
                Id = id,
                Status = request.Status,
                Comment = request.Comment,
                ModifiedBy = _currentUserService.Email ?? "system"
            };

            await _mediator.Send(command);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident status for incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while updating the incident status" });
        }
    }

    /// <summary>
    /// Get incidents reported by current user
    /// </summary>
    [HttpGet("my-reports")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(GetIncidentsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetIncidentsResponse>> GetMyReports(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetMyIncidentsQuery
            {
                SearchTerm = searchTerm,
                Status = status,
                Severity = severity,
                PageNumber = pageNumber,
                PageSize = pageSize,
                UserId = _currentUserService.UserId
            };

            _logger.LogInformation("Getting my incidents for user {UserId} with filters: {@Query}",
                _currentUserService.UserId, query);

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my incidents for user {UserId}", _currentUserService.UserId);
            return BadRequest(new { message = "An error occurred while retrieving your incidents" });
        }
    }

    /// <summary>
    /// Delete an incident
    /// </summary>
    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteIncident(int id)
    {
        try
        {
            _logger.LogInformation("Deleting incident {IncidentId} by user {UserId}", id, _currentUserService.UserId);

            var command = new DeleteIncidentCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = "Incident not found or access denied" });
            }

            // Notify all connected clients about the deleted incident
            await _incidentHub.Clients.All.SendAsync("IncidentDeleted", id);
            
            // Notify dashboard should be updated
            await _incidentHub.Clients.All.SendAsync("DashboardUpdate");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while deleting the incident" });
        }
    }

    /// <summary>
    /// Get incident statistics for dashboard
    /// </summary>
    [HttpGet("statistics")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public Task<ActionResult<object>> GetIncidentStatistics()
    {
        try
        {
            // TODO: Implement incident statistics query
            _logger.LogInformation("Getting incident statistics");

            // Return mock statistics for now
            var stats = new
            {
                TotalIncidents = 0,
                OpenIncidents = 0,
                ClosedIncidents = 0,
                CriticalIncidents = 0,
                IncidentsByMonth = new object[] { }
            };

            return Task.FromResult<ActionResult<object>>(Ok(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident statistics");
            return Task.FromResult<ActionResult<object>>(BadRequest(new { message = "An error occurred while retrieving statistics" }));
        }
    }

    /// <summary>
    /// Get comprehensive incident dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(IncidentDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDashboardDto>> GetIncidentDashboard(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? department = null,
        [FromQuery] bool includeResolved = true)
    {
        try
        {
            _logger.LogInformation("Getting incident dashboard data");

            var query = new GetIncidentDashboardQuery
            {
                FromDate = fromDate,
                ToDate = toDate,
                Department = department,
                IncludeResolved = includeResolved
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident dashboard data");
            return BadRequest(new { message = "An error occurred while retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Upload attachments for an incident
    /// </summary>
    [HttpPost("{id:int}/attachments")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadAttachments(int id, [FromForm] IFormFileCollection files)
    {
        try
        {
            _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
            _logger.LogInformation("Form keys: {FormKeys}", string.Join(", ", Request.Form.Keys));
            _logger.LogInformation("Files in Request.Form.Files: {RequestFileCount}", Request.Form.Files.Count);
            _logger.LogInformation("Uploading {FileCount} attachments for incident {IncidentId}", files?.Count ?? 0, id);

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files provided" });
            }

            var command = new AddIncidentAttachmentsCommand
            {
                IncidentId = id,
                Files = files,
                UploadedBy = _currentUserService.Email ?? "system"
            };

            var result = await _mediator.Send(command);

            return Ok(new
            {
                attachments = result.Attachments,
                message = result.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "File validation failed for incident {IncidentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Incident {IncidentId} not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachments for incident {IncidentId}", id);
            return StatusCode(500, new { message = "An error occurred while uploading files" });
        }
    }

    /// <summary>
    /// Get attachments for an incident
    /// </summary>
    [HttpGet("{id}/attachments")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(List<IncidentAttachmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<IncidentAttachmentDto>>> GetIncidentAttachments(int id)
    {
        try
        {
            var query = new GetIncidentAttachmentsQuery { IncidentId = id };
            var attachments = await _mediator.Send(query);
            return Ok(attachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attachments for incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while retrieving attachments" });
        }
    }

    /// <summary>
    /// Download an attachment
    /// </summary>
    [HttpGet("{incidentId}/attachments/{attachmentId}/download")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadAttachment(int incidentId, int attachmentId)
    {
        try
        {
            var attachment = await _context.IncidentAttachments
                .Where(a => a.Id == attachmentId && a.IncidentId == incidentId)
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
            _logger.LogError(ex, "Error downloading attachment {AttachmentId} for incident {IncidentId}",
                attachmentId, incidentId);
            return BadRequest(new { message = "An error occurred while downloading the file" });
        }
    }

    /// <summary>
    /// Delete an attachment
    /// </summary>
    [HttpDelete("{incidentId}/attachments/{attachmentId}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAttachment(int incidentId, int attachmentId)
    {
        try
        {
            var attachment = await _context.IncidentAttachments
                .Where(a => a.Id == attachmentId && a.IncidentId == incidentId)
                .FirstOrDefaultAsync();

            if (attachment == null)
            {
                return NotFound(new { message = "Attachment not found" });
            }

            // Delete from storage
            await _fileStorageService.DeleteAsync(attachment.FilePath);

            // Delete from database
            _context.IncidentAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            // Log audit trail for attachment removal
            await _auditService.LogAttachmentRemovedAsync(incidentId, attachment.FileName);

            // Save audit trail entry
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted attachment {AttachmentId} ({FileName}) for incident {IncidentId}",
                attachmentId, attachment.FileName, incidentId);

            return Ok(new { message = "Attachment deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId} for incident {IncidentId}",
                attachmentId, incidentId);
            return BadRequest(new { message = "An error occurred while deleting the attachment" });
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

    /// <summary>
    /// Add an involved person to an incident
    /// </summary>
    [HttpPost("{id}/involved-persons")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddInvolvedPerson(int id, [FromBody] AddInvolvedPersonRequest request)
    {
        try
        {
            _logger.LogInformation("Adding involved person {PersonId} to incident {IncidentId}", request.PersonId, id);

            if (!Enum.TryParse<InvolvementType>(request.InvolvementType, out var involvementType))
            {
                return BadRequest(new { message = $"Invalid involvement type: {request.InvolvementType}" });
            }

            var command = new AddInvolvedPersonCommand
            {
                IncidentId = id,
                PersonId = request.PersonId,
                InvolvementType = involvementType,
                InjuryDescription = request.InjuryDescription,
                ManualPersonName = request.ManualPersonName,
                ManualPersonEmail = request.ManualPersonEmail
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to add involved person to incident {IncidentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding involved person to incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while adding the involved person" });
        }
    }

    /// <summary>
    /// Update an involved person's details
    /// </summary>
    [HttpPut("{id}/involved-persons/{personId}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInvolvedPerson(int id, int personId, [FromBody] UpdateInvolvedPersonRequest request)
    {
        try
        {
            _logger.LogInformation("Updating involved person {PersonId} in incident {IncidentId}", personId, id);

            if (!Enum.TryParse<InvolvementType>(request.InvolvementType, out var involvementType))
            {
                return BadRequest(new { message = $"Invalid involvement type: {request.InvolvementType}" });
            }

            var command = new UpdateInvolvedPersonCommand
            {
                IncidentId = id,
                PersonId = personId,
                InvolvementType = involvementType,
                InjuryDescription = request.InjuryDescription
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update involved person in incident {IncidentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating involved person in incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while updating the involved person" });
        }
    }

    /// <summary>
    /// Remove an involved person from an incident
    /// </summary>
    [HttpDelete("{id}/involved-persons/{personId}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveInvolvedPerson(int id, int personId)
    {
        try
        {
            _logger.LogInformation("Removing involved person {PersonId} from incident {IncidentId}", personId, id);

            var command = new RemoveInvolvedPersonCommand
            {
                IncidentId = id,
                PersonId = personId
            };

            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to remove involved person from incident {IncidentId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing involved person from incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while removing the involved person" });
        }
    }

    /// <summary>
    /// Get available users for selection as involved persons
    /// </summary>
    [HttpGet("available-users")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetAvailableUsers([FromQuery] string? searchTerm = null)
    {
        try
        {
            var query = new GetAvailableUsersQuery { SearchTerm = searchTerm };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available users");
            return BadRequest(new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get audit trail for an incident
    /// </summary>
    /// <param name="id">Incident ID</param>
    /// <returns>Audit trail for the incident</returns>
    [HttpGet("{id}/audit-trail")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(List<IncidentAuditLogDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<List<IncidentAuditLogDto>>> GetIncidentAuditTrail(int id)
    {
        try
        {
            var query = new GetIncidentAuditTrailQuery(id);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while retrieving audit trail" });
        }
    }
}

/// <summary>
/// Request model for creating an incident
/// </summary>
public class CreateIncidentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int? DepartmentId { get; set; }
    public int? LocationId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? WitnessNames { get; set; }
    public string? ImmediateActionsTaken { get; set; }
}

/// <summary>
/// Request model for updating an incident
/// </summary>
public class UpdateIncidentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// Request model for adding an involved person
/// </summary>
public class AddInvolvedPersonRequest
{
    public int PersonId { get; set; }
    public string InvolvementType { get; set; } = string.Empty;
    public string? InjuryDescription { get; set; }
    public string? ManualPersonName { get; set; }
    public string? ManualPersonEmail { get; set; }
}

/// <summary>
/// Request model for updating an involved person
/// </summary>
public class UpdateInvolvedPersonRequest
{
    public string InvolvementType { get; set; } = string.Empty;
    public string? InjuryDescription { get; set; }
}

/// <summary>
/// Request model for updating incident status
/// </summary>
public class UpdateIncidentStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}