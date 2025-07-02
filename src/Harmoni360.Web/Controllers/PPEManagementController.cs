using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.PPE.Commands;
using Harmoni360.Application.Features.PPE.DTOs;
using Harmoni360.Application.Features.PPE.Queries;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using Harmoni360.Web.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireModuleAccess(ModuleType.PPEManagement)]
public class PPEManagementController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHubContext<IncidentHub> _incidentHub;

    public PPEManagementController(
        IMediator mediator,
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHubContext<IncidentHub> incidentHub)
    {
        _mediator = mediator;
        _context = context;
        _currentUserService = currentUserService;
        _incidentHub = incidentHub;
    }

    #region PPE Categories Management

    [HttpGet("categories")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<List<PPECategoryManagementDto>>> GetCategories(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = _context.PPECategories.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search) ||
                c.Description.ToLower().Contains(search)
            );
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .Select(c => new PPECategoryManagementDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                Type = c.Type.ToString(),
                RequiresCertification = c.RequiresCertification,
                RequiresInspection = c.RequiresInspection,
                InspectionIntervalDays = c.InspectionIntervalDays,
                RequiresExpiry = c.RequiresExpiry,
                DefaultExpiryDays = c.DefaultExpiryDays,
                ComplianceStandard = c.ComplianceStandard,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                LastModifiedAt = c.LastModifiedAt,
                LastModifiedBy = c.LastModifiedBy,
                ItemCount = c.PPEItems.Count(i => i.CategoryId == c.Id)
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("categories")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult<PPECategoryManagementDto>> CreateCategory([FromBody] CreatePPECategoryRequest request)
    {
        // Check if code already exists
        var existingCategory = await _context.PPECategories
            .FirstOrDefaultAsync(c => c.Code == request.Code);

        if (existingCategory != null)
        {
            return BadRequest($"PPE Category with code '{request.Code}' already exists");
        }

        if (!Enum.TryParse<PPEType>(request.Type, out var ppeType))
        {
            return BadRequest($"Invalid PPE Type: {request.Type}");
        }

        var category = PPECategory.Create(
            request.Name,
            request.Code,
            request.Description,
            ppeType,
            _currentUserService.Email,
            request.RequiresCertification,
            request.RequiresInspection,
            request.InspectionIntervalDays,
            request.RequiresExpiry,
            request.DefaultExpiryDays,
            request.ComplianceStandard
        );

        _context.PPECategories.Add(category);
        await _context.SaveChangesAsync();

        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        var result = new PPECategoryManagementDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Type = category.Type.ToString(),
            RequiresCertification = category.RequiresCertification,
            RequiresInspection = category.RequiresInspection,
            InspectionIntervalDays = category.InspectionIntervalDays,
            RequiresExpiry = category.RequiresExpiry,
            DefaultExpiryDays = category.DefaultExpiryDays,
            ComplianceStandard = category.ComplianceStandard,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            LastModifiedAt = category.LastModifiedAt,
            LastModifiedBy = category.LastModifiedBy,
            ItemCount = 0
        };

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, result);
    }

    [HttpPut("categories/{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult<PPECategoryManagementDto>> UpdateCategory(int id, [FromBody] UpdatePPECategoryRequest request)
    {
        var category = await _context.PPECategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return NotFound($"PPE Category with ID {id} not found");
        }

        // Check if code already exists for a different category
        var existingCategory = await _context.PPECategories
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.Id != id);

        if (existingCategory != null)
        {
            return BadRequest($"PPE Category with code '{request.Code}' already exists");
        }

        if (!Enum.TryParse<PPEType>(request.Type, out var ppeType))
        {
            return BadRequest($"Invalid PPE Type: {request.Type}");
        }

        category.UpdateDetails(
            request.Name,
            request.Code,
            request.Description,
            ppeType,
            _currentUserService.Email,
            request.RequiresCertification,
            request.RequiresInspection,
            request.InspectionIntervalDays,
            request.RequiresExpiry,
            request.DefaultExpiryDays,
            request.ComplianceStandard
        );

        await _context.SaveChangesAsync();
        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        var result = new PPECategoryManagementDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Type = category.Type.ToString(),
            RequiresCertification = category.RequiresCertification,
            RequiresInspection = category.RequiresInspection,
            InspectionIntervalDays = category.InspectionIntervalDays,
            RequiresExpiry = category.RequiresExpiry,
            DefaultExpiryDays = category.DefaultExpiryDays,
            ComplianceStandard = category.ComplianceStandard,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            LastModifiedAt = category.LastModifiedAt,
            LastModifiedBy = category.LastModifiedBy,
            ItemCount = await _context.PPEItems.CountAsync(i => i.CategoryId == category.Id)
        };

        return Ok(result);
    }

    [HttpDelete("categories/{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        var category = await _context.PPECategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return NotFound($"PPE Category with ID {id} not found");
        }

        // Check if category has associated items
        var hasItems = await _context.PPEItems.AnyAsync(i => i.CategoryId == id);
        if (hasItems)
        {
            return BadRequest("Cannot delete category that has associated PPE items");
        }

        category.Deactivate(_currentUserService.Email);
        await _context.SaveChangesAsync();
        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        return NoContent();
    }

    #endregion

    #region PPE Sizes Management

    [HttpGet("sizes")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<List<PPESizeDto>>> GetSizes(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetPPESizesQuery
        {
            IsActive = isActive,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("sizes")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult<PPESizeDto>> CreateSize([FromBody] CreatePPESizeRequest request)
    {
        var command = new CreatePPESizeCommand
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            SortOrder = request.SortOrder
        };

        try
        {
            var result = await _mediator.Send(command);
            await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");
            return CreatedAtAction(nameof(GetSizes), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("sizes/{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult<PPESizeDto>> UpdateSize(int id, [FromBody] UpdatePPESizeRequest request)
    {
        var command = new UpdatePPESizeCommand
        {
            Id = id,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            SortOrder = request.SortOrder
        };

        try
        {
            var result = await _mediator.Send(command);
            await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("sizes/{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult> DeleteSize(int id)
    {
        var size = await _context.PPESizes.FirstOrDefaultAsync(s => s.Id == id);
        if (size == null)
        {
            return NotFound($"PPE Size with ID {id} not found");
        }

        // Check if size has associated items
        var hasItems = await _context.PPEItems.AnyAsync(i => i.SizeId == id);
        if (hasItems)
        {
            return BadRequest("Cannot delete size that has associated PPE items");
        }

        size.Deactivate(_currentUserService.Email);
        await _context.SaveChangesAsync();
        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        return NoContent();
    }

    #endregion

    #region PPE Storage Locations Management

    [HttpGet("storage-locations")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<List<PPEStorageLocationDto>>> GetStorageLocations(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null)
    {
        var query = _context.PPEStorageLocations.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(sl => sl.IsActive == isActive.Value);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(sl =>
                sl.Name.ToLower().Contains(search) ||
                sl.Code.ToLower().Contains(search) ||
                (sl.Description != null && sl.Description.ToLower().Contains(search))
            );
        }

        var locations = await query
            .OrderBy(sl => sl.Name)
            .Select(sl => new PPEStorageLocationDto
            {
                Id = sl.Id,
                Name = sl.Name,
                Code = sl.Code,
                Description = sl.Description,
                Address = sl.Address,
                ContactPerson = sl.ContactPerson,
                ContactPhone = sl.ContactPhone,
                IsActive = sl.IsActive,
                Capacity = sl.Capacity,
                CurrentStock = sl.CurrentStock,
                UtilizationPercentage = sl.UtilizationPercentage,
                CreatedAt = sl.CreatedAt,
                CreatedBy = sl.CreatedBy,
                LastModifiedAt = sl.LastModifiedAt,
                LastModifiedBy = sl.LastModifiedBy
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpPost("storage-locations")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult<PPEStorageLocationDto>> CreateStorageLocation([FromBody] CreatePPEStorageLocationRequest request)
    {
        // Check if code already exists
        var existingLocation = await _context.PPEStorageLocations
            .FirstOrDefaultAsync(sl => sl.Code == request.Code);

        if (existingLocation != null)
        {
            return BadRequest($"Storage Location with code '{request.Code}' already exists");
        }

        var location = PPEStorageLocation.Create(
            request.Name,
            request.Code,
            _currentUserService.Email,
            request.Description,
            request.Address,
            request.ContactPerson,
            request.ContactPhone,
            request.Capacity
        );

        _context.PPEStorageLocations.Add(location);
        await _context.SaveChangesAsync();
        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        var result = new PPEStorageLocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Code = location.Code,
            Description = location.Description,
            Address = location.Address,
            ContactPerson = location.ContactPerson,
            ContactPhone = location.ContactPhone,
            IsActive = location.IsActive,
            Capacity = location.Capacity,
            CurrentStock = location.CurrentStock,
            UtilizationPercentage = location.UtilizationPercentage,
            CreatedAt = location.CreatedAt,
            CreatedBy = location.CreatedBy,
            LastModifiedAt = location.LastModifiedAt,
            LastModifiedBy = location.LastModifiedBy
        };

        return CreatedAtAction(nameof(GetStorageLocations), new { id = location.Id }, result);
    }

    [HttpPut("storage-locations/{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult<PPEStorageLocationDto>> UpdateStorageLocation(int id, [FromBody] CreatePPEStorageLocationRequest request)
    {
        var location = await _context.PPEStorageLocations.FirstOrDefaultAsync(sl => sl.Id == id);
        if (location == null)
        {
            return NotFound($"Storage Location with ID {id} not found");
        }

        // Check if code already exists for another location
        var existingLocation = await _context.PPEStorageLocations
            .FirstOrDefaultAsync(sl => sl.Code == request.Code && sl.Id != id);

        if (existingLocation != null)
        {
            return BadRequest($"Storage Location with code '{request.Code}' already exists");
        }

        location.Update(
            request.Name,
            request.Code,
            _currentUserService.Email,
            request.Description,
            request.Address,
            request.ContactPerson,
            request.ContactPhone,
            request.Capacity
        );

        await _context.SaveChangesAsync();
        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        var result = new PPEStorageLocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Code = location.Code,
            Description = location.Description,
            Address = location.Address,
            ContactPerson = location.ContactPerson,
            ContactPhone = location.ContactPhone,
            IsActive = location.IsActive,
            Capacity = location.Capacity,
            CurrentStock = location.CurrentStock,
            UtilizationPercentage = location.UtilizationPercentage,
            CreatedAt = location.CreatedAt,
            CreatedBy = location.CreatedBy,
            LastModifiedAt = location.LastModifiedAt,
            LastModifiedBy = location.LastModifiedBy
        };

        return Ok(result);
    }

    [HttpDelete("storage-locations/{id}")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Configure)]
    public async Task<ActionResult> DeleteStorageLocation(int id)
    {
        var location = await _context.PPEStorageLocations.FirstOrDefaultAsync(sl => sl.Id == id);
        if (location == null)
        {
            return NotFound($"Storage Location with ID {id} not found");
        }

        // Check if location has associated items
        var hasItems = await _context.PPEItems.AnyAsync(i => i.StorageLocationId == id);
        if (hasItems)
        {
            return BadRequest("Cannot delete storage location that has associated PPE items");
        }

        location.Deactivate(_currentUserService.Email);
        await _context.SaveChangesAsync();
        await _incidentHub.Clients.All.SendAsync("PPEManagementUpdate");

        return NoContent();
    }

    [HttpGet("stats")]
    [RequireModulePermission(ModuleType.PPEManagement, PermissionType.Read)]
    public async Task<ActionResult<PPEManagementStatsDto>> GetManagementStats()
    {
        var stats = new PPEManagementStatsDto
        {
            TotalCategories = await _context.PPECategories.CountAsync(),
            ActiveCategories = await _context.PPECategories.CountAsync(c => c.IsActive),
            TotalSizes = await _context.PPESizes.CountAsync(),
            ActiveSizes = await _context.PPESizes.CountAsync(s => s.IsActive),
            TotalStorageLocations = await _context.PPEStorageLocations.CountAsync(),
            ActiveStorageLocations = await _context.PPEStorageLocations.CountAsync(sl => sl.IsActive),
            TotalPPEItems = await _context.PPEItems.CountAsync(),
            LastUpdated = DateTime.UtcNow
        };

        return Ok(stats);
    }

    #endregion
}

#region Request Models

public class CreatePPECategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool RequiresCertification { get; set; }
    public bool RequiresInspection { get; set; }
    public int? InspectionIntervalDays { get; set; }
    public bool RequiresExpiry { get; set; }
    public int? DefaultExpiryDays { get; set; }
    public string? ComplianceStandard { get; set; }
}

public class UpdatePPECategoryRequest : CreatePPECategoryRequest { }

public class CreatePPESizeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class UpdatePPESizeRequest : CreatePPESizeRequest { }

public class CreatePPEStorageLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public int Capacity { get; set; } = 1000;
}

#endregion