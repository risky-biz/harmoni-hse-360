using Harmoni360.Application.Features.Licenses.Commands;
using Harmoni360.Application.Features.Licenses.Queries;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers
{
    [ApiController]
    [Route("api/licenses")]
    [Authorize]
    public class LicenseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LicenseController> _logger;

        public LicenseController(IMediator mediator, ILogger<LicenseController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new license
        /// </summary>
        [HttpPost]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Create)]
        public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseCommand command)
        {
            _logger.LogInformation("Creating new license with title: {Title}", command.Title);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("License created successfully with ID: {Id}", result.Id);
            return CreatedAtAction(nameof(GetLicense), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves licenses with filtering and pagination
        /// </summary>
        [HttpGet]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Read)]
        public async Task<IActionResult> GetLicenses([FromQuery] GetLicensesQuery query)
        {
            _logger.LogInformation("Retrieving licenses with filters: Type={Type}, Status={Status}", 
                query.Type, query.Status);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific license by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Read)]
        public async Task<IActionResult> GetLicense(int id)
        {
            _logger.LogInformation("Retrieving license with ID: {Id}", id);
            
            var query = new GetLicenseByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                _logger.LogWarning("License with ID {Id} not found", id);
                return NotFound();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing license
        /// </summary>
        [HttpPut("{id}")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateLicense(int id, [FromBody] UpdateLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating license with ID: {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a license
        /// </summary>
        [HttpDelete("{id}")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Delete)]
        public async Task<IActionResult> DeleteLicense(int id)
        {
            _logger.LogInformation("Deleting license with ID: {Id}", id);
            
            var command = new DeleteLicenseCommand { Id = id };
            await _mediator.Send(command);
            
            return NoContent();
        }

        /// <summary>
        /// Submits a license for approval
        /// </summary>
        [HttpPost("{id}/submit")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> SubmitLicense(int id, [FromBody] SubmitLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Submitting license {Id} for approval", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Approves a license
        /// </summary>
        [HttpPost("{id}/approve")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Approve)]
        public async Task<IActionResult> ApproveLicense(int id, [FromBody] ApproveLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Approving license {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Rejects a license
        /// </summary>
        [HttpPost("{id}/reject")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Approve)]
        public async Task<IActionResult> RejectLicense(int id, [FromBody] RejectLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Rejecting license {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Activates a license
        /// </summary>
        [HttpPost("{id}/activate")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> ActivateLicense(int id, [FromBody] ActivateLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Activating license {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Suspends a license
        /// </summary>
        [HttpPost("{id}/suspend")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> SuspendLicense(int id, [FromBody] SuspendLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Suspending license {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Revokes a license
        /// </summary>
        [HttpPost("{id}/revoke")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Delete)]
        public async Task<IActionResult> RevokeLicense(int id, [FromBody] RevokeLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Revoking license {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Initiates license renewal
        /// </summary>
        [HttpPost("{id}/renew")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> RenewLicense(int id, [FromBody] RenewLicenseCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Initiating renewal for license {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves my licenses (user-specific)
        /// </summary>
        [HttpGet("my-licenses")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Read)]
        public async Task<IActionResult> GetMyLicenses([FromQuery] GetMyLicensesQuery query)
        {
            _logger.LogInformation("Retrieving user's licenses");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves license dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Read)]
        public async Task<IActionResult> GetLicenseDashboard([FromQuery] GetLicenseDashboardQuery query)
        {
            _logger.LogInformation("Retrieving license dashboard data");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves expiring licenses
        /// </summary>
        [HttpGet("expiring")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Read)]
        public async Task<IActionResult> GetExpiringLicenses([FromQuery] GetExpiringLicensesQuery query)
        {
            _logger.LogInformation("Retrieving expiring licenses");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Uploads an attachment to a license
        /// </summary>
        [HttpPost("{id}/attachments")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> UploadLicenseAttachment(int id, [FromForm] UploadLicenseAttachmentCommand command)
        {
            if (id != command.LicenseId)
            {
                return BadRequest("License ID mismatch");
            }

            _logger.LogInformation("Uploading attachment to license {LicenseId}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Downloads a license attachment
        /// </summary>
        [HttpGet("{id}/attachments/{attachmentId}")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Read)]
        public async Task<IActionResult> DownloadLicenseAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Downloading attachment {AttachmentId} from license {LicenseId}", attachmentId, id);
            
            var query = new GetLicenseAttachmentQuery { LicenseId = id, AttachmentId = attachmentId };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound();
            }
            
            return File(result.Content, result.ContentType, result.FileName);
        }

        /// <summary>
        /// Deletes a license attachment
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> DeleteLicenseAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} from license {LicenseId}", attachmentId, id);
            
            var command = new DeleteLicenseAttachmentCommand { LicenseId = id, AttachmentId = attachmentId };
            await _mediator.Send(command);
            
            return NoContent();
        }

        /// <summary>
        /// Adds a condition to a license
        /// </summary>
        [HttpPost("{id}/conditions")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> AddLicenseCondition(int id, [FromBody] AddLicenseConditionCommand command)
        {
            if (id != command.LicenseId)
            {
                return BadRequest("License ID mismatch");
            }

            _logger.LogInformation("Adding condition to license {LicenseId}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates a license condition
        /// </summary>
        [HttpPut("{id}/conditions/{conditionId}")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateLicenseCondition(int id, int conditionId, [FromBody] UpdateLicenseConditionCommand command)
        {
            if (id != command.LicenseId || conditionId != command.ConditionId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating condition {ConditionId} for license {LicenseId}", conditionId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Completes a license condition
        /// </summary>
        [HttpPost("{id}/conditions/{conditionId}/complete")]
        [RequireModulePermission(ModuleType.LicenseManagement, PermissionType.Update)]
        public async Task<IActionResult> CompleteLicenseCondition(int id, int conditionId, [FromBody] CompleteLicenseConditionCommand command)
        {
            if (id != command.LicenseId || conditionId != command.ConditionId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Completing condition {ConditionId} for license {LicenseId}", conditionId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}