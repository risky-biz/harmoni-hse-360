using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Incidents.DTOs;

public class IncidentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Reporter info (simplified for list view)
    public string ReporterName { get; set; } = string.Empty;
    public string? ReporterEmail { get; set; }
    public string? ReporterDepartment { get; set; }

    // Injury details
    public string? InjuryType { get; set; }
    public bool? MedicalTreatmentProvided { get; set; }
    public bool? EmergencyServicesContacted { get; set; }
    public string? WitnessNames { get; set; }
    public string? ImmediateActionsTaken { get; set; }

    // Related counts
    public int AttachmentsCount { get; set; }
    public int InvolvedPersonsCount { get; set; }
    public int CorrectiveActionsCount { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Full reporter/investigator objects (optional, for detail views)
    public UserDto? Reporter { get; set; }
    public UserDto? Investigator { get; set; }
}

public class IncidentDetailDto : IncidentDto
{
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<InvolvedPersonDto> InvolvedPersons { get; set; } = new();
    public List<CorrectiveActionDto> CorrectiveActions { get; set; } = new();
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}

public class InvolvedPersonDto
{
    public int Id { get; set; }
    public UserDto Person { get; set; } = null!;
    public string InvolvementType { get; set; } = string.Empty;
    public string? InjuryDescription { get; set; }
}

public class CorrectiveActionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public UserDto AssignedTo { get; set; } = null!;
    public string AssignedToDepartment { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
}