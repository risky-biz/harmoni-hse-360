export enum PersonType {
  Student = 'Student',
  Staff = 'Staff'
}

export enum BloodType {
  APositive = 'APositive',
  ANegative = 'ANegative',
  BPositive = 'BPositive',
  BNegative = 'BNegative',
  ABPositive = 'ABPositive',
  ABNegative = 'ABNegative',
  OPositive = 'OPositive',
  ONegative = 'ONegative'
}

export enum MedicalConditionType {
  Allergy = 'Allergy',
  ChronicCondition = 'ChronicCondition',
  MedicationDependency = 'MedicationDependency',
  DietaryRestriction = 'DietaryRestriction',
  PhysicalLimitation = 'PhysicalLimitation',
  MentalHealthCondition = 'MentalHealthCondition',
  Other = 'Other'
}

export enum MedicalConditionSeverity {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum VaccinationStatus {
  Administered = 'Administered',
  Due = 'Due',
  Overdue = 'Overdue',
  Exempted = 'Exempted',
  Scheduled = 'Scheduled'
}

export enum HealthIncidentType {
  Injury = 'Injury',
  Illness = 'Illness',
  Allergic = 'Allergic',
  Emergency = 'Emergency',
  Preventive = 'Preventive',
  Other = 'Other'
}

export enum HealthIncidentSeverity {
  Minor = 'Minor',
  Moderate = 'Moderate',
  Severe = 'Severe',
  Critical = 'Critical'
}

export interface HealthRecordDto {
  id: string;
  personId: string;
  personName: string;
  personEmail: string;
  personType: PersonType;
  dateOfBirth: string;
  bloodType?: BloodType;
  medicalNotes?: string;
  primaryDoctorName?: string;
  primaryDoctorContact?: string;
  insuranceProvider?: string;
  insurancePolicyNumber?: string;
  lastHealthCheckDate?: string;
  nextHealthCheckDate?: string;
  isActive: boolean;
  medicalConditions: MedicalConditionDto[];
  vaccinations: VaccinationRecordDto[];
  healthIncidents: HealthIncidentDto[];
  emergencyContacts: EmergencyContactDto[];
  createdAt: string;
  lastModifiedAt?: string;
}

export interface MedicalConditionDto {
  id: string;
  healthRecordId: string;
  type: MedicalConditionType;
  name: string;
  description?: string;
  severity: MedicalConditionSeverity;
  diagnosedDate: string;
  diagnosedBy?: string;
  treatmentPlan?: string;
  medicationRequired?: string;
  emergencyProtocol?: string;
  requiresEmergencyAction: boolean;
  createdAt: string;
  lastModifiedAt?: string;
}

export interface VaccinationRecordDto {
  id: string;
  healthRecordId: string;
  vaccineName: string;
  vaccineType?: string;
  dateAdministered: string;
  administeredBy?: string;
  batchNumber?: string;
  manufacturer?: string;
  doseNumber?: number;
  totalDosesRequired?: number;
  expiryDate?: string;
  nextDueDate?: string;
  status: VaccinationStatus;
  exemptionReason?: string;
  exemptionDocumentPath?: string;
  sideEffects?: string;
  createdAt: string;
  lastModifiedAt?: string;
}

export interface HealthIncidentDto {
  id: string;
  incidentId: string;
  healthRecordId: string;
  personName: string;
  type: HealthIncidentType;
  severity: HealthIncidentSeverity;
  dateOccurred: string;
  timeOccurred: string;
  location?: string;
  symptoms?: string;
  vitalSigns?: string;
  treatmentProvided?: string;
  medicationAdministered?: string;
  followUpRequired: boolean;
  followUpNotes?: string;
  treatedBy?: string;
  referredTo?: string;
  parentNotified: boolean;
  createdAt: string;
  lastModifiedAt?: string;
}

export interface EmergencyContactDto {
  id: string;
  healthRecordId: string;
  name: string;
  relationship: string;
  primaryPhone: string;
  secondaryPhone?: string;
  email?: string;
  address?: string;
  workPlace?: string;
  workPhone?: string;
  isPrimary: boolean;
  isAuthorizedForPickup: boolean;
  isAuthorizedForMedicalDecisions: boolean;
  notes?: string;
  priority: number;
  createdAt: string;
  lastModifiedAt?: string;
}

export interface HealthDashboardDto {
  totalHealthRecords: number;
  activeHealthRecords: number;
  studentHealthRecords: number;
  staffHealthRecords: number;
  criticalMedicalConditions: number;
  vaccinationCompliance: VaccinationComplianceDto;
  healthIncidentTrends: HealthIncidentTrendDto[];
  recentHealthIncidents: HealthIncidentDto[];
  upcomingVaccinations: UpcomingVaccinationDto[];
  healthRiskSummary: HealthRiskSummaryDto;
  lastUpdated: string;
}

export interface VaccinationComplianceDto {
  totalRequired: number;
  totalCompliant: number;
  totalOverdue: number;
  totalExempted: number;
  complianceRate: number;
  vaccineBreakdown: VaccineComplianceBreakdownDto[];
  populationBreakdown: PopulationComplianceDto[];
}

export interface VaccineComplianceBreakdownDto {
  vaccineName: string;
  required: number;
  compliant: number;
  overdue: number;
  exempted: number;
  complianceRate: number;
}

export interface PopulationComplianceDto {
  populationType: string;
  totalRequired: number;
  compliant: number;
  complianceRate: number;
}

export interface HealthIncidentTrendDto {
  period: string;
  totalIncidents: number;
  byType: { [key: string]: number };
  bySeverity: { [key: string]: number };
  averageSeverity: number;
}

export interface UpcomingVaccinationDto {
  healthRecordId: string;
  personName: string;
  personType: PersonType;
  vaccineName: string;
  dueDate: string;
  daysUntilDue: number;
  isOverdue: boolean;
}

export interface HealthRiskSummaryDto {
  highRiskIndividuals: number;
  criticalConditions: number;
  overdueVaccinations: number;
  missingEmergencyContacts: number;
  riskLevel: string;
  recommendations: string[];
}

export interface HealthAlertDto {
  id: string;
  alertType: string;
  severity: string;
  title: string;
  message: string;
  affectedPersonId?: string;
  affectedPersonName?: string;
  actionRequired: boolean;
  acknowledgedBy?: string;
  acknowledgedAt?: string;
  createdAt: string;
  expiresAt?: string;
}

export interface CreateHealthRecordRequest {
  personId: string;
  personType: PersonType;
  dateOfBirth: string;
  bloodType?: BloodType;
  medicalNotes?: string;
  primaryDoctorName?: string;
  primaryDoctorContact?: string;
  insuranceProvider?: string;
  insurancePolicyNumber?: string;
}

export interface UpdateHealthRecordRequest {
  bloodType?: BloodType;
  medicalNotes?: string;
  primaryDoctorName?: string;
  primaryDoctorContact?: string;
  insuranceProvider?: string;
  insurancePolicyNumber?: string;
  lastHealthCheckDate?: string;
  nextHealthCheckDate?: string;
}

export interface AddMedicalConditionRequest {
  type: MedicalConditionType;
  name: string;
  description?: string;
  severity: MedicalConditionSeverity;
  diagnosedDate: string;
  diagnosedBy?: string;
  treatmentPlan?: string;
  medicationRequired?: string;
  emergencyProtocol?: string;
  requiresEmergencyAction: boolean;
}

export interface RecordVaccinationRequest {
  vaccineName: string;
  vaccineType?: string;
  dateAdministered: string;
  administeredBy?: string;
  batchNumber?: string;
  manufacturer?: string;
  doseNumber?: number;
  totalDosesRequired?: number;
  expiryDate?: string;
  nextDueDate?: string;
  sideEffects?: string;
}

export interface SetVaccinationExemptionRequest {
  exemptionReason: string;
  exemptionDocumentPath?: string;
}

export interface AddEmergencyContactRequest {
  name: string;
  relationship: string;
  primaryPhone: string;
  secondaryPhone?: string;
  email?: string;
  address?: string;
  workPlace?: string;
  workPhone?: string;
  isPrimary: boolean;
  isAuthorizedForPickup: boolean;
  isAuthorizedForMedicalDecisions: boolean;
  notes?: string;
}

export interface TriggerEmergencyNotificationRequest {
  healthRecordId: string;
  emergencyType: string;
  message: string;
  severity: string;
  notifyContacts: boolean;
  notifyStaff: boolean;
}

export interface HealthRecordListParams {
  personType?: PersonType;
  searchTerm?: string;
  isActive?: boolean;
  hasEmergencyContacts?: boolean;
  vaccinationStatus?: VaccinationStatus;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface HealthRiskAssessmentParams {
  includeVaccinations?: boolean;
  includeMedicalConditions?: boolean;
  includeHealthIncidents?: boolean;
  riskThreshold?: string;
}