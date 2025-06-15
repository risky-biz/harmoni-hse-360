using Harmoni360.Application.Features.WasteReports.Commands;
using Harmoni360.Application.Features.WasteReports.Queries;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Harmoni360.Domain.Entities.Waste;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WasteReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public WasteReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Create)]
    public async Task<ActionResult<WasteReportDto>> Create([FromForm] CreateWasteReportCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpGet]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Read)]
    public async Task<ActionResult<List<WasteReportDto>>> GetAll([FromQuery] WasteCategory? category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetWasteReportsQuery(category, search, page, pageSize));
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [RequireModulePermission(ModuleType.IncidentManagement, PermissionType.Update)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] WasteDisposalStatus status)
    {
        await _mediator.Send(new UpdateDisposalStatusCommand(id, status));
        return NoContent();
    }
}
