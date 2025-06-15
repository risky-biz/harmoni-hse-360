using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Queries;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(IMediator mediator, ILogger<StatisticsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [RequireModulePermission(ModuleType.Reporting, PermissionType.Read)]
    public async Task<ActionResult<HsseStatisticsDto>> GetStatistics([FromQuery] GetHsseStatisticsQuery query)
    {
        _logger.LogInformation("Retrieving HSSE statistics");
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("export")]
    [RequireModulePermission(ModuleType.Reporting, PermissionType.Read)]
    public async Task<IActionResult> ExportStatistics([FromQuery] ExportHsseStatisticsQuery query)
    {
        _logger.LogInformation("Exporting HSSE statistics");
        var result = await _mediator.Send(query);
        if (result.FileContent.Length == 0)
            return NoContent();
        return File(result.FileContent, result.ContentType, result.FileName);
    }

    [HttpGet("trends")]
    [RequireModulePermission(ModuleType.Reporting, PermissionType.Read)]
    public async Task<ActionResult<List<HsseTrendPointDto>>> GetTrends([FromQuery] GetHsseTrendQuery query)
    {
        _logger.LogInformation("Retrieving HSSE trend data");
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
