using Harmoni360.Application.Features.WasteReports.Commands;
using Harmoni360.Application.Features.WasteReports.Queries;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Application.Common.Models;
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
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Create)]
    public async Task<ActionResult<WasteReportDto>> Create([FromForm] CreateWasteReportCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpGet]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<PagedList<WasteReportDto>>> GetAll(
        [FromQuery] WasteCategory? category = null,
        [FromQuery] WasteReportStatus? status = null,
        [FromQuery] WasteClassification? classification = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? location = null,
        [FromQuery] int? reporterId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetWasteReportsQuery(
            category, status, classification, fromDate, toDate, location, reporterId, 
            search, sortBy, sortDescending, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<WasteReportDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetWasteReportByIdQuery(id));
        return Ok(result);
    }

    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Update)]
    public async Task<ActionResult<WasteReportDto>> Update(int id, [FromBody] UpdateWasteReportCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteWasteReportCommand(id));
        return NoContent();
    }

    [HttpPut("{id}/status")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Update)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] WasteDisposalStatus status)
    {
        await _mediator.Send(new UpdateDisposalStatusCommand(id, status));
        return NoContent();
    }

    [HttpGet("my-reports")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<PagedList<WasteReportDto>>> GetMyReports(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId(); 
        var result = await _mediator.Send(new GetMyWasteReportsQuery(userId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<WasteDashboardDto>> GetDashboard()
    {
        var result = await _mediator.Send(new GetWasteDashboardQuery());
        return Ok(result);
    }

    [HttpPost("{id}/attachments")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Create)]
    public async Task<ActionResult<WasteAttachmentDto>> UploadAttachment(int id, [FromForm] IFormFile file)
    {
        var result = await _mediator.Send(new UploadWasteAttachmentCommand(id, file));
        return Ok(result);
    }

    [HttpGet("attachments/{attachmentId}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        var result = await _mediator.Send(new GetWasteAttachmentQuery(attachmentId));
        return File(result.FileData, result.ContentType, result.FileName);
    }

    [HttpDelete("attachments/{attachmentId}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Delete)]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
    {
        await _mediator.Send(new DeleteWasteAttachmentCommand(attachmentId));
        return NoContent();
    }

    [HttpPost("{id}/comments")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Create)]
    public async Task<ActionResult<WasteCommentDto>> AddComment(int id, [FromBody] AddWasteCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new AddWasteCommentCommand(id, request.Comment, request.Type, userId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}/comments")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Read)]
    public async Task<ActionResult<List<WasteCommentDto>>> GetComments(int id)
    {
        var result = await _mediator.Send(new GetWasteCommentsQuery(id));
        return Ok(result);
    }

    [HttpDelete("comments/{commentId}")]
    [RequireModulePermission(ModuleType.WasteManagement, PermissionType.Delete)]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        await _mediator.Send(new DeleteWasteCommentCommand(commentId));
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }
}

public class AddWasteCommentRequest
{
    public string Comment { get; set; } = string.Empty;
    public CommentType Type { get; set; }
}
