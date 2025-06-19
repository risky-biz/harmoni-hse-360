using Harmoni360.Application.Features.WorkPermitSettings.Commands;
using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using Harmoni360.Application.Features.WorkPermitSettings.Queries;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Services;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Harmoni360.Web.Controllers;

/// <summary>
/// Controller for managing Work Permit Settings and Safety Videos
/// </summary>
[ApiController]
[Route("api/work-permits/settings")]
[Authorize]
public class WorkPermitSettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WorkPermitSettingsController> _logger;
    private readonly IVideoValidationService _videoValidationService;
    private readonly IConfiguration _configuration;
    private readonly IFileStorageService _fileStorageService;

    public WorkPermitSettingsController(
        IMediator mediator, 
        ILogger<WorkPermitSettingsController> logger,
        IVideoValidationService videoValidationService,
        IConfiguration configuration,
        IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _logger = logger;
        _videoValidationService = videoValidationService;
        _configuration = configuration;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Get all Work Permit Settings
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(IEnumerable<WorkPermitSettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkPermitSettings([FromQuery] bool? isActive = true, [FromQuery] bool includeSafetyVideos = true)
    {
        try
        {
            var query = new GetWorkPermitSettingsQuery
            {
                IsActive = isActive,
                IncludeSafetyVideos = includeSafetyVideos
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Work Permit Settings");
            return BadRequest(new { message = "An error occurred while retrieving settings" });
        }
    }

    /// <summary>
    /// Get active Work Permit Setting
    /// </summary>
    [HttpGet("active")]
    [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(WorkPermitSettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveWorkPermitSetting([FromQuery] bool includeSafetyVideos = true)
    {
        try
        {
            var query = new GetActiveWorkPermitSettingQuery
            {
                IncludeSafetyVideos = includeSafetyVideos
            };

            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound(new { message = "No active Work Permit Setting found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active Work Permit Setting");
            return BadRequest(new { message = "An error occurred while retrieving the active setting" });
        }
    }

    /// <summary>
    /// Get Work Permit Setting by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(WorkPermitSettingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkPermitSetting(int id, [FromQuery] bool includeSafetyVideos = true)
    {
        try
        {
            var query = new GetWorkPermitSettingByIdQuery(id)
            {
                IncludeSafetyVideos = includeSafetyVideos
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Work Permit Setting with ID {SettingId}", id);
            return BadRequest(new { message = "An error occurred while retrieving the setting" });
        }
    }

    /// <summary>
    /// Create a new Work Permit Setting (SuperAdmin only)
    /// </summary>
    [HttpPost]
    [RequireSystemAdmin]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    [ProducesResponseType(typeof(WorkPermitSettingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorkPermitSetting([FromBody] CreateWorkPermitSettingCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetWorkPermitSetting), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Work Permit Setting");
            return BadRequest(new { message = "An error occurred while creating the setting" });
        }
    }

    /// <summary>
    /// Update an existing Work Permit Setting (SuperAdmin only)
    /// </summary>
    [HttpPut("{id}")]
    [RequireSystemAdmin]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkPermitSetting(int id, [FromBody] UpdateWorkPermitSettingCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "ID mismatch between route and request body" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            }

            await _mediator.Send(command);
            return Ok(new { message = "Work Permit Setting updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Work Permit Setting with ID {SettingId}", id);
            return BadRequest(new { message = "An error occurred while updating the setting" });
        }
    }

    /// <summary>
    /// Delete a Work Permit Setting (SuperAdmin only)
    /// </summary>
    [HttpDelete("{id}")]
    [RequireSystemAdmin]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkPermitSetting(int id)
    {
        try
        {
            var command = new DeleteWorkPermitSettingCommand(id);
            await _mediator.Send(command);
            return Ok(new { message = "Work Permit Setting deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Work Permit Setting with ID {SettingId}", id);
            return BadRequest(new { message = "An error occurred while deleting the setting" });
        }
    }

    /// <summary>
    /// Upload a safety induction video for Work Permit Settings (SuperAdmin only)
    /// </summary>
    [HttpPost("{settingId}/videos")]
    [RequireSystemAdmin]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    [ProducesResponseType(typeof(WorkPermitSafetyVideoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(104_857_600)] // 100MB limit
    public async Task<IActionResult> UploadSafetyVideo(
        int settingId,
        [FromForm] IFormFile videoFile,
        [FromForm] string? description = null,
        [FromForm] string? resolution = null,
        [FromForm] int? bitrate = null,
        [FromForm] bool setAsActive = true)
    {
        try
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return BadRequest(new { message = "No video file provided" });
            }

            // Validate video file using the validation service
            var validationResult = await _videoValidationService.ValidateVideoAsync(videoFile);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = "Video validation failed", errors = validationResult.Errors });
            }

            // Save the file using the file storage service
            FileUploadResult uploadResult;
            try
            {
                await using var videoStream = videoFile.OpenReadStream();
                uploadResult = await _fileStorageService.UploadAsync(videoStream, videoFile.FileName, videoFile.ContentType, "safety-videos");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save video file");
                return BadRequest(new { message = "Failed to save video file" });
            }

            try
            {
                // Use the extracted metadata from validation
                var metadata = validationResult.Metadata!;

                var command = new UploadSafetyVideoCommand
                {
                    WorkPermitSettingsId = settingId,
                    FileName = Path.GetFileName(uploadResult.FilePath),
                    OriginalFileName = videoFile.FileName,
                    FilePath = uploadResult.FilePath,
                    FileSize = uploadResult.Size,
                    ContentType = videoFile.ContentType,
                    Duration = metadata.Duration,
                    Description = description,
                    Resolution = resolution,
                    Bitrate = bitrate,
                    SetAsActive = setAsActive
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetWorkPermitSetting), new { id = settingId }, result);
            }
            catch (Exception ex)
            {
                // Clean up file if database save fails
                try
                {
                    await _fileStorageService.DeleteAsync(uploadResult.FilePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to delete file after upload failure: {FilePath}", uploadResult.FilePath);
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading safety video for Work Permit Setting {SettingId}", settingId);
            return BadRequest(new { message = "An error occurred while uploading the video" });
        }
    }

    /// <summary>
    /// Delete a safety video (SuperAdmin only)
    /// </summary>
    [HttpDelete("{settingsId}/videos/{videoId}")]
    [RequireSystemAdmin]
    [RequireModulePermission(ModuleType.ApplicationSettings, PermissionType.Configure)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSafetyVideo(int settingsId, int videoId)
    {
        try
        {
            var command = new DeleteSafetyVideoCommand(videoId);
            await _mediator.Send(command);
            return Ok(new { message = "Safety video deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting safety video with ID {VideoId}", videoId);
            return BadRequest(new { message = "An error occurred while deleting the video" });
        }
    }

    /// <summary>
    /// Stream/Download a safety video file
    /// </summary>
    [HttpGet("videos/{videoId}/stream")]
    [AllowAnonymous] // Allow anonymous access but validate token manually
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StreamVideo(int videoId, [FromQuery] string? token = null)
    {
        try
        {
            // Validate authentication - either from headers or query parameter
            if (!await IsValidTokenAsync(token))
            {
                return Unauthorized(new { message = "Valid authentication token required" });
            }

            var query = new GetSafetyVideoByIdQuery(videoId);
            var video = await _mediator.Send(query);
            
            if (video == null)
            {
                return NotFound(new { message = "Video not found" });
            }

            if (!await _fileStorageService.ExistsAsync(video.FilePath))
            {
                _logger.LogWarning("Video file not found at path: {FilePath}", video.FilePath);
                return NotFound(new { message = "Video file not found on disk" });
            }

            var fileStream = await _fileStorageService.DownloadAsync(video.FilePath);
            return File(fileStream, video.ContentType, video.OriginalFileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming video with ID {VideoId}", videoId);
            return BadRequest(new { message = "An error occurred while streaming the video" });
        }
    }

    private async Task<bool> IsValidTokenAsync(string? token)
    {
        try
        {
            // First check if user is already authenticated via headers
            if (User.Identity?.IsAuthenticated == true)
            {
                return true;
            }

            // If no token provided, not valid
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            // Validate the JWT token
            var jwtSecretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtSecretKey))
            {
                _logger.LogError("JWT Key not configured");
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return validatedToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    /// <summary>
    /// Get video metadata and thumbnail
    /// </summary>
    [HttpGet("videos/{videoId}")]
    [RequireModulePermission(ModuleType.WorkPermitManagement, PermissionType.Read)]
    [ProducesResponseType(typeof(WorkPermitSafetyVideoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVideoById(int videoId)
    {
        try
        {
            var query = new GetSafetyVideoByIdQuery(videoId);
            var video = await _mediator.Send(query);
            
            if (video == null)
            {
                return NotFound(new { message = "Video not found" });
            }

            return Ok(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving video with ID {VideoId}", videoId);
            return BadRequest(new { message = "An error occurred while retrieving the video" });
        }
    }
}