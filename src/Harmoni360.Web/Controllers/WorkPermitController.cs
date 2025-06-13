using Harmoni360.Application.Features.WorkPermits.Commands;
using Harmoni360.Application.Features.WorkPermits.Queries;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers
{
    [ApiController]
    [Route("api/work-permits")]
    [Authorize]
    public class WorkPermitController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<WorkPermitController> _logger;

        public WorkPermitController(IMediator mediator, ILogger<WorkPermitController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new work permit
        /// </summary>
        [HttpPost]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Create)]
        public async Task<IActionResult> CreateWorkPermit([FromBody] CreateWorkPermitCommand command)
        {
            _logger.LogInformation("Creating new work permit with title: {Title}", command.Title);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Work permit created successfully with ID: {Id}", result.Id);
            return CreatedAtAction(nameof(GetWorkPermit), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves work permits with filtering and pagination
        /// </summary>
        [HttpGet]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> GetWorkPermits([FromQuery] GetWorkPermitsQuery query)
        {
            _logger.LogInformation("Retrieving work permits with filters: Type={Type}, Status={Status}", 
                query.Type, query.Status);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific work permit by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> GetWorkPermit(int id)
        {
            _logger.LogInformation("Retrieving work permit with ID: {Id}", id);
            
            var query = new GetWorkPermitByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                _logger.LogWarning("Work permit with ID {Id} not found", id);
                return NotFound();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing work permit
        /// </summary>
        [HttpPut("{id}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateWorkPermit(int id, [FromBody] UpdateWorkPermitCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating work permit with ID: {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a work permit
        /// </summary>
        [HttpDelete("{id}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Delete)]
        public async Task<IActionResult> DeleteWorkPermit(int id)
        {
            _logger.LogInformation("Deleting work permit with ID: {Id}", id);
            
            var command = new DeleteWorkPermitCommand { Id = id };
            await _mediator.Send(command);
            
            return NoContent();
        }

        /// <summary>
        /// Submits a work permit for approval
        /// </summary>
        [HttpPost("{id}/submit")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> SubmitWorkPermit(int id)
        {
            _logger.LogInformation("Submitting work permit {Id} for approval", id);
            
            var command = new SubmitWorkPermitCommand { Id = id };
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }

        /// <summary>
        /// Approves a work permit
        /// </summary>
        [HttpPost("{id}/approve")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Approve)]
        public async Task<IActionResult> ApproveWorkPermit(int id, [FromBody] ApproveWorkPermitCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Approving work permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Rejects a work permit
        /// </summary>
        [HttpPost("{id}/reject")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Approve)]
        public async Task<IActionResult> RejectWorkPermit(int id, [FromBody] RejectWorkPermitCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Rejecting work permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Starts approved work
        /// </summary>
        [HttpPost("{id}/start")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> StartWork(int id)
        {
            _logger.LogInformation("Starting work for permit {Id}", id);
            
            var command = new StartWorkCommand { Id = id };
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }

        /// <summary>
        /// Completes work permit
        /// </summary>
        [HttpPost("{id}/complete")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> CompleteWork(int id, [FromBody] CompleteWorkCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Completing work for permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Cancels a work permit
        /// </summary>
        [HttpPost("{id}/cancel")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> CancelWorkPermit(int id, [FromBody] CancelWorkPermitCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Cancelling work permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Adds a hazard to the work permit
        /// </summary>
        [HttpPost("{id}/hazards")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> AddHazard(int id, [FromBody] AddWorkPermitHazardCommand command)
        {
            if (id != command.WorkPermitId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Adding hazard to work permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates a hazard in the work permit
        /// </summary>
        [HttpPut("{id}/hazards/{hazardId}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateHazard(int id, int hazardId, [FromBody] UpdateWorkPermitHazardCommand command)
        {
            if (id != command.WorkPermitId || hazardId != command.HazardId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating hazard {HazardId} in work permit {Id}", hazardId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Removes a hazard from the work permit
        /// </summary>
        [HttpDelete("{id}/hazards/{hazardId}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> RemoveHazard(int id, int hazardId)
        {
            _logger.LogInformation("Removing hazard {HazardId} from work permit {Id}", hazardId, id);
            
            var command = new RemoveWorkPermitHazardCommand 
            { 
                WorkPermitId = id, 
                HazardId = hazardId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Adds a precaution to the work permit
        /// </summary>
        [HttpPost("{id}/precautions")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> AddPrecaution(int id, [FromBody] AddWorkPermitPrecautionCommand command)
        {
            if (id != command.WorkPermitId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Adding precaution to work permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates a precaution in the work permit
        /// </summary>
        [HttpPut("{id}/precautions/{precautionId}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdatePrecaution(int id, int precautionId, [FromBody] UpdateWorkPermitPrecautionCommand command)
        {
            if (id != command.WorkPermitId || precautionId != command.PrecautionId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating precaution {PrecautionId} in work permit {Id}", precautionId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Marks a precaution as complete
        /// </summary>
        [HttpPost("{id}/precautions/{precautionId}/complete")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> CompletePrecaution(int id, int precautionId, [FromBody] CompleteWorkPermitPrecautionCommand command)
        {
            if (id != command.WorkPermitId || precautionId != command.PrecautionId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Completing precaution {PrecautionId} in work permit {Id}", precautionId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Uploads an attachment to the work permit
        /// </summary>
        [HttpPost("{id}/attachments")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] UploadWorkPermitAttachmentCommand command)
        {
            if (id != command.WorkPermitId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Uploading attachment to work permit {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an attachment from the work permit
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Update)]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} from work permit {Id}", attachmentId, id);
            
            var command = new DeleteWorkPermitAttachmentCommand 
            { 
                WorkPermitId = id, 
                AttachmentId = attachmentId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Downloads an attachment from the work permit
        /// </summary>
        [HttpGet("{id}/attachments/{attachmentId}/download")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Downloading attachment {AttachmentId} from work permit {Id}", attachmentId, id);
            
            var query = new GetWorkPermitAttachmentQuery 
            { 
                WorkPermitId = id, 
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
        /// Gets work permit dashboard metrics
        /// </summary>
        [HttpGet("dashboard")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> GetDashboard()
        {
            _logger.LogInformation("Retrieving work permit dashboard metrics");
            
            var query = new GetWorkPermitDashboardQuery();
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }

        /// <summary>
        /// Gets user's work permits
        /// </summary>
        [HttpGet("my-permits")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> GetMyWorkPermits([FromQuery] GetMyWorkPermitsQuery query)
        {
            _logger.LogInformation("Retrieving user's work permits");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets work permits pending approval
        /// </summary>
        [HttpGet("pending-approval")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Approve)]
        public async Task<IActionResult> GetPendingApproval([FromQuery] GetPendingApprovalQuery query)
        {
            _logger.LogInformation("Retrieving work permits pending approval");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets overdue work permits
        /// </summary>
        [HttpGet("overdue")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> GetOverduePermits([FromQuery] GetOverdueWorkPermitsQuery query)
        {
            _logger.LogInformation("Retrieving overdue work permits");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets work permit statistics
        /// </summary>
        [HttpGet("statistics")]
        [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
        public async Task<IActionResult> GetStatistics([FromQuery] GetWorkPermitStatisticsQuery query)
        {
            _logger.LogInformation("Retrieving work permit statistics");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}