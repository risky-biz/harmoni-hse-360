namespace Harmoni360.Domain.Enums;

public enum AuditType
{
    Safety = 1,
    Environmental = 2,
    Equipment = 3,
    Compliance = 4,
    Fire = 5,
    Chemical = 6,
    Ergonomic = 7,
    Emergency = 8,
    Management = 9,
    Process = 10
}

public enum AuditStatus
{
    Draft = 1,
    Scheduled = 2,
    InProgress = 3,
    Completed = 4,
    Overdue = 5,
    Cancelled = 6,
    Archived = 7,
    UnderReview = 8
}

public enum AuditPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum AuditCategory
{
    Routine = 1,
    Planned = 2,
    Unplanned = 3,
    Regulatory = 4,
    Internal = 5,
    External = 6,
    Incident = 7,
    Maintenance = 8
}

public enum AuditScore
{
    Excellent = 1,    // 90-100%
    Good = 2,         // 80-89%
    Satisfactory = 3, // 70-79%
    NeedsImprovement = 4, // 60-69%
    Unsatisfactory = 5    // Below 60%
}

public enum AuditItemType
{
    YesNo = 1,
    Text = 2,
    Number = 3,
    MultipleChoice = 4,
    Checklist = 5,
    Photo = 6,
    Measurement = 7,
    Rating = 8
}

public enum AuditItemStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    NonCompliant = 4,
    NotApplicable = 5,
    RequiresFollowUp = 6
}

public enum AuditResult
{
    Compliant = 1,
    PartiallyCompliant = 2,
    NonCompliant = 3,
    NotApplicable = 4,
    RequiresFollowUp = 5
}

public enum FindingType
{
    NonConformance = 1,
    Observation = 2,
    OpportunityForImprovement = 3,
    PositiveFinding = 4,
    CriticalNonConformance = 5
}

public enum FindingSeverity
{
    Minor = 1,
    Moderate = 2,
    Major = 3,
    Critical = 4
}

public enum FindingStatus
{
    Open = 1,
    InProgress = 2,
    Resolved = 3,
    Verified = 4,
    Closed = 5
}

public enum AuditAttachmentType
{
    Evidence = 1,
    Checklist = 2,
    Report = 3,
    Photo = 4,
    Document = 5,
    Certificate = 6,
    Standard = 7,
    Procedure = 8,
    Other = 9
}