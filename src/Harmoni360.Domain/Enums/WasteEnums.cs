namespace Harmoni360.Domain.Enums;

public enum WasteClassification
{
    NonHazardous = 1,
    HazardousChemical = 2,
    HazardousBiological = 3,
    HazardousRadioactive = 4,
    Recyclable = 5,
    Organic = 6,
    Electronic = 7,
    Construction = 8,
    Medical = 9,
    Universal = 10
}

public enum WasteSource
{
    Laboratory = 1,
    Cafeteria = 2,
    Office = 3,
    Maintenance = 4,
    Construction = 5,
    Medical = 6,
    Classroom = 7,
    Event = 8,
    Other = 9
}

public enum UnitOfMeasure
{
    Kilogram = 1,
    Liter = 2,
    CubicMeter = 3,
    Ton = 4,
    Gallon = 5,
    Pound = 6,
    Unit = 7,
    Container = 8
}

public enum WasteReportStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    InStorage = 5,
    AwaitingPickup = 6,
    InTransit = 7,
    Disposed = 8,
    Rejected = 9,
    Cancelled = 10
}

public enum DisposalStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    CertificatePending = 4,
    Certified = 5,
    Failed = 6
}

public enum AttachmentType
{
    Photo = 1,
    Manifest = 2,
    Certificate = 3,
    Invoice = 4,
    Permit = 5,
    Report = 6,
    Other = 7
}

public enum CommentType
{
    General = 1,
    StatusUpdate = 2,
    ComplianceNote = 3,
    DisposalUpdate = 4,
    Correction = 5
}

public enum ProviderStatus
{
    Active = 1,
    Suspended = 2,
    Expired = 3,
    UnderReview = 4,
    Terminated = 5
}

public enum ComplianceStatus
{
    Compliant = 1,
    NonCompliant = 2,
    PendingReview = 3,
    Overdue = 4
}
