using Harmoni360.Application.Features.Audits.Commands;
using Harmoni360.Application.Features.Audits.Queries;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers
{
    [ApiController]
    [Route("api/audits")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IMediator mediator, ILogger<AuditController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new audit
        /// </summary>
        [HttpPost]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Create)]
        public async Task<IActionResult> CreateAudit([FromBody] CreateAuditCommand command)
        {
            _logger.LogInformation("Creating new audit with title: {Title}", command.Title);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Audit created successfully with ID: {Id}", result.Id);
            return CreatedAtAction(nameof(GetAudit), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves audits with filtering and pagination
        /// </summary>
        [HttpGet]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetAudits([FromQuery] GetAuditsQuery query)
        {
            _logger.LogInformation("Retrieving audits with filters: Type={Type}, Status={Status}", 
                query.Type, query.Status);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific audit by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetAudit(int id)
        {
            _logger.LogInformation("Retrieving audit with ID: {Id}", id);
            
            var query = new GetAuditByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                _logger.LogWarning("Audit with ID {Id} not found", id);
                return NotFound();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing audit
        /// </summary>
        [HttpPut("{id}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateAudit(int id, [FromBody] UpdateAuditCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating audit with ID: {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an audit
        /// </summary>
        [HttpDelete("{id}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Delete)]
        public async Task<IActionResult> DeleteAudit(int id)
        {
            _logger.LogInformation("Deleting audit with ID: {Id}", id);
            
            var command = new DeleteAuditCommand { Id = id };
            await _mediator.Send(command);
            
            return NoContent();
        }

        /// <summary>
        /// Starts an audit
        /// </summary>
        [HttpPost("{id}/start")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> StartAudit(int id)
        {
            _logger.LogInformation("Starting audit {Id}", id);
            
            var command = new StartAuditCommand { Id = id };
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }

        /// <summary>
        /// Completes an audit
        /// </summary>
        [HttpPost("{id}/complete")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> CompleteAudit(int id, [FromBody] CompleteAuditCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Completing audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Cancels an audit
        /// </summary>
        [HttpPost("{id}/cancel")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> CancelAudit(int id, [FromBody] CancelAuditCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Cancelling audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Archives an audit
        /// </summary>
        [HttpPost("{id}/archive")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> ArchiveAudit(int id, [FromBody] ArchiveAuditCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Archiving audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // TODO: Implement audit item management commands
        /*
        /// <summary>
        /// Adds an item to the audit checklist
        /// </summary>
        [HttpPost("{id}/items")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> AddAuditItem(int id, [FromBody] AddAuditItemCommand command)
        {
            if (id != command.AuditId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Adding item to audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        */

        /*
        // TODO: Implement all advanced audit management commands and queries
        /// <summary>
        /// Updates an audit item
        /// </summary>
        [HttpPut("{id}/items/{itemId}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateAuditItem(int id, int itemId, [FromBody] UpdateAuditItemCommand command)
        {
            if (id != command.AuditId || itemId != command.ItemId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating item {ItemId} in audit {Id}", itemId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Removes an item from the audit
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> RemoveAuditItem(int id, int itemId)
        {
            _logger.LogInformation("Removing item {ItemId} from audit {Id}", itemId, id);
            
            var command = new RemoveAuditItemCommand 
            { 
                AuditId = id, 
                ItemId = itemId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Assesses an audit item
        /// </summary>
        [HttpPost("{id}/items/{itemId}/assess")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> AssessAuditItem(int id, int itemId, [FromBody] AssessAuditItemCommand command)
        {
            if (id != command.AuditId || itemId != command.ItemId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Assessing item {ItemId} in audit {Id}", itemId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Creates a finding for an audit
        /// </summary>
        [HttpPost("{id}/findings")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> CreateFinding(int id, [FromBody] CreateAuditFindingCommand command)
        {
            if (id != command.AuditId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Creating finding for audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates an audit finding
        /// </summary>
        [HttpPut("{id}/findings/{findingId}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateFinding(int id, int findingId, [FromBody] UpdateAuditFindingCommand command)
        {
            if (id != command.AuditId || findingId != command.FindingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating finding {FindingId} in audit {Id}", findingId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Closes an audit finding
        /// </summary>
        [HttpPost("{id}/findings/{findingId}/close")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> CloseFinding(int id, int findingId, [FromBody] CloseAuditFindingCommand command)
        {
            if (id != command.AuditId || findingId != command.FindingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Closing finding {FindingId} in audit {Id}", findingId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Verifies an audit finding closure
        /// </summary>
        [HttpPost("{id}/findings/{findingId}/verify")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> VerifyFinding(int id, int findingId, [FromBody] VerifyAuditFindingCommand command)
        {
            if (id != command.AuditId || findingId != command.FindingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Verifying finding {FindingId} in audit {Id}", findingId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Uploads an attachment to the audit
        /// </summary>
        [HttpPost("{id}/attachments")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] UploadAuditAttachmentCommand command)
        {
            if (id != command.AuditId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Uploading attachment to audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an attachment from the audit
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} from audit {Id}", attachmentId, id);
            
            var command = new DeleteAuditAttachmentCommand 
            { 
                AuditId = id, 
                AttachmentId = attachmentId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Downloads an attachment from the audit
        /// </summary>
        [HttpGet("{id}/attachments/{attachmentId}/download")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Downloading attachment {AttachmentId} from audit {Id}", attachmentId, id);
            
            var query = new GetAuditAttachmentQuery 
            { 
                AuditId = id, 
                AttachmentId = attachmentId 
            };
            
            var result = await _mediator.Send(query);
            
            if (result?.FileContent == null)
            {
                return NotFound();
            }
            
            return File(result.FileContent, result.ContentType, result.FileName);
        }

        /// <summary>
        /// Uploads an attachment to an audit finding
        /// </summary>
        [HttpPost("{id}/findings/{findingId}/attachments")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> UploadFindingAttachment(int id, int findingId, [FromForm] UploadFindingAttachmentCommand command)
        {
            if (id != command.AuditId || findingId != command.FindingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Uploading attachment to finding {FindingId} in audit {Id}", findingId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an attachment from an audit finding
        /// </summary>
        [HttpDelete("{id}/findings/{findingId}/attachments/{attachmentId}")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> DeleteFindingAttachment(int id, int findingId, int attachmentId)
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} from finding {FindingId} in audit {Id}", 
                attachmentId, findingId, id);
            
            var command = new DeleteFindingAttachmentCommand 
            { 
                AuditId = id,
                FindingId = findingId, 
                AttachmentId = attachmentId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Adds a comment to the audit
        /// </summary>
        [HttpPost("{id}/comments")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Update)]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddAuditCommentCommand command)
        {
            if (id != command.AuditId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Adding comment to audit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Gets audit dashboard metrics
        /// </summary>
        [HttpGet("dashboard")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetDashboard([FromQuery] GetAuditDashboardQuery query)
        {
            _logger.LogInformation("Retrieving audit dashboard metrics");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets user's audits
        /// </summary>
        [HttpGet("my-audits")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetMyAudits([FromQuery] GetMyAuditsQuery query)
        {
            _logger.LogInformation("Retrieving user's audits");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets audits pending action
        /// </summary>
        [HttpGet("pending")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetPendingAudits([FromQuery] GetPendingAuditsQuery query)
        {
            _logger.LogInformation("Retrieving audits pending action");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets overdue audits
        /// </summary>
        [HttpGet("overdue")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetOverdueAudits([FromQuery] GetOverdueAuditsQuery query)
        {
            _logger.LogInformation("Retrieving overdue audits");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets audit statistics
        /// </summary>
        [HttpGet("statistics")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetStatistics([FromQuery] GetAuditStatisticsQuery query)
        {
            _logger.LogInformation("Retrieving audit statistics");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets audit trail for an audit
        /// </summary>
        [HttpGet("{id}/audit-trail")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetAuditTrail(int id)
        {
            _logger.LogInformation("Retrieving audit trail for audit {Id}", id);
            
            var query = new GetAuditTrailQuery { AuditId = id };
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }

        /// <summary>
        /// Gets findings for an audit
        /// </summary>
        [HttpGet("{id}/findings")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> GetAuditFindings(int id, [FromQuery] GetAuditFindingsQuery query)
        {
            if (id != query.AuditId)
            {
                query = query with { AuditId = id };
            }

            _logger.LogInformation("Retrieving findings for audit {Id}", id);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        */

        // TODO: Implement export functionality
        /*
        /// <summary>
        /// Exports audit report
        /// </summary>
        [HttpGet("{id}/export")]
        [RequireModulePermission(ModuleType.AuditManagement, PermissionType.Read)]
        public async Task<IActionResult> ExportAuditReport(int id, [FromQuery] string format = "pdf")
        {
            _logger.LogInformation("Exporting audit report for audit {Id} in format {Format}", id, format);
            
            var command = new ExportAuditReportCommand 
            { 
                AuditId = id, 
                Format = format 
            };
            
            var result = await _mediator.Send(command);
            
            if (result?.FileContent == null)
            {
                return NotFound();
            }
            
            return File(result.FileContent, result.ContentType, result.FileName);
        }
        */
    }
}