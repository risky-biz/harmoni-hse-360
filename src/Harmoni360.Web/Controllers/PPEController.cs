using Harmoni360.Application.Features.PPE.Commands;
using Harmoni360.Application.Features.PPE.DTOs;
using Harmoni360.Application.Features.PPE.Queries;
using Harmoni360.Web.Hubs;
using Harmoni360.Web.Authorization;
using Harmoni360.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Harmoni360.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PPEController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PPEController> _logger;
    private readonly IHubContext<IncidentHub> _incidentHub;

    public PPEController(IMediator mediator, ILogger<PPEController> logger, IHubContext<IncidentHub> incidentHub)
    {
        _mediator = mediator;
        _logger = logger;
        _incidentHub = incidentHub;
    }

    /// <summary>
    /// Get paginated list of PPE items with filtering and search
    /// </summary>
    [HttpGet]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<GetPPEItemsResponse>> GetPPEItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? condition = null,
        [FromQuery] string? location = null,
        [FromQuery] int? assignedToId = null,
        [FromQuery] bool? isExpired = null,
        [FromQuery] bool? isExpiringSoon = null,
        [FromQuery] bool? isMaintenanceDue = null,
        [FromQuery] bool? isInspectionDue = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc")
    {
        try
        {
            var query = new GetPPEItemsQuery
            {
                PageNumber = pageNumber,
                PageSize = Math.Min(pageSize, 100), // Cap at 100 items per page
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                Status = status,
                Condition = condition,
                Location = location,
                AssignedToId = assignedToId,
                IsExpired = isExpired,
                IsExpiringSoon = isExpiringSoon,
                IsMaintenanceDue = isMaintenanceDue,
                IsInspectionDue = isInspectionDue,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PPE items");
            return StatusCode(500, new { message = "An error occurred while retrieving PPE items" });
        }
    }

    /// <summary>
    /// Get PPE item by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<PPEItemDto>> GetPPEItem(int id)
    {
        try
        {
            var query = new GetPPEItemByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound(new { message = $"PPE item with ID {id} not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PPE item with ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the PPE item" });
        }
    }

    /// <summary>
    /// Create a new PPE item
    /// </summary>
    [HttpPost]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Create)]
    public async Task<ActionResult<PPEItemDto>> CreatePPEItem([FromBody] CreatePPEItemCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            // Notify all connected clients about the new PPE item
            await _incidentHub.Clients.All.SendAsync("PPEItemCreated", result.Id);
            
            // Notify dashboard should be updated
            await _incidentHub.Clients.All.SendAsync("DashboardUpdate");
            
            return CreatedAtAction(nameof(GetPPEItem), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating PPE item");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PPE item");
            return StatusCode(500, new { message = "An error occurred while creating the PPE item" });
        }
    }

    /// <summary>
    /// Update PPE item
    /// </summary>
    [HttpPut("{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Update)]
    public async Task<ActionResult<PPEItemDto>> UpdatePPEItem(int id, [FromBody] UpdatePPEItemCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in request body" });
            }

            var result = await _mediator.Send(command);
            
            // Notify all connected clients about the updated PPE item
            await _incidentHub.Clients.All.SendAsync("PPEItemUpdated", result.Id);
            
            // Notify dashboard should be updated
            await _incidentHub.Clients.All.SendAsync("DashboardUpdate");
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating PPE item {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PPE item {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the PPE item" });
        }
    }

    /// <summary>
    /// Delete PPE item
    /// </summary>
    [HttpDelete("{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Delete)]
    public async Task<ActionResult> DeletePPEItem(int id)
    {
        try
        {
            var command = new DeletePPEItemCommand { Id = id };
            await _mediator.Send(command);
            
            // Notify all connected clients about the deleted PPE item
            await _incidentHub.Clients.All.SendAsync("PPEItemDeleted", id);
            
            // Notify dashboard should be updated
            await _incidentHub.Clients.All.SendAsync("DashboardUpdate");
            
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when deleting PPE item {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting PPE item {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the PPE item" });
        }
    }

    /// <summary>
    /// Assign PPE item to user
    /// </summary>
    [HttpPost("{id}/assign")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Update)]
    public async Task<ActionResult<PPEAssignmentDto>> AssignPPEItem(int id, [FromBody] AssignPPECommand command)
    {
        try
        {
            if (id != command.PPEItemId)
            {
                return BadRequest(new { message = "ID in URL does not match PPE item ID in request body" });
            }

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when assigning PPE item {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when assigning PPE item {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning PPE item {Id}", id);
            return StatusCode(500, new { message = "An error occurred while assigning the PPE item" });
        }
    }

    /// <summary>
    /// Return assigned PPE item
    /// </summary>
    [HttpPost("{id}/return")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Update)]
    public async Task<ActionResult> ReturnPPEItem(int id, [FromBody] ReturnPPECommand command)
    {
        try
        {
            if (id != command.PPEItemId)
            {
                return BadRequest(new { message = "ID in URL does not match PPE item ID in request body" });
            }

            await _mediator.Send(command);
            return Ok(new { message = "PPE item returned successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when returning PPE item {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when returning PPE item {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning PPE item {Id}", id);
            return StatusCode(500, new { message = "An error occurred while returning the PPE item" });
        }
    }

    /// <summary>
    /// Mark PPE item as lost
    /// </summary>
    [HttpPost("{id}/lost")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Update)]
    public async Task<ActionResult> MarkPPEItemAsLost(int id, [FromBody] MarkPPEAsLostCommand command)
    {
        try
        {
            if (id != command.PPEItemId)
            {
                return BadRequest(new { message = "ID in URL does not match PPE item ID in request body" });
            }

            await _mediator.Send(command);
            return Ok(new { message = "PPE item marked as lost successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when marking PPE item {Id} as lost", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking PPE item {Id} as lost", id);
            return StatusCode(500, new { message = "An error occurred while marking the PPE item as lost" });
        }
    }

    /// <summary>
    /// Get PPE categories
    /// </summary>
    [HttpGet("categories")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<List<PPECategoryDto>>> GetPPECategories()
    {
        try
        {
            var query = new GetPPECategoriesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PPE categories");
            return StatusCode(500, new { message = "An error occurred while retrieving PPE categories" });
        }
    }

    /// <summary>
    /// Get PPE inventory summary/dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<PPEDashboardDto>> GetPPEDashboard()
    {
        try
        {
            var query = new GetPPEDashboardQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting PPE dashboard data");
            return StatusCode(500, new { message = "An error occurred while retrieving PPE dashboard data" });
        }
    }
}