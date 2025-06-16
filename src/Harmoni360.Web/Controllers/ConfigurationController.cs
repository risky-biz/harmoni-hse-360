using Harmoni360.Application.Features.Configuration.Commands;
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

    // Department management endpoints
    [HttpPost("departments")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result });
    }

    [HttpPut("departments/{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return Ok();
    }

    [HttpDelete("departments/{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var command = new DeleteDepartmentCommand { Id = id };
        await _mediator.Send(command);
        return Ok();
    }

    // Company Configuration endpoints
    [HttpGet("company")]
    public async Task<IActionResult> GetCompanyConfiguration()
    {
        var query = new GetActiveCompanyConfigurationQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("company")]
    [Authorize(Roles = "Administrator,SystemAdministrator")]
    public async Task<IActionResult> UpdateCompanyConfiguration([FromBody] UpdateCompanyConfigurationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("company")]
    [Authorize(Roles = "Administrator,SystemAdministrator")]
    public async Task<IActionResult> CreateCompanyConfiguration([FromBody] CreateCompanyConfigurationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}