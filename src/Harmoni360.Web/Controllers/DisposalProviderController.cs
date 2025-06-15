using Harmoni360.Application.Features.DisposalProviders.Commands;
using Harmoni360.Application.Features.DisposalProviders.DTOs;
using Harmoni360.Application.Features.DisposalProviders.Queries;
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
    public async Task<ActionResult<List<DisposalProviderDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetDisposalProvidersQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DisposalProviderDto>> Create(CreateDisposalProviderCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
