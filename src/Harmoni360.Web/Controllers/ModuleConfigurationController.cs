using Harmoni360.Application.Features.ModuleConfiguration.Commands;
using Harmoni360.Application.Features.ModuleConfiguration.DTOs;
using Harmoni360.Application.Features.ModuleConfiguration.Queries;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

/// <summary>
/// Controller for managing module configurations and dynamic module enable/disable functionality
/// </summary>
[ApiController]
[Route("api/module-configuration")]
[Authorize]
public class ModuleConfigurationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ModuleConfigurationController> _logger;
    private readonly IModuleConfigurationService _moduleConfigurationService;

    public ModuleConfigurationController(
        IMediator mediator, 
        ILogger<ModuleConfigurationController> logger,
        IModuleConfigurationService moduleConfigurationService)
    {
        _mediator = mediator;
        _logger = logger;
        _moduleConfigurationService = moduleConfigurationService;
    }

    /// <summary>
    /// Get all module configurations
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<ModuleConfigurationDto>>> GetModuleConfigurations(
        [FromQuery] bool? isEnabled = null,
        [FromQuery] bool includeDependencies = true,
        [FromQuery] bool includeSubModules = true,
        [FromQuery] bool includeAuditProperties = false)
    {
        try
        {
            var query = new GetModuleConfigurationsQuery
            {
                IsEnabled = isEnabled,
                IncludeDependencies = includeDependencies,
                IncludeSubModules = includeSubModules,
                IncludeAuditProperties = includeAuditProperties
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module configurations");
            return StatusCode(500, "An error occurred while retrieving module configurations");
        }
    }

    /// <summary>
    /// Get enabled module configurations only - accessible to all authenticated users for navigation
    /// </summary>
    [HttpGet("enabled")]
    public async Task<ActionResult<IEnumerable<ModuleConfigurationDto>>> GetEnabledModuleConfigurations()
    {
        try
        {
            var modules = await _moduleConfigurationService.GetEnabledModuleConfigurationsAsync();
            return Ok(modules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enabled module configurations");
            return StatusCode(500, "An error occurred while retrieving enabled module configurations");
        }
    }

    /// <summary>
    /// Get module configuration by module type
    /// </summary>
    [HttpGet("{moduleType}")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<ModuleConfigurationDto>> GetModuleConfiguration(
        ModuleType moduleType,
        [FromQuery] bool includeDependencies = true,
        [FromQuery] bool includeSubModules = true,
        [FromQuery] bool includeAuditProperties = true)
    {
        try
        {
            var query = new GetModuleConfigurationByIdQuery
            {
                ModuleType = moduleType,
                IncludeDependencies = includeDependencies,
                IncludeSubModules = includeSubModules,
                IncludeAuditProperties = includeAuditProperties
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound($"Module configuration for {moduleType} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module configuration for {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while retrieving the module configuration");
        }
    }

    /// <summary>
    /// Get module configuration dashboard with statistics and warnings
    /// </summary>
    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<ModuleConfigurationDashboardDto>> GetModuleConfigurationDashboard(
        [FromQuery] int recentActivityCount = 10)
    {
        try
        {
            var query = new GetModuleConfigurationDashboardQuery
            {
                RecentActivityCount = recentActivityCount
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module configuration dashboard");
            return StatusCode(500, "An error occurred while retrieving the dashboard");
        }
    }

    /// <summary>
    /// Enable a module
    /// </summary>
    [HttpPost("{moduleType}/enable")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    public async Task<ActionResult> EnableModule(ModuleType moduleType, [FromBody] EnableModuleRequest? request = null)
    {
        try
        {
            var command = new EnableModuleCommand
            {
                ModuleType = moduleType,
                Context = request?.Context
            };

            var result = await _mediator.Send(command);
            
            if (!result)
                return BadRequest("Failed to enable module. Module may not exist or is already enabled.");

            _logger.LogInformation("Module {ModuleType} enabled successfully", moduleType);
            return Ok(new { message = $"Module {moduleType} enabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while enabling the module");
        }
    }

    /// <summary>
    /// Disable a module
    /// </summary>
    [HttpPost("{moduleType}/disable")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    public async Task<ActionResult> DisableModule(ModuleType moduleType, [FromBody] DisableModuleRequest? request = null)
    {
        try
        {
            var command = new DisableModuleCommand
            {
                ModuleType = moduleType,
                Context = request?.Context,
                ForceDisable = request?.ForceDisable ?? false
            };

            var result = await _mediator.Send(command);
            
            if (!result)
                return BadRequest("Failed to disable module. Module may not exist, is already disabled, or has active dependencies.");

            _logger.LogInformation("Module {ModuleType} disabled successfully", moduleType);
            return Ok(new { message = $"Module {moduleType} disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while disabling the module");
        }
    }

    /// <summary>
    /// Check if a module can be disabled
    /// </summary>
    [HttpGet("{moduleType}/can-disable")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<CanDisableModuleResponse>> CanDisableModule(ModuleType moduleType)
    {
        try
        {
            var canDisable = await _moduleConfigurationService.CanModuleBeDisabledAsync(moduleType);
            var warnings = await _moduleConfigurationService.GetModuleDisableWarningsAsync(moduleType);

            return Ok(new CanDisableModuleResponse
            {
                CanDisable = canDisable,
                Warnings = warnings.ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if module {ModuleType} can be disabled", moduleType);
            return StatusCode(500, "An error occurred while checking module disable status");
        }
    }

    /// <summary>
    /// Update module settings
    /// </summary>
    [HttpPut("{moduleType}/settings")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    public async Task<ActionResult> UpdateModuleSettings(ModuleType moduleType, [FromBody] UpdateModuleSettingsRequest request)
    {
        try
        {
            var command = new UpdateModuleSettingsCommand
            {
                ModuleType = moduleType,
                Settings = request.Settings,
                Context = request.Context
            };

            var result = await _mediator.Send(command);
            
            if (!result)
                return NotFound($"Module {moduleType} not found");

            _logger.LogInformation("Settings updated for module {ModuleType}", moduleType);
            return Ok(new { message = $"Settings updated for module {moduleType}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings for module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while updating module settings");
        }
    }

    /// <summary>
    /// Get module settings
    /// </summary>
    [HttpGet("{moduleType}/settings")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<string?>> GetModuleSettings(ModuleType moduleType)
    {
        try
        {
            var settings = await _moduleConfigurationService.GetModuleSettingsAsync(moduleType);
            return Ok(new { settings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings for module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while retrieving module settings");
        }
    }

    /// <summary>
    /// Get module dependencies
    /// </summary>
    [HttpGet("{moduleType}/dependencies")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<ModuleDependencyDto>>> GetModuleDependencies(ModuleType moduleType)
    {
        try
        {
            var dependencies = await _moduleConfigurationService.GetModuleDependenciesAsync(moduleType);
            return Ok(dependencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dependencies for module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while retrieving module dependencies");
        }
    }

    /// <summary>
    /// Get modules that depend on this module
    /// </summary>
    [HttpGet("{moduleType}/dependents")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<ModuleDependencyDto>>> GetDependentModules(ModuleType moduleType)
    {
        try
        {
            var dependents = await _moduleConfigurationService.GetDependentModulesAsync(moduleType);
            return Ok(dependents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dependent modules for {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while retrieving dependent modules");
        }
    }

    /// <summary>
    /// Get module audit trail
    /// </summary>
    [HttpGet("{moduleType}/audit-trail")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<ModuleConfigurationAuditLogDto>>> GetModuleAuditTrail(ModuleType moduleType)
    {
        try
        {
            var auditTrail = await _moduleConfigurationService.GetModuleAuditTrailAsync(moduleType);
            return Ok(auditTrail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit trail for module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while retrieving module audit trail");
        }
    }

    /// <summary>
    /// Get recent module activity across all modules
    /// </summary>
    [HttpGet("recent-activity")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<ModuleConfigurationAuditLogDto>>> GetRecentModuleActivity(
        [FromQuery] int count = 10)
    {
        try
        {
            var recentActivity = await _moduleConfigurationService.GetRecentModuleActivityAsync(count);
            return Ok(recentActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent module activity");
            return StatusCode(500, "An error occurred while retrieving recent activity");
        }
    }

    /// <summary>
    /// Get module hierarchy (parent-child relationships)
    /// </summary>
    [HttpGet("hierarchy")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<IEnumerable<ModuleConfigurationDto>>> GetModuleHierarchy()
    {
        try
        {
            var hierarchy = await _moduleConfigurationService.GetModuleHierarchyAsync();
            return Ok(hierarchy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module hierarchy");
            return StatusCode(500, "An error occurred while retrieving module hierarchy");
        }
    }

    /// <summary>
    /// Validate module dependencies
    /// </summary>
    [HttpGet("{moduleType}/validate-dependencies")]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Read)]
    public async Task<ActionResult<bool>> ValidateModuleDependencies(ModuleType moduleType)
    {
        try
        {
            var isValid = await _moduleConfigurationService.ValidateModuleDependenciesAsync(moduleType);
            return Ok(new { isValid, moduleType });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating dependencies for module {ModuleType}", moduleType);
            return StatusCode(500, "An error occurred while validating module dependencies");
        }
    }
}

// Request/Response DTOs
public class EnableModuleRequest
{
    public string? Context { get; set; }
}

public class DisableModuleRequest
{
    public string? Context { get; set; }
    public bool ForceDisable { get; set; } = false;
}

public class UpdateModuleSettingsRequest
{
    public string? Settings { get; set; }
    public string? Context { get; set; }
}

public class CanDisableModuleResponse
{
    public bool CanDisable { get; set; }
    public List<string> Warnings { get; set; } = new();
}