using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Harmoni360.Application.Features.UserManagement.Commands;
using Harmoni360.Application.Features.UserManagement.Queries;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<UserController> logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of users with filtering and searching
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<UserListDto>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? department = null,
        [FromQuery] string? workLocation = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] UserStatus? status = null,
        [FromQuery] int? roleId = null,
        [FromQuery] bool? requiresMFA = null,
        [FromQuery] bool? isLocked = null,
        [FromQuery] DateTime? hiredAfter = null,
        [FromQuery] DateTime? hiredBefore = null,
        [FromQuery] string? supervisorEmployeeId = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var query = new GetUsersQuery
            {
                Page = page,
                PageSize = Math.Min(pageSize, 100), // Limit max page size
                SearchTerm = searchTerm,
                Department = department,
                WorkLocation = workLocation,
                IsActive = isActive,
                Status = status,
                RoleId = roleId,
                RequiresMFA = requiresMFA,
                IsLocked = isLocked,
                HiredAfter = hiredAfter,
                HiredBefore = hiredBefore,
                SupervisorEmployeeId = supervisorEmployeeId,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var query = new GetUserByIdQuery { UserId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Create)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var command = new CreateUserCommand
            {
                Email = createUserDto.Email,
                Name = createUserDto.Name,
                EmployeeId = createUserDto.EmployeeId,
                Department = createUserDto.Department,
                Position = createUserDto.Position,
                Password = createUserDto.Password,
                PhoneNumber = createUserDto.PhoneNumber,
                EmergencyContactName = createUserDto.EmergencyContactName,
                EmergencyContactPhone = createUserDto.EmergencyContactPhone,
                SupervisorEmployeeId = createUserDto.SupervisorEmployeeId,
                HireDate = createUserDto.HireDate,
                WorkLocation = createUserDto.WorkLocation,
                CostCenter = createUserDto.CostCenter,
                PreferredLanguage = createUserDto.PreferredLanguage,
                TimeZone = createUserDto.TimeZone,
                RoleIds = createUserDto.RoleIds
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User created successfully with ID: {UserId} by {CurrentUser}", 
                result.Id, _currentUserService.Email);

            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Update)]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var command = new UpdateUserCommand
            {
                UserId = id,
                Name = updateUserDto.Name,
                Department = updateUserDto.Department,
                Position = updateUserDto.Position,
                IsActive = updateUserDto.IsActive,
                Status = updateUserDto.Status,
                PhoneNumber = updateUserDto.PhoneNumber,
                EmergencyContactName = updateUserDto.EmergencyContactName,
                EmergencyContactPhone = updateUserDto.EmergencyContactPhone,
                SupervisorEmployeeId = updateUserDto.SupervisorEmployeeId,
                HireDate = updateUserDto.HireDate,
                WorkLocation = updateUserDto.WorkLocation,
                CostCenter = updateUserDto.CostCenter,
                PreferredLanguage = updateUserDto.PreferredLanguage,
                TimeZone = updateUserDto.TimeZone,
                RequiresMFA = updateUserDto.RequiresMFA,
                RoleIds = updateUserDto.RoleIds
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User updated successfully with ID: {UserId} by {CurrentUser}", 
                result.Id, _currentUserService.Email);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating user {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    /// <summary>
    /// Deactivate a user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Delete)]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = id };
            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("User deactivated successfully with ID: {UserId} by {CurrentUser}", 
                    id, _currentUserService.Email);
                return NoContent();
            }

            return BadRequest("Failed to deactivate user");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while deactivating user {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating user {UserId}", id);
            return StatusCode(500, "An error occurred while deactivating the user");
        }
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("{userId}/roles/{roleId}")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Assign)]
    public async Task<ActionResult> AssignRole(int userId, int roleId)
    {
        try
        {
            var command = new AssignRoleCommand { UserId = userId, RoleId = roleId };
            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("Role {RoleId} assigned to user {UserId} by {CurrentUser}", 
                    roleId, userId, _currentUserService.Email);
                return Ok(new { message = "Role assigned successfully" });
            }

            return BadRequest("Failed to assign role");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to assign role {RoleId} to user {UserId}", roleId, userId);
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while assigning role {RoleId} to user {UserId}", roleId, userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role {RoleId} to user {UserId}", roleId, userId);
            return StatusCode(500, "An error occurred while assigning the role");
        }
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpDelete("{userId}/roles/{roleId}")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Assign)]
    public async Task<ActionResult> RemoveRole(int userId, int roleId)
    {
        try
        {
            var command = new RemoveRoleCommand { UserId = userId, RoleId = roleId };
            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("Role {RoleId} removed from user {UserId} by {CurrentUser}", 
                    roleId, userId, _currentUserService.Email);
                return Ok(new { message = "Role removed successfully" });
            }

            return BadRequest("Failed to remove role");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to remove role {RoleId} from user {UserId}", roleId, userId);
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while removing role {RoleId} from user {UserId}", roleId, userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing role {RoleId} from user {UserId}", roleId, userId);
            return StatusCode(500, "An error occurred while removing the role");
        }
    }

    /// <summary>
    /// Change user status (activate, deactivate, suspend, terminate)
    /// </summary>
    [HttpPatch("{id}/status")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Update)]
    public async Task<ActionResult<UserDto>> ChangeUserStatus(int id, [FromBody] ChangeUserStatusRequest request)
    {
        try
        {
            var command = new ChangeUserStatusCommand
            {
                UserId = id,
                Status = request.Status,
                Reason = request.Reason
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User status changed successfully for ID: {UserId} to {Status} by {CurrentUser}", 
                id, request.Status, _currentUserService.Email);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while changing status for user {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing status for user {UserId}", id);
            return StatusCode(500, "An error occurred while changing user status");
        }
    }

    /// <summary>
    /// Unlock a user account
    /// </summary>
    [HttpPost("{id}/unlock")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Update)]
    public async Task<ActionResult<UserDto>> UnlockUserAccount(int id, [FromBody] UnlockAccountRequest request)
    {
        try
        {
            var command = new UnlockUserAccountCommand
            {
                UserId = id,
                Reason = request.Reason
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("User account unlocked successfully for ID: {UserId} by {CurrentUser}", 
                id, _currentUserService.Email);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while unlocking user {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while unlocking user {UserId}", id);
            return StatusCode(500, "An error occurred while unlocking user account");
        }
    }

    /// <summary>
    /// Reset user password
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Update)]
    public async Task<ActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            var command = new ResetPasswordCommand
            {
                UserId = id,
                NewPassword = request.NewPassword,
                RequirePasswordChange = request.RequirePasswordChange
            };

            var result = await _mediator.Send(command);
            
            if (result)
            {
                _logger.LogInformation("Password reset successfully for user ID: {UserId} by {CurrentUser}", 
                    id, _currentUserService.Email);
                return Ok(new { message = "Password reset successfully" });
            }

            return BadRequest("Failed to reset password");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while resetting password for user {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting password for user {UserId}", id);
            return StatusCode(500, "An error occurred while resetting password");
        }
    }

    /// <summary>
    /// Get user activity logs
    /// </summary>
    [HttpGet("{id}/activity")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<List<UserActivityLogDto>>> GetUserActivity(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? activityType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = new GetUserActivityQuery
            {
                UserId = id,
                Page = page,
                PageSize = Math.Min(pageSize, 100),
                ActivityType = activityType,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting activity for user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving user activity");
        }
    }

    /// <summary>
    /// Get user management statistics
    /// </summary>
    [HttpGet("statistics")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
    {
        try
        {
            var query = new GetUserStatisticsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user statistics");
            return StatusCode(500, "An error occurred while retrieving user statistics");
        }
    }

    /// <summary>
    /// Get all available roles
    /// </summary>
    [HttpGet("roles")]
    [RequireModulePermission(ModuleType.UserManagement, PermissionType.Read)]
    public async Task<ActionResult<List<RoleDto>>> GetRoles([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new GetRolesQuery { IncludeInactive = includeInactive };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting roles");
            return StatusCode(500, "An error occurred while retrieving roles");
        }
    }
}

// Request DTOs for new endpoints
public class ChangeUserStatusRequest
{
    public UserStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class UnlockAccountRequest
{
    public string? Reason { get; set; }
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    public bool RequirePasswordChange { get; set; } = true;
}