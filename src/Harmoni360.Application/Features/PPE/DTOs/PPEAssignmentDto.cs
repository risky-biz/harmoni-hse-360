namespace Harmoni360.Application.Features.PPE.DTOs;

public class PPEAssignmentDto
{
    public int Id { get; set; }
    public int PPEItemId { get; set; }
    public string PPEItemCode { get; set; } = string.Empty;
    public string PPEItemName { get; set; } = string.Empty;
    public string PPEItemCategory { get; set; } = string.Empty;
    public int AssignedToId { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public string AssignedToEmail { get; set; } = string.Empty;
    public string AssignedToDepartment { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public string? ReturnedBy { get; set; }
    public string? Purpose { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReturnNotes { get; set; }
    
    // Computed Properties
    public int DaysAssigned { get; set; }
    public string AssignmentDurationDisplay { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Audit Info
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class UserPPEAssignmentDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<PPEAssignmentDto> ActiveAssignments { get; set; } = new();
    public List<PPEComplianceStatusDto> ComplianceStatus { get; set; } = new();
    public int TotalAssignedItems { get; set; }
    public int OverdueReturns { get; set; }
    public bool IsCompliant { get; set; }
}

public class PPEComplianceStatusDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryType { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsAssigned { get; set; }
    public int? AssignedItemId { get; set; }
    public string? AssignedItemCode { get; set; }
    public DateTime? AssignedDate { get; set; }
    public bool RequiresTraining { get; set; }
    public bool HasValidTraining { get; set; }
    public bool IsCompliant { get; set; }
    public string? ComplianceNote { get; set; }
}