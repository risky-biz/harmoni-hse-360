using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Harmoni360.Domain.Entities;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/configuration")]
[Authorize]
public class HazardConfigurationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HazardConfigurationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hazard Categories endpoints
    [HttpGet("hazard-categories")]
    public async Task<ActionResult<IEnumerable<HazardCategoryDto>>> GetHazardCategories()
    {
        var categories = await _context.Set<HazardCategory>()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new HazardCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description,
                Color = c.Color,
                RiskLevel = c.RiskLevel,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = c.LastModifiedAt.HasValue ? c.LastModifiedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("hazard-categories/{id}")]
    public async Task<ActionResult<HazardCategoryDto>> GetHazardCategory(int id)
    {
        var category = await _context.Set<HazardCategory>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        return Ok(new HazardCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Color = category.Color,
            RiskLevel = category.RiskLevel,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = category.LastModifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? category.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    [HttpPost("hazard-categories")]
    public async Task<ActionResult<HazardCategoryDto>> CreateHazardCategory(CreateHazardCategoryRequest request)
    {
        // Check if code already exists
        var existingCategory = await _context.Set<HazardCategory>()
            .FirstOrDefaultAsync(c => c.Code == request.Code);

        if (existingCategory != null)
            return BadRequest($"A category with code '{request.Code}' already exists.");

        var category = HazardCategory.Create(
            request.Name,
            request.Code,
            request.Description,
            request.Color ?? "#fd7e14",
            request.RiskLevel,
            request.DisplayOrder
        );

        _context.Set<HazardCategory>().Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHazardCategory), new { id = category.Id }, new HazardCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Color = category.Color,
            RiskLevel = category.RiskLevel,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = category.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    [HttpPut("hazard-categories/{id}")]
    public async Task<ActionResult<HazardCategoryDto>> UpdateHazardCategory(int id, UpdateHazardCategoryRequest request)
    {
        var category = await _context.Set<HazardCategory>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        // Check if code already exists for another category
        var existingCategory = await _context.Set<HazardCategory>()
            .FirstOrDefaultAsync(c => c.Code == request.Code && c.Id != id);

        if (existingCategory != null)
            return BadRequest($"A category with code '{request.Code}' already exists.");

        category.Update(
            request.Name,
            request.Code,
            request.Description,
            request.Color ?? "#fd7e14",
            request.RiskLevel,
            request.DisplayOrder
        );

        if (request.IsActive)
            category.Activate();
        else
            category.Deactivate();

        await _context.SaveChangesAsync();

        return Ok(new HazardCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            Description = category.Description,
            Color = category.Color,
            RiskLevel = category.RiskLevel,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = category.LastModifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? category.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    [HttpDelete("hazard-categories/{id}")]
    public async Task<ActionResult> DeleteHazardCategory(int id)
    {
        var category = await _context.Set<HazardCategory>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        // Check if category is being used by hazards
        var hazardCount = await _context.Set<Hazard>()
            .CountAsync(h => h.CategoryId == id);

        if (hazardCount > 0)
            return BadRequest($"Cannot delete category '{category.Name}' because it is being used by {hazardCount} hazard(s).");

        _context.Set<HazardCategory>().Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Hazard Types endpoints
    [HttpGet("hazard-types")]
    public async Task<ActionResult<IEnumerable<HazardTypeDto>>> GetHazardTypes()
    {
        var types = await _context.Set<HazardType>()
            .Include(t => t.Category)
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .Select(t => new HazardTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null,
                RiskMultiplier = t.RiskMultiplier,
                RequiresPermit = t.RequiresPermit,
                DisplayOrder = t.DisplayOrder,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = t.LastModifiedAt.HasValue ? t.LastModifiedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("hazard-types/{id}")]
    public async Task<ActionResult<HazardTypeDto>> GetHazardType(int id)
    {
        var type = await _context.Set<HazardType>()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type == null)
            return NotFound();

        return Ok(new HazardTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Code = type.Code,
            Description = type.Description,
            CategoryId = type.CategoryId,
            CategoryName = type.Category?.Name,
            RiskMultiplier = type.RiskMultiplier,
            RequiresPermit = type.RequiresPermit,
            DisplayOrder = type.DisplayOrder,
            IsActive = type.IsActive,
            CreatedAt = type.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = type.LastModifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? type.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    [HttpPost("hazard-types")]
    public async Task<ActionResult<HazardTypeDto>> CreateHazardType(CreateHazardTypeRequest request)
    {
        // Check if code already exists
        var existingType = await _context.Set<HazardType>()
            .FirstOrDefaultAsync(t => t.Code == request.Code);

        if (existingType != null)
            return BadRequest($"A type with code '{request.Code}' already exists.");

        var type = HazardType.Create(
            request.Name,
            request.Code,
            request.Description,
            request.CategoryId,
            request.RiskMultiplier,
            request.RequiresPermit,
            request.DisplayOrder
        );

        _context.Set<HazardType>().Add(type);
        await _context.SaveChangesAsync();

        // Load the category for response
        await _context.Entry(type)
            .Reference(t => t.Category)
            .LoadAsync();

        return CreatedAtAction(nameof(GetHazardType), new { id = type.Id }, new HazardTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Code = type.Code,
            Description = type.Description,
            CategoryId = type.CategoryId,
            CategoryName = type.Category?.Name,
            RiskMultiplier = type.RiskMultiplier,
            RequiresPermit = type.RequiresPermit,
            DisplayOrder = type.DisplayOrder,
            IsActive = type.IsActive,
            CreatedAt = type.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = type.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    [HttpPut("hazard-types/{id}")]
    public async Task<ActionResult<HazardTypeDto>> UpdateHazardType(int id, UpdateHazardTypeRequest request)
    {
        var type = await _context.Set<HazardType>()
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type == null)
            return NotFound();

        // Check if code already exists for another type
        var existingType = await _context.Set<HazardType>()
            .FirstOrDefaultAsync(t => t.Code == request.Code && t.Id != id);

        if (existingType != null)
            return BadRequest($"A type with code '{request.Code}' already exists.");

        type.Update(
            request.Name,
            request.Code,
            request.Description,
            request.CategoryId,
            request.RiskMultiplier,
            request.RequiresPermit,
            request.DisplayOrder
        );

        if (request.IsActive)
            type.Activate();
        else
            type.Deactivate();

        await _context.SaveChangesAsync();

        // Reload the category reference
        await _context.Entry(type)
            .Reference(t => t.Category)
            .LoadAsync();

        return Ok(new HazardTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Code = type.Code,
            Description = type.Description,
            CategoryId = type.CategoryId,
            CategoryName = type.Category?.Name,
            RiskMultiplier = type.RiskMultiplier,
            RequiresPermit = type.RequiresPermit,
            DisplayOrder = type.DisplayOrder,
            IsActive = type.IsActive,
            CreatedAt = type.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            UpdatedAt = type.LastModifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? type.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }

    [HttpDelete("hazard-types/{id}")]
    public async Task<ActionResult> DeleteHazardType(int id)
    {
        var type = await _context.Set<HazardType>()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type == null)
            return NotFound();

        // Check if type is being used by hazards
        var hazardCount = await _context.Set<Hazard>()
            .CountAsync(h => h.TypeId == id);

        if (hazardCount > 0)
            return BadRequest($"Cannot delete type '{type.Name}' because it is being used by {hazardCount} hazard(s).");

        _context.Set<HazardType>().Remove(type);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs
public class HazardCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class HazardTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal RiskMultiplier { get; set; }
    public bool RequiresPermit { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

public class CreateHazardCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string RiskLevel { get; set; } = "Medium";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateHazardCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string RiskLevel { get; set; } = "Medium";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateHazardTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public decimal RiskMultiplier { get; set; } = 1.0m;
    public bool RequiresPermit { get; set; } = false;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateHazardTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public decimal RiskMultiplier { get; set; } = 1.0m;
    public bool RequiresPermit { get; set; } = false;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}