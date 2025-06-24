using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Harmoni360.Application.Features.HSSE.Queries;
using Harmoni360.Application.Features.HSSE.DTOs;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HSSEController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HSSEController> _logger;

    public HSSEController(
        IMediator mediator,
        ILogger<HSSEController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive HSSE dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.Dashboard, PermissionType.Read)]
    public async Task<ActionResult<HSSEDashboardDto>> GetDashboard(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null,
        [FromQuery] string? location = null,
        [FromQuery] bool includeTrends = true,
        [FromQuery] bool includeComparisons = true)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                Location = location,
                IncludeTrends = includeTrends,
                IncludeComparisons = includeComparisons
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting HSSE dashboard data");
            return StatusCode(500, "An error occurred while retrieving HSSE dashboard data");
        }
    }

    /// <summary>
    /// Get hazard statistics summary
    /// </summary>
    [HttpGet("statistics/hazards")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<HazardStatisticsDto>> GetHazardStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = false,
                IncludeComparisons = false
            };

            var result = await _mediator.Send(query);
            return Ok(result.HazardStatistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting hazard statistics");
            return StatusCode(500, "An error occurred while retrieving hazard statistics");
        }
    }

    /// <summary>
    /// Get monthly hazard trends
    /// </summary>
    [HttpGet("trends/monthly")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<List<MonthlyHazardDto>>> GetMonthlyTrends(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = true,
                IncludeComparisons = false
            };

            var result = await _mediator.Send(query);
            return Ok(result.MonthlyHazards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting monthly trends");
            return StatusCode(500, "An error occurred while retrieving monthly trends");
        }
    }

    /// <summary>
    /// Get hazard classification breakdown
    /// </summary>
    [HttpGet("classifications")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<List<HazardClassificationDto>>> GetHazardClassifications(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = false,
                IncludeComparisons = false
            };

            var result = await _mediator.Send(query);
            return Ok(result.HazardClassifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting hazard classifications");
            return StatusCode(500, "An error occurred while retrieving hazard classifications");
        }
    }

    /// <summary>
    /// Get top unsafe conditions
    /// </summary>
    [HttpGet("unsafe-conditions")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<List<UnsafeConditionDto>>> GetUnsafeConditions(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null,
        [FromQuery] int limit = 10)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = false,
                IncludeComparisons = false
            };

            var result = await _mediator.Send(query);
            return Ok(result.TopUnsafeConditions.Take(limit));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting unsafe conditions");
            return StatusCode(500, "An error occurred while retrieving unsafe conditions");
        }
    }

    /// <summary>
    /// Get incident frequency rates
    /// </summary>
    [HttpGet("rates/incident-frequency")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<List<IncidentFrequencyRateDto>>> GetIncidentFrequencyRates(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = true,
                IncludeComparisons = true
            };

            var result = await _mediator.Send(query);
            return Ok(result.IncidentFrequencyRates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting incident frequency rates");
            return StatusCode(500, "An error occurred while retrieving incident frequency rates");
        }
    }

    /// <summary>
    /// Get safety performance data
    /// </summary>
    [HttpGet("performance/safety")]
    [RequireModulePermission(ModuleType.Dashboard, PermissionType.Read)]
    public async Task<ActionResult<List<SafetyPerformanceDto>>> GetSafetyPerformance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = true,
                IncludeComparisons = true
            };

            var result = await _mediator.Send(query);
            return Ok(result.SafetyPerformance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting safety performance data");
            return StatusCode(500, "An error occurred while retrieving safety performance data");
        }
    }

    /// <summary>
    /// Get responsible actions summary
    /// </summary>
    [HttpGet("actions/responsible")]
    [RequireModulePermission(ModuleType.RiskManagement, PermissionType.Read)]
    public async Task<ActionResult<ResponsibleActionSummaryDto>> GetResponsibleActions(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? department = null)
    {
        try
        {
            var query = new GetHSSEDashboardQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Department = department,
                IncludeTrends = false,
                IncludeComparisons = false
            };

            var result = await _mediator.Send(query);
            return Ok(result.ResponsibleActions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting responsible actions");
            return StatusCode(500, "An error occurred while retrieving responsible actions");
        }
    }
}