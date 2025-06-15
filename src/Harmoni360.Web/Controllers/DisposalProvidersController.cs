using Harmoni360.Application.Features.DisposalProviders.Commands;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using Harmoni360.Application.Features.DisposalProviders.Queries;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/disposal-providers")]
[Authorize]
public class DisposalProvidersController : ControllerBase
{
    private readonly IMediator _mediator;

    public DisposalProvidersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all disposal providers
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<List<DisposalProviderDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetDisposalProvidersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get disposal provider by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<DisposalProviderDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetDisposalProviderByIdQuery(id));
        if (result == null)
        {
            return NotFound($"Disposal provider with ID {id} not found");
        }
        return Ok(result);
    }

    /// <summary>
    /// Search disposal providers with filters
    /// </summary>
    [HttpGet("search")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<List<DisposalProviderDto>>> Search(
        [FromQuery] string? searchTerm = null,
        [FromQuery] ProviderStatus? status = null,
        [FromQuery] bool? includeInactive = false,
        [FromQuery] bool? onlyExpiring = false,
        [FromQuery] int? expiringDays = 30)
    {
        var query = new SearchDisposalProvidersQuery(searchTerm, status, includeInactive, onlyExpiring, expiringDays);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get expiring disposal providers
    /// </summary>
    [HttpGet("expiring")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<List<DisposalProviderDto>>> GetExpiring([FromQuery] int daysAhead = 30)
    {
        var result = await _mediator.Send(new GetExpiringProvidersQuery(daysAhead));
        return Ok(result);
    }

    /// <summary>
    /// Create new disposal provider
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Create)]
    public async Task<ActionResult<DisposalProviderDto>> Create([FromBody] CreateDisposalProviderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update disposal provider
    /// </summary>
    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Update)]
    public async Task<ActionResult<DisposalProviderDto>> Update(int id, [FromBody] UpdateDisposalProviderRequest request)
    {
        var command = new UpdateDisposalProviderCommand(id, request.Name, request.LicenseNumber, request.LicenseExpiryDate);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Change disposal provider status
    /// </summary>
    [HttpPatch("{id}/status")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Update)]
    public async Task<ActionResult<DisposalProviderDto>> ChangeStatus(int id, [FromBody] ChangeProviderStatusRequest request)
    {
        var command = new ChangeProviderStatusCommand(id, request.Status);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete disposal provider
    /// </summary>
    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Delete)]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteDisposalProviderCommand(id));
        return NoContent();
    }
}

// Request DTOs
public class UpdateDisposalProviderRequest
{
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
}

public class ChangeProviderStatusRequest
{
    public ProviderStatus Status { get; set; }
}