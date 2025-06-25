import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// Types
export interface HealthRecordDto {
  id: number;
  personId: number;
  personType: string;
  dateOfBirth?: string;
  bloodType?: string;
  medicalNotes?: string;
  isActive: boolean;

  // Person information
  personName: string;
  personEmail: string;
  personDepartment?: string;
  personPosition?: string;

  // Related counts
  medicalConditionsCount: number;
  vaccinationsCount: number;
  healthIncidentsCount: number;
  emergencyContactsCount: number;

  // Critical health indicators
  hasCriticalConditions: boolean;
  expiringVaccinationsCount: number;
  criticalAllergyAlerts: string[];

  // Audit fields
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;

  // Navigation DTOs
  person?: UserDto;
}

export interface HealthRecordDetailDto extends HealthRecordDto {
  medicalConditions: MedicalConditionDto[];
  vaccinations: VaccinationRecordDto[];
  healthIncidents: HealthIncidentDto[];
  emergencyContacts: EmergencyContactDto[];
}

export interface MedicalConditionDto {
  id: number;
  healthRecordId: number;
  type: string;
  name: string;
  description: string;
  severity: string;
  treatmentPlan?: string;
  diagnosedDate?: string;
  requiresEmergencyAction: boolean;
  emergencyInstructions?: string;
  isActive: boolean;

  // Audit fields
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface VaccinationRecordDto {
  id: number;
  healthRecordId: number;
  vaccineName: string;
  dateAdministered?: string;
  expiryDate?: string;
  batchNumber?: string;
  administeredBy?: string;
  administrationLocation?: string;
  status: string;
  notes?: string;
  isRequired: boolean;
  exemptionReason?: string;

  // Computed fields
  isExpired: boolean;
  isExpiringSoon: boolean;
  daysUntilExpiry: number;
  isCompliant: boolean;

  // Audit fields
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface HealthIncidentDto {
  id: number;
  incidentId?: number;
  healthRecordId: number;
  type: string;
  severity: string;
  symptoms: string;
  treatmentProvided: string;
  treatmentLocation: string;
  requiredHospitalization: boolean;
  parentsNotified: boolean;
  parentNotificationTime?: string;
  returnToSchoolDate?: string;
  followUpRequired?: string;
  treatedBy?: string;
  incidentDateTime: string;
  isResolved: boolean;
  resolutionNotes?: string;

  // Computed fields
  isCritical: boolean;
  requiresParentNotification: boolean;
  isOverdue: boolean;

  // Audit fields
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;

  // Navigation DTOs
  relatedIncident?: IncidentDto;
}

export interface EmergencyContactDto {
  id: number;
  healthRecordId: number;
  name: string;
  relationship: string;
  customRelationship?: string;
  primaryPhone: string;
  secondaryPhone?: string;
  email?: string;
  address?: string;
  isPrimaryContact: boolean;
  authorizedForPickup: boolean;
  authorizedForMedicalDecisions: boolean;
  contactOrder: number;
  isActive: boolean;
  notes?: string;

  // Computed fields
  displayRelationship: string;
  fullContactInfo: string;
  hasValidContactMethod: boolean;

  // Audit fields
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface UserDto {
  id: number;
  name: string;
  email: string;
  department?: string;
  position?: string;
}

export interface IncidentDto {
  id: number;
  title: string;
  description: string;
  severity: string;
  status: string;
  incidentDate: string;
}

// Request/Response DTOs
export interface GetHealthRecordsParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  personType?: string;
  department?: string;
  hasCriticalConditions?: boolean;
  hasExpiringVaccinations?: boolean;
  includeInactive?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface GetHealthRecordsResponse {
  records: HealthRecordDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  activeRecords: number;
  studentRecords: number;
  staffRecords: number;
  criticalConditionsCount: number;
  expiringVaccinationsCount: number;
}

export interface CreateHealthRecordCommand {
  // Existing person ID (if person already exists)
  personId?: number;
  
  // Person details (for creating new person)
  personName?: string;
  personEmail?: string;
  personPhoneNumber?: string;
  personDepartment?: string;
  personPosition?: string;
  personEmployeeId?: string;
  personType: number;
  
  // Health record details
  dateOfBirth?: string;
  bloodType?: number;
  medicalNotes?: string;
}

export interface UpdateHealthRecordCommand {
  id: number;
  dateOfBirth?: string;
  bloodType?: string;
  medicalNotes?: string;
  isActive?: boolean;
}

export interface AddMedicalConditionCommand {
  healthRecordId: number;
  type: string;
  name: string;
  description: string;
  severity: string;
  treatmentPlan?: string;
  diagnosedDate?: string;
  requiresEmergencyAction: boolean;
  emergencyInstructions?: string;
}

export interface UpdateMedicalConditionCommand {
  id: number;
  type: string;
  name: string;
  description: string;
  severity: string;
  treatmentPlan?: string;
  diagnosedDate?: string;
  requiresEmergencyAction: boolean;
  emergencyInstructions?: string;
  isActive: boolean;
}

export interface RecordVaccinationCommand {
  healthRecordId: number;
  vaccineName: string;
  dateAdministered?: string;
  expiryDate?: string;
  batchNumber?: string;
  administeredBy?: string;
  administrationLocation?: string;
  notes?: string;
  isRequired: boolean;
}

export interface UpdateVaccinationCommand {
  id: number;
  vaccineName: string;
  dateAdministered?: string;
  expiryDate?: string;
  batchNumber?: string;
  administeredBy?: string;
  administrationLocation?: string;
  status: string;
  notes?: string;
  isRequired: boolean;
}

export interface SetVaccinationExemptionCommand {
  id: number;
  exemptionReason: string;
  exemptionExpiryDate?: string;
  approvedBy: string;
}

// Emergency Contact Commands
export interface AddEmergencyContactCommand {
  healthRecordId: number;
  name: string;
  relationship: string;
  customRelationship?: string;
  primaryPhone: string;
  secondaryPhone?: string;
  email?: string;
  address?: string;
  isPrimaryContact: boolean;
  authorizedForPickup: boolean;
  authorizedForMedicalDecisions: boolean;
  contactOrder: number;
  notes?: string;
}

export interface UpdateEmergencyContactCommand {
  id: number;
  name: string;
  relationship: string;
  customRelationship?: string;
  primaryPhone: string;
  secondaryPhone?: string;
  email?: string;
  address?: string;
  contactOrder: number;
  notes?: string;
}

// Dashboard and Analytics DTOs
export interface HealthDashboardDto {
  totalHealthRecords: number;
  totalStudentRecords: number;
  totalStaffRecords: number;
  activeHealthRecords: number;

  // Medical Conditions
  totalMedicalConditions: number;
  criticalMedicalConditions: number;
  lifeThreateningConditions: number;
  conditionsByCategory: ConditionCategoryBreakdown[];

  // Vaccinations
  vaccinationComplianceRate: number;
  expiringVaccinations: number;
  expiredVaccinations: number;
  overdueVaccinations: number;
  vaccinationsByStatus: VaccinationStatusBreakdown[];

  // Health Incidents
  totalHealthIncidents: number;
  criticalHealthIncidents: number;
  unresolvedHealthIncidents: number;
  recentHealthIncidents: number;
  healthIncidentTrends: HealthIncidentTrendDto[];

  // Emergency Contacts
  totalEmergencyContacts: number;
  emergencyContactCompleteness: number;
  primaryContactsMissing: number;

  // Recent activity
  recentHealthRecords: HealthRecordDto[];
  recentHealthIncidentDetails: HealthIncidentDto[];
  expiringVaccinationDetails: VaccinationRecordDto[];

  // Time range information
  fromDate: string;
  toDate: string;
}

export interface ConditionCategoryBreakdown {
  category: string;
  count: number;
  criticalCount: number;
  percentage: number;
}

export interface VaccinationStatusBreakdown {
  status: string;
  count: number;
  percentage: number;
}

export interface HealthIncidentTrendDto {
  date: string;
  count: number;
  criticalCount: number;
}

export interface GetHealthDashboardParams {
  fromDate?: string;
  toDate?: string;
  department?: string;
  personType?: string;
  includeInactive?: boolean;
}

export interface VaccinationComplianceDto {
  totalRecords: number;
  compliantRecords: number;
  nonCompliantRecords: number;
  complianceRate: number;
  expiringVaccinations: number;
  expiredVaccinations: number;
  exemptRecords: number;
  
  // Breakdown by person type
  studentCompliance: VaccinationComplianceBreakdown;
  staffCompliance: VaccinationComplianceBreakdown;
  
  // Breakdown by vaccine type
  vaccinationsByType: VaccinationTypeCompliance[];
  
  // Breakdown by department/grade
  complianceByDepartment: DepartmentVaccinationCompliance[];
  
  // Time range information
  fromDate: string;
  toDate: string;
  
  // Recent non-compliant records
  nonCompliantRecordDetails: NonCompliantRecordDto[];
  
  // Expiring vaccinations
  expiringVaccinationDetails: ExpiringVaccinationDto[];
}

export interface VaccinationComplianceBreakdown {
  personType: string;
  totalRecords: number;
  compliantRecords: number;
  nonCompliantRecords: number;
  complianceRate: number;
  expiringVaccinations: number;
  expiredVaccinations: number;
}

export interface VaccinationTypeCompliance {
  vaccineName: string;
  totalRequired: number;
  totalCompliant: number;
  totalExpired: number;
  totalExpiring: number;
  totalExempt: number;
  complianceRate: number;
  isMandatory: boolean;
}

export interface DepartmentVaccinationCompliance {
  department: string;
  personType: string;
  totalRecords: number;
  compliantRecords: number;
  complianceRate: number;
  atRiskRecords: number;
}

export interface NonCompliantRecordDto {
  healthRecordId: number;
  personName: string;
  personType: string;
  department?: string;
  missingVaccinations: string[];
  expiredVaccinations: string[];
  daysOverdue: number;
  hasExemption: boolean;
}

export interface ExpiringVaccinationDto {
  vaccinationId: number;
  healthRecordId: number;
  personName: string;
  personType: string;
  vaccineName: string;
  expiryDate: string;
  daysUntilExpiry: number;
  isExpired: boolean;
  urgencyLevel: string;
}

export interface GetVaccinationComplianceParams {
  fromDate?: string;
  toDate?: string;
  department?: string;
  personType?: string;
  vaccineName?: string;
  includeInactive?: boolean;
  includeExemptions?: boolean;
}

export interface HealthIncidentTrendsDto {
  totalIncidents: number;
  criticalIncidents: number;
  unresolvedIncidents: number;
  hospitalizationCount: number;
  avgResolutionTimeHours: number;
  
  trendData: HealthIncidentTrendDataPoint[];
  incidentsByType: IncidentTypeBreakdown[];
  incidentsBySeverity: IncidentSeverityBreakdown[];
  incidentsByDepartment: DepartmentIncidentBreakdown[];
  
  peakHours: HourlyIncidentPattern[];
  dayOfWeekPattern: DayOfWeekIncidentPattern[];
  recentCriticalIncidents: CriticalIncidentSummary[];
  riskIndicators: HealthIncidentRiskIndicators;
  
  period: string;
  fromDate: string;
  toDate: string;
  generatedAt: string;
}

export interface HealthIncidentTrendDataPoint {
  date: string;
  count: number;
  criticalCount: number;
  hospitalizationCount: number;
  unresolvedCount: number;
}

export interface IncidentTypeBreakdown {
  type: string;
  count: number;
  criticalCount: number;
  hospitalizationCount: number;
  unresolvedCount: number;
  percentage: number;
  avgResolutionHours: number;
}

export interface IncidentSeverityBreakdown {
  severity: string;
  count: number;
  hospitalizationCount: number;
  unresolvedCount: number;
  percentage: number;
  avgResolutionHours: number;
}

export interface DepartmentIncidentBreakdown {
  department: string;
  count: number;
  criticalCount: number;
  hospitalizationCount: number;
  unresolvedCount: number;
  percentage: number;
  studentIncidents: number;
  staffIncidents: number;
}

export interface HourlyIncidentPattern {
  hour: number;
  count: number;
  criticalCount: number;
  percentage: number;
}

export interface DayOfWeekIncidentPattern {
  dayOfWeek: string;
  count: number;
  criticalCount: number;
  percentage: number;
}

export interface CriticalIncidentSummary {
  id: number;
  incidentId?: number;
  personName: string;
  personType: string;
  department?: string;
  type: string;
  severity: string;
  incidentDateTime: string;
  isCritical: boolean;
  isResolved: boolean;
  requiredHospitalization: boolean;
  daysOpen: number;
  isOverdue: boolean;
}

export interface HealthIncidentRiskIndicators {
  incidentRateIncrease: number;
  criticalIncidentRate: number;
  hospitalizationRate: number;
  unresolvedIncidentRate: number;
  avgResolutionTimeHours: number;
  highRiskDepartments: string[];
  peakIncidentHours: number[];
}

export interface GetHealthIncidentTrendsParams {
  fromDate?: string;
  toDate?: string;
  department?: string;
  personType?: string;
  incidentType?: string;
  severity?: string;
  period?: 'Daily' | 'Weekly' | 'Monthly';
  includeResolved?: boolean;
}

export interface EmergencyContactValidationDto {
  totalHealthRecords: number;
  recordsWithValidContacts: number;
  recordsWithMissingContacts: number;
  recordsWithIncompleteContacts: number;
  validationCompleteness: number;
  
  // Breakdown by person type
  studentContacts: EmergencyContactCompleteness;
  staffContacts: EmergencyContactCompleteness;
  
  // Validation issues breakdown
  validationIssues: ValidationIssueBreakdown[];
  
  // Records requiring attention
  recordsRequiringAttention: ContactValidationIssueDto[];
  
  // Contact method analysis
  contactMethods: ContactMethodAnalysis;
  
  // Department breakdown
  completenessbyDepartment: DepartmentContactCompleteness[];
  
  // Assessment date
  assessmentDate: string;
}

export interface EmergencyContactCompleteness {
  personType: string;
  totalRecords: number;
  recordsWithPrimaryContact: number;
  recordsWithSecondaryContact: number;
  recordsWithValidPhones: number;
  recordsWithValidEmails: number;
  recordsWithPickupAuthorization: number;
  recordsWithMedicalAuthorization: number;
  completenessScore: number;
}

export interface ValidationIssueBreakdown {
  issueType: string;
  severity: string;
  affectedCount: number;
  description: string;
  recommendedActions: string[];
}

export interface ContactValidationIssueDto {
  healthRecordId: number;
  personName: string;
  personType: string;
  department?: string;
  validationIssues: string[];
  highestSeverity: string;
  emergencyContactsCount: number;
  hasPrimaryContact: boolean;
  hasValidContactMethod: boolean;
  lastUpdated: string;
}

export interface ContactMethodAnalysis {
  totalContacts: number;
  contactsWithValidPhone: number;
  contactsWithValidEmail: number;
  contactsWithBothMethods: number;
  contactsWithNoValidMethod: number;
  phoneValidityRate: number;
  emailValidityRate: number;
  overallValidityRate: number;
}

export interface DepartmentContactCompleteness {
  department: string;
  personType: string;
  totalRecords: number;
  recordsWithCompleteContacts: number;
  completenessRate: number;
  criticalIssuesCount: number;
  commonIssues: string[];
}

export interface GetEmergencyContactValidationParams {
  department?: string;
  personType?: string;
  includeInactive?: boolean;
  level?: 'Standard' | 'Detailed' | 'Comprehensive';
}

export interface HealthRiskAssessmentDto {
  totalPopulation: number;
  highRiskIndividuals: number;
  mediumRiskIndividuals: number;
  lowRiskIndividuals: number;
  overallRiskScore: number;
  
  // Risk factors breakdown
  riskFactors: RiskFactorBreakdown[];
  
  // Population health metrics
  studentMetrics: PopulationHealthMetrics;
  staffMetrics: PopulationHealthMetrics;
  
  // Environmental and incident correlations
  incidentCorrelations: HealthIncidentCorrelation[];
  
  // High-risk individuals requiring attention
  highRiskIndividualDetails: HighRiskIndividualDto[];
  
  // Recommendations
  recommendations: HealthRecommendationDto[];
  
  // Assessment period
  assessmentDate: string;
  fromDate: string;
  toDate: string;
}

export interface RiskFactorBreakdown {
  riskFactor: string;
  category: string;
  affectedCount: number;
  riskWeight: number;
  severityLevel: string;
  affectedDepartments: string[];
}

export interface PopulationHealthMetrics {
  populationType: string;
  totalCount: number;
  averageRiskScore: number;
  criticalConditionsCount: number;
  chronicConditionsCount: number;
  healthIncidentsLastMonth: number;
  vaccinationComplianceRate: number;
  emergencyContactCompleteness: number;
}

export interface HealthIncidentCorrelation {
  incidentType: string;
  incidentCount: number;
  correlatedConditions: string[];
  correlatedDepartments: string[];
  trendDirection: string;
  correlationStrength: number;
}

export interface HighRiskIndividualDto {
  healthRecordId: number;
  personName: string;
  personType: string;
  department?: string;
  riskScore: number;
  riskFactors: string[];
  criticalConditions: string[];
  recentIncidentsCount: number;
  hasValidEmergencyContacts: boolean;
  lastHealthUpdate: string;
}

export interface HealthRecommendationDto {
  type: string;
  category: string;
  title: string;
  description: string;
  priority: string;
  affectedAreas: string[];
  estimatedImpact: number;
  recommendedBy: string;
}

export interface GetHealthRiskAssessmentParams {
  fromDate?: string;
  toDate?: string;
  department?: string;
  personType?: string;
  scope?: 'Standard' | 'Comprehensive' | 'Full';
  includeInactive?: boolean;
  includePredictiveAnalysis?: boolean;
}

// Health Alerts DTOs
export interface HealthAlert {
  id: number;
  personId: number;
  personName: string;
  type: 'CriticalCondition' | 'VaccinationExpiring' | 'VaccinationOverdue' | 'HealthIncident' | 'EmergencyContactMissing' | 'DataValidationIssue';
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  message: string;
  createdAt: string;
  isActive: boolean;
}

export interface GetHealthAlertsParams {
  personId?: number;
  department?: string;
  severity?: 'Low' | 'Medium' | 'High' | 'Critical';
  activeOnly?: boolean;
}

export interface EmergencyNotificationRequest {
  personId: number;
  personName: string;
  message: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  location: string;
  emergencyContactIds: number[];
}

export interface EmergencyContactValidationResult {
  contactId: number;
  isValid: boolean;
  validationErrors: string[];
  phoneValidation: ContactValidationStatus;
  emailValidation: ContactValidationStatus;
  lastValidated: string;
}

export interface ContactValidationStatus {
  isValid: boolean;
  validationMessage: string;
  lastChecked?: string;
}

// API slice
export const healthApi = createApi({
  reducerPath: 'healthApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/health',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      headers.set('content-type', 'application/json');
      return headers;
    },
  }),
  tagTypes: [
    'HealthRecord',
    'MedicalCondition',
    'Vaccination',
    'HealthIncident',
    'EmergencyContact',
    'HealthDashboard',
    'HealthStatistics',
    'HealthAlert',
  ],
  endpoints: (builder) => ({
    // Health Records Management
    getHealthRecords: builder.query<GetHealthRecordsResponse, GetHealthRecordsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/records?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: (result) => [
        'HealthRecord',
        ...(result?.records?.map(({ id }) => ({
          type: 'HealthRecord' as const,
          id,
        })) ?? []),
      ],
    }),

    getHealthRecord: builder.query<HealthRecordDetailDto, { id: number; includeInactive?: boolean }>({
      query: ({ id, includeInactive = false }) => ({
        url: `/records/${id}?includeInactive=${includeInactive}`,
        method: 'GET',
      }),
      providesTags: (_, __, { id }) => [{ type: 'HealthRecord' as const, id }],
    }),

    createHealthRecord: builder.mutation<HealthRecordDto, CreateHealthRecordCommand>({
      query: (command) => ({
        url: '/records',
        method: 'POST',
        body: command,
      }),
      invalidatesTags: ['HealthRecord', 'HealthDashboard', 'HealthStatistics'],
    }),

    updateHealthRecord: builder.mutation<void, UpdateHealthRecordCommand>({
      query: ({ id, ...command }) => ({
        url: `/records/${id}`,
        method: 'PUT',
        body: command,
      }),
      invalidatesTags: (_, __, { id }) => [
        { type: 'HealthRecord' as const, id },
        'HealthDashboard',
        'HealthStatistics',
      ],
    }),

    deactivateHealthRecord: builder.mutation<void, number>({
      query: (id) => ({
        url: `/records/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, id) => [
        { type: 'HealthRecord' as const, id },
        'HealthDashboard',
        'HealthStatistics',
      ],
    }),

    // Medical Conditions Management
    addMedicalCondition: builder.mutation<{ id: number }, AddMedicalConditionCommand>({
      query: ({ healthRecordId, ...command }) => ({
        url: `/records/${healthRecordId}/conditions`,
        method: 'POST',
        body: command,
      }),
      invalidatesTags: (_, __, { healthRecordId }) => [
        { type: 'HealthRecord' as const, id: healthRecordId },
        'MedicalCondition',
        'HealthDashboard',
      ],
    }),

    updateMedicalCondition: builder.mutation<void, UpdateMedicalConditionCommand>({
      query: ({ id, ...command }) => ({
        url: `/conditions/${id}`,
        method: 'PUT',
        body: command,
      }),
      invalidatesTags: ['MedicalCondition', 'HealthRecord', 'HealthDashboard'],
    }),

    removeMedicalCondition: builder.mutation<void, number>({
      query: (id) => ({
        url: `/conditions/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['MedicalCondition', 'HealthRecord', 'HealthDashboard'],
    }),

    // Vaccination Management
    recordVaccination: builder.mutation<{ id: number }, RecordVaccinationCommand>({
      query: ({ healthRecordId, ...command }) => ({
        url: `/records/${healthRecordId}/vaccinations`,
        method: 'POST',
        body: command,
      }),
      invalidatesTags: (_, __, { healthRecordId }) => [
        { type: 'HealthRecord' as const, id: healthRecordId },
        'Vaccination',
        'HealthDashboard',
      ],
    }),

    updateVaccination: builder.mutation<void, UpdateVaccinationCommand>({
      query: ({ id, ...command }) => ({
        url: `/vaccinations/${id}`,
        method: 'PUT',
        body: command,
      }),
      invalidatesTags: ['Vaccination', 'HealthRecord', 'HealthDashboard'],
    }),

    setVaccinationExemption: builder.mutation<void, SetVaccinationExemptionCommand>({
      query: ({ id, ...command }) => ({
        url: `/vaccinations/${id}/exemption`,
        method: 'POST',
        body: command,
      }),
      invalidatesTags: ['Vaccination', 'HealthRecord', 'HealthDashboard'],
    }),

    // Emergency Contact Management
    addEmergencyContact: builder.mutation<{ id: number }, AddEmergencyContactCommand>({
      query: ({ healthRecordId, ...command }) => ({
        url: `/records/${healthRecordId}/emergency-contacts`,
        method: 'POST',
        body: command,
      }),
      invalidatesTags: (_, __, { healthRecordId }) => [
        { type: 'HealthRecord' as const, id: healthRecordId },
        'EmergencyContact',
        'HealthDashboard',
      ],
    }),

    updateEmergencyContact: builder.mutation<void, UpdateEmergencyContactCommand>({
      query: ({ id, ...command }) => ({
        url: `/emergency-contacts/${id}`,
        method: 'PUT',
        body: command,
      }),
      invalidatesTags: ['EmergencyContact', 'HealthRecord', 'HealthDashboard'],
    }),

    removeEmergencyContact: builder.mutation<void, number>({
      query: (id) => ({
        url: `/emergency-contacts/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['EmergencyContact', 'HealthRecord', 'HealthDashboard'],
    }),

    getEmergencyContacts: builder.query<EmergencyContactDto[], { healthRecordId: number; includeInactive?: boolean }>({
      query: ({ healthRecordId, includeInactive = false }) => ({
        url: `/records/${healthRecordId}/emergency-contacts?includeInactive=${includeInactive}`,
        method: 'GET',
      }),
      providesTags: (_, __, { healthRecordId }) => [
        { type: 'EmergencyContact' as const, id: healthRecordId },
      ],
    }),

    validateEmergencyContact: builder.mutation<EmergencyContactValidationResult, number>({
      query: (id) => ({
        url: `/emergency-contacts/${id}/validate`,
        method: 'POST',
      }),
    }),

    // Dashboard and Analytics
    getHealthDashboard: builder.query<HealthDashboardDto, GetHealthDashboardParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/dashboard?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HealthDashboard', 'HealthStatistics'],
    }),

    getVaccinationCompliance: builder.query<VaccinationComplianceDto, GetVaccinationComplianceParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/analytics/vaccination-compliance?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HealthStatistics'],
    }),

    getHealthIncidentTrends: builder.query<HealthIncidentTrendsDto, GetHealthIncidentTrendsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/analytics/incident-trends?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HealthStatistics'],
    }),

    getEmergencyContactValidation: builder.query<EmergencyContactValidationDto, GetEmergencyContactValidationParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/analytics/emergency-contact-validation?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HealthStatistics'],
    }),

    getHealthRiskAssessment: builder.query<HealthRiskAssessmentDto, GetHealthRiskAssessmentParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/analytics/risk-assessment?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HealthStatistics'],
    }),

    // Health Alerts and Notifications
    getHealthAlerts: builder.query<HealthAlert[], GetHealthAlertsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });
        return {
          url: `/alerts?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HealthAlert'],
    }),

    triggerEmergencyNotification: builder.mutation<{ message: string }, EmergencyNotificationRequest>({
      query: (request) => ({
        url: '/emergency-notification',
        method: 'POST',
        body: request,
      }),
      invalidatesTags: ['HealthAlert'],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  // Health Records
  useGetHealthRecordsQuery,
  useGetHealthRecordQuery,
  useCreateHealthRecordMutation,
  useUpdateHealthRecordMutation,
  useDeactivateHealthRecordMutation,
  
  // Medical Conditions
  useAddMedicalConditionMutation,
  useUpdateMedicalConditionMutation,
  useRemoveMedicalConditionMutation,
  
  // Vaccinations
  useRecordVaccinationMutation,
  useUpdateVaccinationMutation,
  useSetVaccinationExemptionMutation,
  
  // Emergency Contacts
  useAddEmergencyContactMutation,
  useUpdateEmergencyContactMutation,
  useRemoveEmergencyContactMutation,
  useGetEmergencyContactsQuery,
  useValidateEmergencyContactMutation,
  
  // Dashboard and Analytics
  useGetHealthDashboardQuery,
  useGetVaccinationComplianceQuery,
  useGetHealthIncidentTrendsQuery,
  useGetEmergencyContactValidationQuery,
  useGetHealthRiskAssessmentQuery,
  
  // Alerts and Notifications
  useGetHealthAlertsQuery,
  useTriggerEmergencyNotificationMutation,
} = healthApi;