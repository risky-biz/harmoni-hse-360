using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Health.DTOs;

public class HealthRecordDto
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string PersonType { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public string? MedicalNotes { get; set; }
    public bool IsActive { get; set; }

    // Person information
    public string PersonName { get; set; } = string.Empty;
    public string PersonEmail { get; set; } = string.Empty;
    public string? PersonDepartment { get; set; }
    public string? PersonPosition { get; set; }

    // Related counts
    public int MedicalConditionsCount { get; set; }
    public int VaccinationsCount { get; set; }
    public int HealthIncidentsCount { get; set; }
    public int EmergencyContactsCount { get; set; }

    // Critical health indicators
    public bool HasCriticalConditions { get; set; }
    public int ExpiringVaccinationsCount { get; set; }
    public List<string> CriticalAllergyAlerts { get; set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Navigation DTOs (for detail views)
    public UserDto? Person { get; set; }
}

public class HealthRecordDetailDto : HealthRecordDto
{
    public List<MedicalConditionDto> MedicalConditions { get; set; } = new();
    public List<VaccinationRecordDto> Vaccinations { get; set; } = new();
    public List<HealthIncidentDto> HealthIncidents { get; set; } = new();
    public List<EmergencyContactDto> EmergencyContacts { get; set; } = new();
}

public class MedicalConditionDto
{
    public int Id { get; set; }
    public int HealthRecordId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? TreatmentPlan { get; set; }
    public DateTime? DiagnosedDate { get; set; }
    public bool RequiresEmergencyAction { get; set; }
    public string? EmergencyInstructions { get; set; }
    public bool IsActive { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class VaccinationRecordDto
{
    public int Id { get; set; }
    public int HealthRecordId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateTime? DateAdministered { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? BatchNumber { get; set; }
    public string? AdministeredBy { get; set; }
    public string? AdministrationLocation { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsRequired { get; set; }
    public string? ExemptionReason { get; set; }

    // Computed fields
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsCompliant { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class HealthIncidentDto
{
    public int Id { get; set; }
    public int? IncidentId { get; set; }
    public int HealthRecordId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string TreatmentProvided { get; set; } = string.Empty;
    public string TreatmentLocation { get; set; } = string.Empty;
    public bool RequiredHospitalization { get; set; }
    public bool ParentsNotified { get; set; }
    public DateTime? ParentNotificationTime { get; set; }
    public DateTime? ReturnToSchoolDate { get; set; }
    public string? FollowUpRequired { get; set; }
    public string? TreatedBy { get; set; }
    public DateTime IncidentDateTime { get; set; }
    public bool IsResolved { get; set; }
    public string? ResolutionNotes { get; set; }

    // Computed fields
    public bool IsCritical { get; set; }
    public bool RequiresParentNotification { get; set; }
    public bool IsOverdue { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Navigation DTOs
    public IncidentDto? RelatedIncident { get; set; }
}

public class EmergencyContactDto
{
    public int Id { get; set; }
    public int HealthRecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string? CustomRelationship { get; set; }
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsPrimaryContact { get; set; }
    public bool AuthorizedForPickup { get; set; }
    public bool AuthorizedForMedicalDecisions { get; set; }
    public int ContactOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    // Computed fields
    public string DisplayRelationship { get; set; } = string.Empty;
    public string FullContactInfo { get; set; } = string.Empty;
    public bool HasValidContactMethod { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class HealthDashboardDto
{
    public int TotalHealthRecords { get; set; }
    public int TotalStudentRecords { get; set; }
    public int TotalStaffRecords { get; set; }
    public int ActiveHealthRecords { get; set; }

    // Medical Conditions
    public int TotalMedicalConditions { get; set; }
    public int CriticalMedicalConditions { get; set; }
    public int LifeThreateningConditions { get; set; }
    public List<ConditionCategoryBreakdown> ConditionsByCategory { get; set; } = new();

    // Vaccinations
    public decimal VaccinationComplianceRate { get; set; }
    public int ExpiringVaccinations { get; set; }
    public int ExpiredVaccinations { get; set; }
    public int OverdueVaccinations { get; set; }
    public List<VaccinationStatusBreakdown> VaccinationsByStatus { get; set; } = new();

    // Health Incidents
    public int TotalHealthIncidents { get; set; }
    public int CriticalHealthIncidents { get; set; }
    public int UnresolvedHealthIncidents { get; set; }
    public int RecentHealthIncidents { get; set; }
    public List<HealthIncidentTrendDto> HealthIncidentTrends { get; set; } = new();

    // Emergency Contacts
    public int TotalEmergencyContacts { get; set; }
    public decimal EmergencyContactCompleteness { get; set; }
    public int PrimaryContactsMissing { get; set; }

    // Recent activity
    public List<HealthRecordDto> RecentHealthRecords { get; set; } = new();
    public List<HealthIncidentDto> RecentHealthIncidentDetails { get; set; } = new();
    public List<VaccinationRecordDto> DashboardExpiringVaccinations { get; set; } = new();

    // Time range information
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class ConditionCategoryBreakdown
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CriticalCount { get; set; }
    public decimal Percentage { get; set; }
}

public class VaccinationStatusBreakdown
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class HealthIncidentTrendDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int CriticalCount { get; set; }
}

// Shared DTOs (already defined in other modules)
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
}

public class IncidentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
}

// Additional DTOs for Health Analytics Queries

public class VaccinationComplianceDto
{
    public int TotalRecords { get; set; }
    public int CompliantRecords { get; set; }
    public int NonCompliantRecords { get; set; }
    public decimal ComplianceRate { get; set; }
    public int ExpiringVaccinations { get; set; }
    public int ExpiredVaccinations { get; set; }
    public int ExemptRecords { get; set; }
    
    // Breakdown by person type
    public VaccinationComplianceBreakdown StudentCompliance { get; set; } = new();
    public VaccinationComplianceBreakdown StaffCompliance { get; set; } = new();
    
    // Breakdown by vaccine type
    public List<VaccinationTypeCompliance> VaccinationsByType { get; set; } = new();
    
    // Breakdown by department/grade
    public List<DepartmentVaccinationCompliance> ComplianceByDepartment { get; set; } = new();
    
    // Time range information
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Recent non-compliant records  
    public List<NonCompliantRecordDto> RecentNonCompliantRecords { get; set; } = new();
    
    // Expiring vaccinations
    public List<ExpiringVaccinationDto> CurrentExpiringVaccinations { get; set; } = new();
}

public class VaccinationComplianceBreakdown
{
    public string PersonType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int CompliantRecords { get; set; }
    public int NonCompliantRecords { get; set; }
    public decimal ComplianceRate { get; set; }
    public int ExpiringVaccinations { get; set; }
    public int ExpiredVaccinations { get; set; }
}

public class VaccinationTypeCompliance
{
    public string VaccineName { get; set; } = string.Empty;
    public int TotalRequired { get; set; }
    public int TotalCompliant { get; set; }
    public int TotalExpired { get; set; }
    public int TotalExpiring { get; set; }
    public int TotalExempt { get; set; }
    public decimal ComplianceRate { get; set; }
    public bool IsMandatory { get; set; }
}

public class DepartmentVaccinationCompliance
{
    public string Department { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int CompliantRecords { get; set; }
    public decimal ComplianceRate { get; set; }
    public int AtRiskRecords { get; set; }
}

public class NonCompliantRecordDto
{
    public int HealthRecordId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public List<string> MissingVaccinations { get; set; } = new();
    public List<string> ExpiredVaccinations { get; set; } = new();
    public int DaysOverdue { get; set; }
    public bool HasExemption { get; set; }
}

public class ExpiringVaccinationDto
{
    public int VaccinationId { get; set; }
    public int HealthRecordId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public string VaccineName { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsExpired { get; set; }
    public string UrgencyLevel { get; set; } = string.Empty; // Critical, High, Medium, Low
}

public class HealthRiskAssessmentDto
{
    public int TotalPopulation { get; set; }
    public int HighRiskIndividuals { get; set; }
    public int MediumRiskIndividuals { get; set; }
    public int LowRiskIndividuals { get; set; }
    public decimal OverallRiskScore { get; set; }
    
    // Risk factors breakdown
    public List<RiskFactorBreakdown> RiskFactors { get; set; } = new();
    
    // Population health metrics
    public PopulationHealthMetrics StudentMetrics { get; set; } = new();
    public PopulationHealthMetrics StaffMetrics { get; set; } = new();
    
    // Environmental and incident correlations
    public List<HealthIncidentCorrelation> IncidentCorrelations { get; set; } = new();
    
    // High-risk individuals requiring attention
    public List<HighRiskIndividualDto> HighRiskIndividualDetails { get; set; } = new();
    
    // Recommendations
    public List<HealthRecommendationDto> Recommendations { get; set; } = new();
    
    // Assessment period
    public DateTime AssessmentDate { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class RiskFactorBreakdown
{
    public string RiskFactor { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Medical, Environmental, Behavioral
    public int AffectedCount { get; set; }
    public decimal RiskWeight { get; set; }
    public string SeverityLevel { get; set; } = string.Empty;
    public List<string> AffectedDepartments { get; set; } = new();
}

public class PopulationHealthMetrics
{
    public string PopulationType { get; set; } = string.Empty; // Student, Staff
    public int TotalCount { get; set; }
    public decimal AverageRiskScore { get; set; }
    public int CriticalConditionsCount { get; set; }
    public int ChronicConditionsCount { get; set; }
    public int HealthIncidentsLastMonth { get; set; }
    public decimal VaccinationComplianceRate { get; set; }
    public int EmergencyContactCompleteness { get; set; }
}

public class HealthIncidentCorrelation
{
    public string IncidentType { get; set; } = string.Empty;
    public int IncidentCount { get; set; }
    public List<string> CorrelatedConditions { get; set; } = new();
    public List<string> CorrelatedDepartments { get; set; } = new();
    public string TrendDirection { get; set; } = string.Empty; // Increasing, Decreasing, Stable
    public decimal CorrelationStrength { get; set; }
}

public class HighRiskIndividualDto
{
    public int HealthRecordId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public decimal RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public List<string> CriticalConditions { get; set; } = new();
    public int RecentIncidentsCount { get; set; }
    public bool HasValidEmergencyContacts { get; set; }
    public DateTime LastHealthUpdate { get; set; }
}

public class HealthRecommendationDto
{
    public string Type { get; set; } = string.Empty; // Immediate, Short-term, Long-term
    public string Category { get; set; } = string.Empty; // Medical, Policy, Training
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // Critical, High, Medium, Low
    public List<string> AffectedAreas { get; set; } = new();
    public int EstimatedImpact { get; set; } // Number of people affected
    public DateTime RecommendedBy { get; set; }
}

public class EmergencyContactValidationDto
{
    public int TotalHealthRecords { get; set; }
    public int RecordsWithValidContacts { get; set; }
    public int RecordsWithMissingContacts { get; set; }
    public int RecordsWithIncompleteContacts { get; set; }
    public decimal ValidationCompleteness { get; set; }
    
    // Breakdown by person type
    public EmergencyContactCompleteness StudentContacts { get; set; } = new();
    public EmergencyContactCompleteness StaffContacts { get; set; } = new();
    
    // Validation issues breakdown
    public List<ValidationIssueBreakdown> ValidationIssues { get; set; } = new();
    
    // Records requiring attention
    public List<ContactValidationIssueDto> RecordsRequiringAttention { get; set; } = new();
    
    // Contact method analysis
    public ContactMethodAnalysis ContactMethods { get; set; } = new();
    
    // Department breakdown
    public List<DepartmentContactCompleteness> CompletenessbyDepartment { get; set; } = new();
    
    // Assessment date
    public DateTime AssessmentDate { get; set; }
}

public class EmergencyContactCompleteness
{
    public string PersonType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int RecordsWithPrimaryContact { get; set; }
    public int RecordsWithSecondaryContact { get; set; }
    public int RecordsWithValidPhones { get; set; }
    public int RecordsWithValidEmails { get; set; }
    public int RecordsWithPickupAuthorization { get; set; }
    public int RecordsWithMedicalAuthorization { get; set; }
    public decimal CompletenessScore { get; set; }
}

public class ValidationIssueBreakdown
{
    public string IssueType { get; set; } = string.Empty; // Missing Primary, Invalid Phone, etc.
    public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
    public int AffectedCount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
}

public class ContactValidationIssueDto
{
    public int HealthRecordId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public List<string> ValidationIssues { get; set; } = new();
    public string HighestSeverity { get; set; } = string.Empty;
    public int EmergencyContactsCount { get; set; }
    public bool HasPrimaryContact { get; set; }
    public bool HasValidContactMethod { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ContactMethodAnalysis
{
    public int TotalContacts { get; set; }
    public int ContactsWithValidPhone { get; set; }
    public int ContactsWithValidEmail { get; set; }
    public int ContactsWithBothMethods { get; set; }
    public int ContactsWithNoValidMethod { get; set; }
    public decimal PhoneValidityRate { get; set; }
    public decimal EmailValidityRate { get; set; }
    public decimal OverallValidityRate { get; set; }
}

public class DepartmentContactCompleteness
{
    public string Department { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int RecordsWithCompleteContacts { get; set; }
    public decimal CompletenessRate { get; set; }
    public int CriticalIssuesCount { get; set; }
    public List<string> CommonIssues { get; set; } = new();
}

// Health Incident Trends DTOs

public class HealthIncidentTrendsDto
{
    public int TotalIncidents { get; set; }
    public int CriticalIncidents { get; set; }
    public int UnresolvedIncidents { get; set; }
    public int HospitalizationCount { get; set; }
    public double AvgResolutionTimeHours { get; set; }
    
    public List<HealthIncidentTrendDataPoint> TrendData { get; set; } = new();
    public List<IncidentTypeBreakdown> IncidentsByType { get; set; } = new();
    public List<IncidentSeverityBreakdown> IncidentsBySeverity { get; set; } = new();
    public List<DepartmentIncidentBreakdown> IncidentsByDepartment { get; set; } = new();
    
    public List<HourlyIncidentPattern> PeakHours { get; set; } = new();
    public List<DayOfWeekIncidentPattern> DayOfWeekPattern { get; set; } = new();
    public List<CriticalIncidentSummary> RecentCriticalIncidents { get; set; } = new();
    public HealthIncidentRiskIndicators RiskIndicators { get; set; } = new();
    
    public string Period { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class HealthIncidentTrendDataPoint
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int CriticalCount { get; set; }
    public int HospitalizationCount { get; set; }
    public int UnresolvedCount { get; set; }
}

public class IncidentTypeBreakdown
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CriticalCount { get; set; }
    public int HospitalizationCount { get; set; }
    public int UnresolvedCount { get; set; }
    public decimal Percentage { get; set; }
    public double AvgResolutionHours { get; set; }
}

public class IncidentSeverityBreakdown
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
    public int HospitalizationCount { get; set; }
    public int UnresolvedCount { get; set; }
    public decimal Percentage { get; set; }
    public double AvgResolutionHours { get; set; }
}

public class DepartmentIncidentBreakdown
{
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CriticalCount { get; set; }
    public int HospitalizationCount { get; set; }
    public int UnresolvedCount { get; set; }
    public decimal Percentage { get; set; }
    public int StudentIncidents { get; set; }
    public int StaffIncidents { get; set; }
}

public class HourlyIncidentPattern
{
    public int Hour { get; set; }
    public int Count { get; set; }
    public int CriticalCount { get; set; }
    public decimal Percentage { get; set; }
}

public class DayOfWeekIncidentPattern
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int Count { get; set; }
    public int CriticalCount { get; set; }
    public decimal Percentage { get; set; }
}

public class CriticalIncidentSummary
{
    public int Id { get; set; }
    public int? IncidentId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime IncidentDateTime { get; set; }
    public bool IsCritical { get; set; }
    public bool IsResolved { get; set; }
    public bool RequiredHospitalization { get; set; }
    public int DaysOpen { get; set; }
    public bool IsOverdue { get; set; }
}

public class HealthIncidentRiskIndicators
{
    public decimal IncidentRateIncrease { get; set; }
    public decimal CriticalIncidentRate { get; set; }
    public decimal HospitalizationRate { get; set; }
    public decimal UnresolvedIncidentRate { get; set; }
    public double AvgResolutionTimeHours { get; set; }
    public List<string> HighRiskDepartments { get; set; } = new();
    public List<int> PeakIncidentHours { get; set; } = new();
}

// Missing DTOs for Emergency Contact Validation
public class PersonContactValidationResult
{
    public int HealthRecordId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsValid { get; set; }
    public decimal ValidationScore { get; set; }
    public List<ContactValidationIssue> Issues { get; set; } = new();
    public int ContactCount { get; set; }
    public int ValidContactCount { get; set; }
}

public class ContactValidationIssue
{
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? ContactId { get; set; }
    public string? ContactName { get; set; }
}

public class DepartmentContactValidationBreakdown
{
    public string Department { get; set; } = string.Empty;
    public string PersonType { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public decimal ValidationRate { get; set; }
    public ContactQualityMetrics QualityMetrics { get; set; } = new();
}

public class ContactQualityMetrics
{
    public decimal CompletenessScore { get; set; }
    public decimal AccuracyScore { get; set; }
    public decimal ReachabilityScore { get; set; }
    public decimal OverallQualityScore { get; set; }
}

// Missing DTOs for Health Risk Assessment
public class CriticalConditionsRisk
{
    public int TotalCriticalConditions { get; set; }
    public int UncontrolledCriticalConditions { get; set; }
    public decimal CriticalConditionRate { get; set; }
    public List<string> HighRiskConditions { get; set; } = new();
}

public class VaccinationRisk
{
    public int OverdueVaccinations { get; set; }
    public int ExpiringVaccinations { get; set; }
    public decimal NonComplianceRate { get; set; }
    public List<string> HighRiskVaccines { get; set; } = new();
}

public class IncidentRisk
{
    public int RecentHealthIncidents { get; set; }
    public int CriticalHealthIncidents { get; set; }
    public decimal IncidentTrendRate { get; set; }
    public List<string> HighRiskIncidentTypes { get; set; } = new();
}

public class EmergencyPreparedness
{
    public int RecordsWithoutEmergencyContacts { get; set; }
    public int RecordsWithIncompleteContacts { get; set; }
    public decimal EmergencyReadinessScore { get; set; }
    public List<string> PreparednessGaps { get; set; } = new();
}

public class DepartmentRiskAnalysis
{
    public string Department { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
}

public class AgeGroupRiskAnalysis
{
    public string AgeGroup { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
}

public class IndividualRiskAssessment
{
    public int HealthRecordId { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public List<string> RiskFactors { get; set; } = new();
}

public class PredictiveHealthInsights
{
    public List<string> HealthTrends { get; set; } = new();
    public List<string> RiskPredictions { get; set; } = new();
    public List<string> PreventiveRecommendations { get; set; } = new();
    public List<IndividualRiskAssessment> HighRiskIndividuals { get; set; } = new();
}

public class RiskTrendDataPoint
{
    public DateTime Date { get; set; }
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}