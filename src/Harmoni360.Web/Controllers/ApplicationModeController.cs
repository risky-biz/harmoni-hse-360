using Microsoft.AspNetCore.Mvc;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationModeController : ControllerBase
{
    private readonly IApplicationModeService _applicationModeService;
    private readonly ILogger<ApplicationModeController> _logger;

    public ApplicationModeController(
        IApplicationModeService applicationModeService,
        ILogger<ApplicationModeController> logger)
    {
        _applicationModeService = applicationModeService;
        _logger = logger;
    }

    /// <summary>
    /// Get application mode information
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(ApplicationModeInfo), StatusCodes.Status200OK)]
    public ActionResult<ApplicationModeInfo> GetModeInfo()
    {
        try
        {
            var modeInfo = _applicationModeService.GetModeInfo();
            return Ok(modeInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application mode information");
            return StatusCode(500, new { message = "An error occurred while retrieving mode information" });
        }
    }

    /// <summary>
    /// Get demo limitations for the current mode
    /// </summary>
    [HttpGet("limitations")]
    [ProducesResponseType(typeof(DemoLimitations), StatusCodes.Status200OK)]
    public ActionResult<DemoLimitations> GetLimitations()
    {
        try
        {
            var limitations = _applicationModeService.GetDemoLimitations();
            return Ok(limitations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving demo limitations");
            return StatusCode(500, new { message = "An error occurred while retrieving limitations" });
        }
    }

    /// <summary>
    /// Check if a specific operation is allowed
    /// </summary>
    [HttpGet("check-operation/{operationType}")]
    [ProducesResponseType(typeof(OperationCheckResult), StatusCodes.Status200OK)]
    public ActionResult<OperationCheckResult> CheckOperation(string operationType)
    {
        try
        {
            var isAllowed = _applicationModeService.IsOperationAllowed(operationType);
            var result = new OperationCheckResult
            {
                OperationType = operationType,
                IsAllowed = isAllowed,
                Reason = isAllowed ? "Operation allowed" : "Operation restricted in current mode"
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking operation {OperationType}", operationType);
            return StatusCode(500, new { message = "An error occurred while checking operation" });
        }
    }

    /// <summary>
    /// Get environment status (public endpoint for health checks)
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(EnvironmentStatus), StatusCodes.Status200OK)]
    public ActionResult<EnvironmentStatus> GetStatus()
    {
        try
        {
            var status = new EnvironmentStatus
            {
                Environment = _applicationModeService.Environment.ToString(),
                IsDemoMode = _applicationModeService.IsDemoMode,
                IsProductionMode = _applicationModeService.IsProductionMode,
                Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
                Timestamp = DateTime.UtcNow
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving environment status");
            return StatusCode(500, new { message = "An error occurred while retrieving status" });
        }
    }
}

public class OperationCheckResult
{
    public string OperationType { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class EnvironmentStatus
{
    public string Environment { get; set; } = string.Empty;
    public bool IsDemoMode { get; set; }
    public bool IsProductionMode { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}