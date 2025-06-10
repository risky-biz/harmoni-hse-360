using Harmoni360.Application.Features.Configuration.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConfigurationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] bool? isActive = true)
    {
        var query = new GetDepartmentsQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("incident-categories")]
    public async Task<IActionResult> GetIncidentCategories([FromQuery] bool? isActive = true)
    {
        var query = new GetIncidentCategoriesQuery { IsActive = isActive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("incident-locations")]
    public async Task<IActionResult> GetIncidentLocations([FromQuery] bool? isActive = true, [FromQuery] string? building = null)
    {
        var query = new GetIncidentLocationsQuery { IsActive = isActive, Building = building };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}