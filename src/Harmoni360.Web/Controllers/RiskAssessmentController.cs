using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Harmoni360.Application.Features.RiskAssessments.Commands;
using Harmoni360.Application.Features.RiskAssessments.Queries;
using Harmoni360.Application.Features.RiskAssessments.DTOs;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RiskAssessmentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RiskAssessmentController> _logger;

    public RiskAssessmentController(IMediator mediator, ILogger<RiskAssessmentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all risk assessments with filtering, pagination, and sorting
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<GetRiskAssessmentsResponse>> GetRiskAssessments([FromQuery] GetRiskAssessmentsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessments");
            return StatusCode(500, "An error occurred while retrieving risk assessments");
        }
    }

    /// <summary>
    /// Get a specific risk assessment by ID with full details
    /// </summary>
    [HttpGet("{id:int}")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<RiskAssessmentDetailDto>> GetRiskAssessment(int id)
    {
        try
        {
            var query = new GetRiskAssessmentByIdQuery(id);
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"Risk assessment with ID {id} not found or access denied");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessment {RiskAssessmentId}", id);
            return StatusCode(500, "An error occurred while retrieving the risk assessment");
        }
    }

    /// <summary>
    /// Get risk assessments requiring review (due for reassessment)
    /// </summary>
    [HttpGet("due-for-review")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<GetRiskAssessmentsResponse>> GetRiskAssessmentsDueForReview([FromQuery] GetRiskAssessmentsQuery query)
    {
        try
        {
            // Filter for assessments where NextReviewDate <= today
            var result = await _mediator.Send(query);
            
            // TODO: Add server-side filtering for due date in the query handler
            var filteredAssessments = result.RiskAssessments
                .Where(ra => ra.NextReviewDate <= DateTime.UtcNow)
                .ToList();

            var filteredResponse = new GetRiskAssessmentsResponse
            {
                RiskAssessments = filteredAssessments,
                TotalCount = filteredAssessments.Count,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)filteredAssessments.Count / query.PageSize),
                HasPreviousPage = query.PageNumber > 1,
                HasNextPage = query.PageNumber < Math.Ceiling((double)filteredAssessments.Count / query.PageSize)
            };

            return Ok(filteredResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessments due for review");
            return StatusCode(500, "An error occurred while retrieving risk assessments due for review");
        }
    }

    /// <summary>
    /// Get risk assessments by risk level
    /// </summary>
    [HttpGet("by-risk-level/{riskLevel}")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<GetRiskAssessmentsResponse>> GetRiskAssessmentsByRiskLevel(
        string riskLevel, 
        [FromQuery] GetRiskAssessmentsQuery query)
    {
        try
        {
            var queryWithFilter = query with { RiskLevel = riskLevel };
            var result = await _mediator.Send(queryWithFilter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessments by risk level {RiskLevel}", riskLevel);
            return StatusCode(500, "An error occurred while retrieving risk assessments");
        }
    }

    /// <summary>
    /// Get unapproved risk assessments requiring approval
    /// </summary>
    [HttpGet("pending-approval")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Configure)]
    public async Task<ActionResult<GetRiskAssessmentsResponse>> GetPendingApprovalRiskAssessments([FromQuery] GetRiskAssessmentsQuery query)
    {
        try
        {
            var queryWithFilter = query with { IsApproved = false };
            var result = await _mediator.Send(queryWithFilter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending approval risk assessments");
            return StatusCode(500, "An error occurred while retrieving pending approval risk assessments");
        }
    }

    /// <summary>
    /// Get risk assessment statistics and dashboard data
    /// </summary>
    [HttpGet("statistics")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult> GetRiskAssessmentStatistics()
    {
        try
        {
            // Get all active risk assessments for statistics
            var query = new GetRiskAssessmentsQuery { PageSize = 1000, IsActive = true };
            var result = await _mediator.Send(query);

            var statistics = new
            {
                TotalAssessments = result.TotalCount,
                ApprovedAssessments = result.RiskAssessments.Count(ra => ra.IsApproved),
                PendingApproval = result.RiskAssessments.Count(ra => !ra.IsApproved),
                DueForReview = result.RiskAssessments.Count(ra => ra.NextReviewDate <= DateTime.UtcNow),
                RiskLevelDistribution = result.RiskAssessments
                    .GroupBy(ra => ra.RiskLevel)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AssessmentTypeDistribution = result.RiskAssessments
                    .GroupBy(ra => ra.Type)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageRiskScore = result.RiskAssessments.Any() 
                    ? result.RiskAssessments.Average(ra => ra.RiskScore) 
                    : 0,
                HighRiskCount = result.RiskAssessments.Count(ra => ra.RiskLevel is "High" or "Critical"),
                CompletionRate = result.TotalCount > 0 
                    ? (double)result.RiskAssessments.Count(ra => ra.IsApproved) / result.TotalCount * 100 
                    : 0
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessment statistics");
            return StatusCode(500, "An error occurred while retrieving statistics");
        }
    }

    /// <summary>
    /// Create a new risk assessment
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Create)]
    public async Task<ActionResult<RiskAssessmentDto>> CreateRiskAssessment([FromBody] CreateRiskAssessmentCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetRiskAssessment), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating risk assessment for hazard {HazardId}", command.HazardId);
            return StatusCode(500, "An error occurred while creating the risk assessment");
        }
    }

    /// <summary>
    /// Update an existing risk assessment
    /// </summary>
    [HttpPut("{id:int}")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Update)]
    public async Task<ActionResult<RiskAssessmentDto>> UpdateRiskAssessment(int id, [FromBody] UpdateRiskAssessmentCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest("Risk assessment ID in URL does not match the ID in the request body");
            }

            var result = await _mediator.Send(command);
            
            if (result == null)
            {
                return NotFound($"Risk assessment with ID {id} not found or access denied");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating risk assessment {RiskAssessmentId}", id);
            return StatusCode(500, "An error occurred while updating the risk assessment");
        }
    }

    /// <summary>
    /// Delete a risk assessment
    /// </summary>
    [HttpDelete("{id:int}")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Delete)]
    public async Task<ActionResult> DeleteRiskAssessment(int id)
    {
        try
        {
            var command = new DeleteRiskAssessmentCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting risk assessment {RiskAssessmentId}", id);
            return StatusCode(500, "An error occurred while deleting the risk assessment");
        }
    }

    /// <summary>
    /// Approve a risk assessment
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Approve)]
    public async Task<ActionResult<RiskAssessmentDto>> ApproveRiskAssessment(int id, [FromBody] ApproveRiskAssessmentCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest("Risk assessment ID in URL does not match the ID in the request body");
            }

            var result = await _mediator.Send(command);
            
            if (result == null)
            {
                return NotFound($"Risk assessment with ID {id} not found or access denied");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving risk assessment {RiskAssessmentId}", id);
            return StatusCode(500, "An error occurred while approving the risk assessment");
        }
    }
}