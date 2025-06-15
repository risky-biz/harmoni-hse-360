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
[Route("api/waste/disposal-providers")]
[Authorize]
public class DisposalProviderController : ControllerBase
{
    private readonly IMediator _mediator;
    public DisposalProviderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<List<DisposalProviderDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetDisposalProvidersQuery());
        return Ok(result);
    }

    [HttpPost]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Create)]
    public async Task<ActionResult<DisposalProviderDto>> Create(CreateDisposalProviderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
