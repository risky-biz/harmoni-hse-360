using Harmoni360.Application.Features.CorrectiveActions.Commands;
using Harmoni360.Application.Features.CorrectiveActions.Queries;
using Harmoni360.Application.Features.Incidents.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/incident/{incidentId}/corrective-actions")]
[RequireModuleAccess(ModuleType.IncidentManagement)]
public class CorrectiveActionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CorrectiveActionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<List<CorrectiveActionDto>>> GetCorrectiveActions(int incidentId)
    {
        var query = new GetCorrectiveActionsByIncidentQuery(incidentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Create)]
    public async Task<ActionResult<int>> CreateCorrectiveAction(
        int incidentId,
        [FromBody] CreateCorrectiveActionRequest request)
    {
        var command = new CreateCorrectiveActionCommand
        {
            IncidentId = incidentId,
            Description = request.Description,
            AssignedToDepartment = request.AssignedToDepartment,
            AssignedToId = request.AssignedToId,
            DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc),
            Priority = Enum.Parse<ActionPriority>(request.Priority)
        };

        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCorrectiveActions), new { incidentId }, new { id });
    }

    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    public async Task<ActionResult> UpdateCorrectiveAction(
        int incidentId,
        int id,
        [FromBody] UpdateCorrectiveActionRequest request)
    {
        var command = new UpdateCorrectiveActionCommand
        {
            Id = id,
            Description = request.Description,
            AssignedToDepartment = request.AssignedToDepartment,
            AssignedToId = request.AssignedToId,
            DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc),
            Priority = Enum.Parse<ActionPriority>(request.Priority),
            Status = Enum.Parse<ActionStatus>(request.Status),
            CompletionNotes = request.CompletionNotes
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Delete)]
    public async Task<ActionResult> DeleteCorrectiveAction(int incidentId, int id)
    {
        var command = new DeleteCorrectiveActionCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}

// Request DTOs
public class CreateCorrectiveActionRequest
{
    public string Description { get; set; } = string.Empty;
    public string AssignedToDepartment { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = string.Empty;
}

public class UpdateCorrectiveActionRequest
{
    public string Description { get; set; } = string.Empty;
    public string AssignedToDepartment { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
}