using HarmoniHSE360.Domain.Entities;

namespace HarmoniHSE360.Application.Features.Incidents.DTOs;

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
    public UserDto Reporter { get; set; } = null!;
    public UserDto? Investigator { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AttachmentCount { get; set; }
    public int InvolvedPersonCount { get; set; }
    public int CorrectiveActionCount { get; set; }
}

public class IncidentDetailDto : IncidentDto
{
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<InvolvedPersonDto> InvolvedPersons { get; set; } = new();
    public List<CorrectiveActionDto> CorrectiveActions { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
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
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CompletionNotes { get; set; }
}