namespace Harmoni360.Domain.Enums;

public enum InspectionType
{
    Safety = 1,
    Environmental = 2,
    Equipment = 3,
    Compliance = 4,
    Fire = 5,
    Chemical = 6,
    Ergonomic = 7,
    Emergency = 8
}

public enum InspectionStatus
{
    Draft = 1,
    Scheduled = 2,
    InProgress = 3,
    Completed = 4,
    Overdue = 5,
    Cancelled = 6,
    Archived = 7
}

public enum InspectionPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum InspectionCategory
{
    Routine = 1,
    Planned = 2,
    Unplanned = 3,
    Regulatory = 4,
    Audit = 5,
    Incident = 6,
    Maintenance = 7
}

public enum InspectionItemType
{
    YesNo = 1,
    Text = 2,
    Number = 3,
    MultipleChoice = 4,
    Checklist = 5,
    Photo = 6
}

public enum InspectionItemStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    NonCompliant = 4,
    NotApplicable = 5
}

// Finding-related enums are now shared in AuditEnums.cs and imported via using statements