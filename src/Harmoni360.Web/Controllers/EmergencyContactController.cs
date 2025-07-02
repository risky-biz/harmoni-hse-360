using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using Harmoni360.Application.Features.Health.Commands;
using Harmoni360.Application.Features.Health.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Web.Authorization;
using Harmoni360.Web.Hubs;

namespace Harmoni360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireModuleAccess(ModuleType.HealthMonitoring)]
public class EmergencyContactController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmergencyContactController> _logger;
    private readonly IHubContext<NotificationHub> _notificationHub;

    public EmergencyContactController(
        IMediator mediator,
        ILogger<EmergencyContactController> logger,
        IHubContext<NotificationHub> notificationHub)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationHub = notificationHub;
    }

    /// <summary>
    /// Add emergency contact to a health record
    /// </summary>
    [HttpPost("health-records/{healthRecordId:guid}/contacts")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Create)]
    public async Task<ActionResult<Guid>> AddEmergencyContact(
        Guid healthRecordId,
        [FromBody] AddEmergencyContactCommand command)
    {
        try
        {
            if (healthRecordId != command.HealthRecordId)
            {
                return BadRequest(new { message = "Health record ID mismatch" });
            }

            var result = await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("EmergencyContactAdded", 
                new { HealthRecordId = healthRecordId, ContactId = result });
            
            return CreatedAtAction(nameof(GetEmergencyContact), new { id = result }, new { Id = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding emergency contact to health record {HealthRecordId}", healthRecordId);
            return StatusCode(500, new { message = "An error occurred while adding the emergency contact" });
        }
    }

    /// <summary>
    /// Get emergency contact by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    public async Task<ActionResult<EmergencyContactDto>> GetEmergencyContact(Guid id)
    {
        try
        {
            var query = new GetEmergencyContactByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound(new { message = "Emergency contact not found" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the emergency contact" });
        }
    }

    /// <summary>
    /// Get all emergency contacts for a health record
    /// </summary>
    [HttpGet("health-records/{healthRecordId:guid}/contacts")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    public async Task<ActionResult<List<EmergencyContactDto>>> GetEmergencyContacts(
        Guid healthRecordId,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new GetEmergencyContactsByHealthRecordQuery 
            { 
                HealthRecordId = healthRecordId,
                IncludeInactive = includeInactive
            };
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency contacts for health record {HealthRecordId}", healthRecordId);
            return StatusCode(500, new { message = "An error occurred while retrieving emergency contacts" });
        }
    }

    /// <summary>
    /// Update emergency contact information
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult> UpdateEmergencyContact(Guid id, [FromBody] UpdateEmergencyContactCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "Emergency contact ID mismatch" });
            }

            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("EmergencyContactUpdated", new { ContactId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the emergency contact" });
        }
    }

    /// <summary>
    /// Remove/deactivate emergency contact
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Delete)]
    public async Task<ActionResult> RemoveEmergencyContact(Guid id)
    {
        try
        {
            var command = new RemoveEmergencyContactCommand { Id = id };
            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("EmergencyContactRemoved", new { ContactId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while removing the emergency contact" });
        }
    }

    /// <summary>
    /// Set primary emergency contact
    /// </summary>
    [HttpPost("{id:guid}/set-primary")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Update)]
    public async Task<ActionResult> SetPrimaryContact(Guid id)
    {
        try
        {
            var command = new SetPrimaryEmergencyContactCommand { Id = id };
            await _mediator.Send(command);
            
            // Notify via SignalR
            await _notificationHub.Clients.All.SendAsync("PrimaryContactChanged", new { ContactId = id });
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while setting the primary contact" });
        }
    }

    /// <summary>
    /// Update contact authorization levels
    /// </summary>
    [HttpPost("{id:guid}/authorize")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    public async Task<ActionResult> UpdateContactAuthorization(Guid id, [FromBody] UpdateContactAuthorizationCommand command)
    {
        try
        {
            if (id != command.Id)
            {
                return BadRequest(new { message = "Emergency contact ID mismatch" });
            }

            await _mediator.Send(command);
            
            // Notify via SignalR if medical authorization changed
            if (command.AuthorizedForMedicalDecisions)
            {
                await _notificationHub.Clients.All.SendAsync("MedicalAuthorizationUpdated", new { ContactId = id });
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact authorization {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while updating contact authorization" });
        }
    }

    /// <summary>
    /// Validate emergency contact information
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    public async Task<ActionResult<ContactValidationResult>> ValidateEmergencyContact(Guid id)
    {
        try
        {
            var command = new ValidateEmergencyContactCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating emergency contact {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while validating the emergency contact" });
        }
    }

    /// <summary>
    /// Bulk import emergency contacts from CSV/Excel
    /// </summary>
    [HttpPost("bulk-import")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Configure)]
    public async Task<ActionResult<BulkImportResult>> BulkImportContacts(IFormFile file, [FromQuery] bool validateOnly = false)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            using var stream = file.OpenReadStream();
            var command = new BulkImportEmergencyContactsCommand 
            { 
                FileStream = stream,
                FileName = file.FileName,
                ValidateOnly = validateOnly
            };
            
            // TODO: Implement BulkImportEmergencyContactsCommand
            var result = new BulkImportResult
            {
                SuccessCount = 0,
                ErrorCount = 0,
                Errors = new List<string> { "Bulk import not yet implemented" }
            };
            
            if (!validateOnly && result.SuccessCount > 0)
            {
                // Notify via SignalR
                await _notificationHub.Clients.All.SendAsync("BulkContactsImported", 
                    new { SuccessCount = result.SuccessCount, ErrorCount = result.ErrorCount });
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk import of emergency contacts");
            return StatusCode(500, new { message = "An error occurred during bulk import" });
        }
    }

    /// <summary>
    /// Export emergency contacts to CSV/Excel
    /// </summary>
    [HttpGet("export")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Export)]
    public async Task<ActionResult> ExportContacts(
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null,
        [FromQuery] string format = "csv",
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = new ExportEmergencyContactsQuery
            {
                Department = department,
                PersonType = personType,
                Format = format,
                IncludeInactive = includeInactive
            };
            
            var result = await _mediator.Send(query);
            
            var contentType = format.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                _ => "text/csv"
            };
            
            var fileName = $"emergency_contacts_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format}";
            
            // TODO: Implement emergency contact export
            var data = new byte[0];
            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting emergency contacts");
            return StatusCode(500, new { message = "An error occurred while exporting emergency contacts" });
        }
    }

    /// <summary>
    /// Test emergency contact reachability
    /// </summary>
    [HttpPost("{id:guid}/test-reachability")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    public async Task<ActionResult<ContactReachabilityResult>> TestContactReachability(Guid id)
    {
        try
        {
            var command = new TestContactReachabilityCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing contact reachability {ContactId}", id);
            return StatusCode(500, new { message = "An error occurred while testing contact reachability" });
        }
    }

    /// <summary>
    /// Get emergency contact statistics and insights
    /// </summary>
    [HttpGet("statistics")]
    [RequireModulePermission(ModuleType.HealthMonitoring, PermissionType.Read)]
    public async Task<ActionResult<EmergencyContactStatistics>> GetContactStatistics(
        [FromQuery] string? department = null,
        [FromQuery] PersonType? personType = null)
    {
        try
        {
            var query = new GetEmergencyContactStatisticsQuery
            {
                Department = department,
                PersonType = personType
            };
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emergency contact statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving contact statistics" });
        }
    }
}

// Supporting DTOs and classes for emergency contact management
public class AddEmergencyContactCommand
{
    public Guid HealthRecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ContactRelationship Relationship { get; set; }
    public string? CustomRelationship { get; set; }
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsPrimaryContact { get; set; }
    public bool AuthorizedForPickup { get; set; }
    public bool AuthorizedForMedicalDecisions { get; set; }
    public int ContactOrder { get; set; }
    public string? Notes { get; set; }
}

public class GetEmergencyContactByIdQuery
{
    public Guid Id { get; set; }
}

public class GetEmergencyContactsByHealthRecordQuery
{
    public Guid HealthRecordId { get; set; }
    public bool IncludeInactive { get; set; }
}

public class UpdateEmergencyContactCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ContactRelationship Relationship { get; set; }
    public string? CustomRelationship { get; set; }
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public int ContactOrder { get; set; }
    public string? Notes { get; set; }
}

public class RemoveEmergencyContactCommand
{
    public Guid Id { get; set; }
}

public class SetPrimaryEmergencyContactCommand
{
    public Guid Id { get; set; }
}

public class UpdateContactAuthorizationCommand
{
    public Guid Id { get; set; }
    public bool AuthorizedForPickup { get; set; }
    public bool AuthorizedForMedicalDecisions { get; set; }
}

public class ValidateEmergencyContactCommand
{
    public Guid Id { get; set; }
}

public class BulkImportEmergencyContactsCommand
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public bool ValidateOnly { get; set; }
}

public class ExportEmergencyContactsQuery
{
    public string? Department { get; set; }
    public PersonType? PersonType { get; set; }
    public string Format { get; set; } = "csv";
    public bool IncludeInactive { get; set; }
}

public class TestContactReachabilityCommand
{
    public Guid Id { get; set; }
}

public class GetEmergencyContactStatisticsQuery
{
    public string? Department { get; set; }
    public PersonType? PersonType { get; set; }
}

// Result DTOs
public class ContactValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    public int ValidationScore { get; set; }
}

public class BulkImportResult
{
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ExportResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class ContactReachabilityResult
{
    public bool IsReachable { get; set; }
    public string TestMethod { get; set; } = string.Empty;
    public string TestResult { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public List<string> ReachabilityIssues { get; set; } = new();
}

public class EmergencyContactStatistics
{
    public int TotalContacts { get; set; }
    public int ActiveContacts { get; set; }
    public int PeopleWithContacts { get; set; }
    public int PeopleWithoutContacts { get; set; }
    public decimal CompletionRate { get; set; }
    public int ContactsWithValidPhone { get; set; }
    public int ContactsWithValidEmail { get; set; }
    public int AuthorizedForPickup { get; set; }
    public int AuthorizedForMedical { get; set; }
    public List<RelationshipBreakdown> RelationshipBreakdown { get; set; } = new();
    public List<DepartmentContactStats> DepartmentStats { get; set; } = new();
}

public class RelationshipBreakdown
{
    public string Relationship { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class DepartmentContactStats
{
    public string Department { get; set; } = string.Empty;
    public int TotalPeople { get; set; }
    public int PeopleWithContacts { get; set; }
    public decimal CompletionRate { get; set; }
}