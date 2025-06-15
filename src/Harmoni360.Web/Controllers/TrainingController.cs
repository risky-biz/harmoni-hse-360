using Harmoni360.Application.Features.Trainings.Commands;
using Harmoni360.Application.Features.Trainings.Queries;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harmoni360.Web.Controllers
{
    [ApiController]
    [Route("api/trainings")]
    [Authorize]
    public class TrainingController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TrainingController> _logger;

        public TrainingController(IMediator mediator, ILogger<TrainingController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new training program
        /// </summary>
        [HttpPost]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Create)]
        public async Task<IActionResult> CreateTraining([FromBody] CreateTrainingCommand command)
        {
            _logger.LogInformation("Creating new training with title: {Title}", command.Title);
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Training created successfully with ID: {Id}", result.Id);
            return CreatedAtAction(nameof(GetTraining), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves trainings with filtering and pagination
        /// </summary>
        [HttpGet]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetTrainings([FromQuery] GetTrainingsQuery query)
        {
            _logger.LogInformation("Retrieving trainings with filters: Type={Type}, Category={Category}, Status={Status}", 
                query.Type, query.Category, query.Status);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific training by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetTraining(int id)
        {
            _logger.LogInformation("Retrieving training with ID: {Id}", id);
            
            var query = new GetTrainingByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                _logger.LogWarning("Training with ID {Id} not found", id);
                return NotFound();
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing training
        /// </summary>
        [HttpPut("{id}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateTraining(int id, [FromBody] UpdateTrainingCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating training with ID: {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a training
        /// </summary>
        [HttpDelete("{id}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Delete)]
        public async Task<IActionResult> DeleteTraining(int id)
        {
            _logger.LogInformation("Deleting training with ID: {Id}", id);
            
            var command = new DeleteTrainingCommand { Id = id };
            await _mediator.Send(command);
            
            return NoContent();
        }

        /// <summary>
        /// Enrolls a participant in training
        /// </summary>
        [HttpPost("{id}/participants")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> EnrollParticipant(int id, [FromBody] EnrollParticipantCommand command)
        {
            if (id != command.TrainingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Enrolling participant in training {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Withdraws a participant from training
        /// </summary>
        [HttpDelete("{id}/participants/{participantId}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> WithdrawParticipant(int id, int participantId)
        {
            _logger.LogInformation("Withdrawing participant {ParticipantId} from training {Id}", participantId, id);
            
            var command = new WithdrawParticipantCommand 
            { 
                TrainingId = id, 
                ParticipantId = participantId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Marks participant attendance
        /// </summary>
        [HttpPost("{id}/participants/{participantId}/attendance")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> MarkAttendance(int id, int participantId, [FromBody] MarkAttendanceCommand command)
        {
            if (id != command.TrainingId || participantId != command.ParticipantId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Marking attendance for participant {ParticipantId} in training {Id}", participantId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Records participant assessment score
        /// </summary>
        [HttpPost("{id}/participants/{participantId}/assessment")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> RecordAssessment(int id, int participantId, [FromBody] RecordAssessmentCommand command)
        {
            if (id != command.TrainingId || participantId != command.ParticipantId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Recording assessment for participant {ParticipantId} in training {Id}", participantId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Issues certification to participant
        /// </summary>
        [HttpPost("{id}/participants/{participantId}/certification")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Approve)]
        public async Task<IActionResult> IssueCertification(int id, int participantId, [FromBody] IssueCertificationCommand command)
        {
            if (id != command.TrainingId || participantId != command.ParticipantId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Issuing certification for participant {ParticipantId} in training {Id}", participantId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Adds a training requirement
        /// </summary>
        [HttpPost("{id}/requirements")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> AddRequirement(int id, [FromBody] AddTrainingRequirementCommand command)
        {
            if (id != command.TrainingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Adding requirement to training {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Updates a training requirement
        /// </summary>
        [HttpPut("{id}/requirements/{requirementId}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> UpdateRequirement(int id, int requirementId, [FromBody] UpdateTrainingRequirementCommand command)
        {
            if (id != command.TrainingId || requirementId != command.RequirementId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating requirement {RequirementId} in training {Id}", requirementId, id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Removes a training requirement
        /// </summary>
        [HttpDelete("{id}/requirements/{requirementId}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> RemoveRequirement(int id, int requirementId)
        {
            _logger.LogInformation("Removing requirement {RequirementId} from training {Id}", requirementId, id);
            
            var command = new RemoveTrainingRequirementCommand 
            { 
                TrainingId = id, 
                RequirementId = requirementId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Adds a comment to the training
        /// </summary>
        [HttpPost("{id}/comments")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> AddComment(int id, [FromBody] AddTrainingCommentCommand command)
        {
            if (id != command.TrainingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Adding comment to training {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Uploads an attachment to the training
        /// </summary>
        [HttpPost("{id}/attachments")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> UploadAttachment(int id, [FromForm] UploadTrainingAttachmentCommand command)
        {
            if (id != command.TrainingId)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Uploading attachment to training {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an attachment from the training
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Deleting attachment {AttachmentId} from training {Id}", attachmentId, id);
            
            var command = new DeleteTrainingAttachmentCommand 
            { 
                TrainingId = id, 
                AttachmentId = attachmentId 
            };
            
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Downloads an attachment from the training
        /// </summary>
        [HttpGet("{id}/attachments/{attachmentId}/download")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            _logger.LogInformation("Downloading attachment {AttachmentId} from training {Id}", attachmentId, id);
            
            var query = new GetTrainingAttachmentQuery 
            { 
                TrainingId = id, 
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
        /// Starts a training program
        /// </summary>
        [HttpPost("{id}/start")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> StartTraining(int id)
        {
            _logger.LogInformation("Starting training {Id}", id);
            
            var command = new StartTrainingCommand { Id = id };
            var result = await _mediator.Send(command);
            
            return Ok(result);
        }

        /// <summary>
        /// Completes a training program
        /// </summary>
        [HttpPost("{id}/complete")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> CompleteTraining(int id, [FromBody] CompleteTrainingCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Completing training {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Cancels a training program
        /// </summary>
        [HttpPost("{id}/cancel")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Update)]
        public async Task<IActionResult> CancelTraining(int id, [FromBody] CancelTrainingCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Cancelling training {Id}", id);
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Gets training dashboard metrics
        /// </summary>
        [HttpGet("dashboard")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetDashboard()
        {
            _logger.LogInformation("Retrieving training dashboard metrics");
            
            var query = new GetTrainingDashboardQuery();
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }

        /// <summary>
        /// Gets user's trainings
        /// </summary>
        [HttpGet("my-trainings")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetMyTrainings([FromQuery] GetMyTrainingsQuery query)
        {
            _logger.LogInformation("Retrieving user's trainings");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets upcoming trainings
        /// </summary>
        [HttpGet("upcoming")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetUpcomingTrainings([FromQuery] GetUpcomingTrainingsQuery query)
        {
            _logger.LogInformation("Retrieving upcoming trainings");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets training statistics
        /// </summary>
        [HttpGet("statistics")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetStatistics([FromQuery] GetTrainingStatisticsQuery query)
        {
            _logger.LogInformation("Retrieving training statistics");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Gets certifications for a participant
        /// </summary>
        [HttpGet("certifications")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> GetCertifications([FromQuery] GetCertificationsQuery query)
        {
            _logger.LogInformation("Retrieving certifications");
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Downloads a digital certificate
        /// </summary>
        [HttpGet("certifications/{certificationId}/download")]
        [RequireModulePermission(ModuleType.TrainingManagement, PermissionType.Read)]
        public async Task<IActionResult> DownloadCertificate(int certificationId)
        {
            _logger.LogInformation("Downloading certificate {CertificationId}", certificationId);
            
            var query = new GetCertificateDownloadQuery { CertificationId = certificationId };
            var result = await _mediator.Send(query);
            
            if (result?.FileContent == null)
            {
                return NotFound();
            }
            
            return File(result.FileContent, "application/pdf", result.FileName);
        }
    }
}