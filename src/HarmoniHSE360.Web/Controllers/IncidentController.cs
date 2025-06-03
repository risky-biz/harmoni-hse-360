using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using HarmoniHSE360.Application.Features.Incidents.Commands;
using HarmoniHSE360.Application.Features.Incidents.DTOs;
using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IncidentController> _logger;

    public IncidentController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<IncidentController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new incident report
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        try
        {
            var command = new CreateIncidentCommand
            {
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                IncidentDate = request.IncidentDate,
                Location = request.Location,
                ReporterId = _currentUserService.UserId
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Incident created successfully with ID: {IncidentId}", result.Id);

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
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> GetIncident(int id)
    {
        try
        {
            // TODO: Implement GetIncidentQuery when query handlers are created
            _logger.LogInformation("Getting incident {IncidentId}", id);

            // For now, return a placeholder response
            return NotFound(new { message = "Incident not found or access denied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while retrieving the incident" });
        }
    }

    /// <summary>
    /// Get all incidents (with pagination and filtering)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetIncidents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] IncidentStatus? status = null,
        [FromQuery] IncidentSeverity? severity = null)
    {
        try
        {
            // TODO: Implement GetIncidentsQuery when query handlers are created
            _logger.LogInformation("Getting incidents for user {UserId}, page {Page}, pageSize {PageSize}",
                _currentUserService.UserId, page, pageSize);

            // For now, return empty list
            return Ok(new List<IncidentDto>());
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
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IncidentDto>> UpdateIncident(int id, [FromBody] UpdateIncidentRequest request)
    {
        try
        {
            // TODO: Implement UpdateIncidentCommand when update handlers are created
            _logger.LogInformation("Updating incident {IncidentId} by user {UserId}", id, _currentUserService.UserId);

            return NotFound(new { message = "Incident not found or access denied" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident {IncidentId}", id);
            return BadRequest(new { message = "An error occurred while updating the incident" });
        }
    }

    /// <summary>
    /// Get incidents reported by current user
    /// </summary>
    [HttpGet("my-reports")]
    [ProducesResponseType(typeof(IEnumerable<IncidentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetMyReports()
    {
        try
        {
            // TODO: Implement GetMyIncidentsQuery when query handlers are created
            _logger.LogInformation("Getting my incidents for user {UserId}", _currentUserService.UserId);

            // For now, return empty list
            return Ok(new List<IncidentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving my incidents for user {UserId}", _currentUserService.UserId);
            return BadRequest(new { message = "An error occurred while retrieving your incidents" });
        }
    }

    /// <summary>
    /// Get incident statistics for dashboard
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetIncidentStatistics()
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

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident statistics");
            return BadRequest(new { message = "An error occurred while retrieving statistics" });
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
    public IncidentSeverity Severity { get; set; }
    public DateTime IncidentDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>
/// Request model for updating an incident
/// </summary>
public class UpdateIncidentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IncidentSeverity? Severity { get; set; }
    public IncidentStatus? Status { get; set; }
    public string? Location { get; set; }
}