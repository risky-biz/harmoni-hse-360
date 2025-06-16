// Work Permit Types
export type WorkPermitType = 'General' | 'HotWork' | 'ColdWork' | 'ConfinedSpace' | 'ElectricalWork' | 'Special';

export type WorkPermitStatus = 'Draft' | 'Submitted' | 'PendingApproval' | 'Approved' | 'Rejected' | 'InProgress' | 'Completed' | 'Cancelled' | 'Expired';

export type WorkPermitPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type RiskLevel = 'Low' | 'Medium' | 'High' | 'Critical';

export type WorkPermitAttachmentType = 
  | 'WorkPlan' 
  | 'SafetyProcedure' 
  | 'RiskAssessment' 
  | 'MethodStatement' 
  | 'CertificateOfIsolation' 
  | 'PermitToWork' 
  | 'PhotoEvidence' 
  | 'ComplianceDocument' 
  | 'K3License' 
  | 'EnvironmentalPermit' 
  | 'CompanyPermit' 
  | 'Other';

export type HazardCategory = 
  | 'Physical' 
  | 'Chemical' 
  | 'Biological' 
  | 'Ergonomic' 
  | 'Psychosocial' 
  | 'Environmental' 
  | 'Fire' 
  | 'Electrical' 
  | 'Mechanical' 
  | 'Radiation' 
  | 'Confined_Space' 
  | 'Height_Related' 
  | 'Traffic_Related' 
  | 'Weather_Related';

export type PrecautionCategory = 
  | 'PersonalProtectiveEquipment' 
  | 'Isolation' 
  | 'FireSafety' 
  | 'GasMonitoring' 
  | 'VentilationControl' 
  | 'AccessControl' 
  | 'EmergencyProcedures' 
  | 'EnvironmentalProtection' 
  | 'TrafficControl' 
  | 'WeatherPrecautions' 
  | 'EquipmentSafety' 
  | 'MaterialHandling' 
  | 'WasteManagement' 
  | 'CommunicationProtocol' 
  | 'K3_Compliance' 
  | 'BPJS_Compliance' 
  | 'Environmental_Permit' 
  | 'Other';

// Core DTOs
export interface GeoLocationDto {
  latitude: number;
  longitude: number;
  address: string;
  locationDescription: string;
}

export interface WorkPermitDto {
  id: number;
  permitNumber: string;
  title: string;
  description: string;
  type: WorkPermitType;
  typeDisplay: string;
  status: WorkPermitStatus;
  statusDisplay: string;
  priority: WorkPermitPriority;
  priorityDisplay: string;
  
  // Work Details
  workLocation: string;
  geoLocation?: GeoLocationDto;
  plannedStartDate: string;
  plannedEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  // Aliases for backward compatibility
  startDate: string;
  endDate: string;
  validUntil: string;
  estimatedDuration: number;
  
  // Personnel Information
  requestedById: number;
  requestedByName: string;
  requestedByDepartment: string;
  // Aliases for backward compatibility  
  requestorName: string;
  requestorEmail: string;
  requestorDepartment: string;
  requestedByPosition: string;
  contactPhone: string;
  workSupervisor: string;
  safetyOfficer: string;
  
  // Work Scope
  workScope: string;
  equipmentToBeUsed: string;
  materialsInvolved: string;
  numberOfWorkers: number;
  contractorCompany: string;
  
  // Safety Requirements
  requiresHotWorkPermit: boolean;
  requiresConfinedSpaceEntry: boolean;
  requiresElectricalIsolation: boolean;
  requiresHeightWork: boolean;
  requiresRadiationWork: boolean;
  requiresExcavation: boolean;
  requiresFireWatch: boolean;
  requiresGasMonitoring: boolean;
  
  // Indonesian K3 Compliance
  k3LicenseNumber: string;
  companyWorkPermitNumber: string;
  isJamsostekCompliant: boolean;
  hasSMK3Compliance: boolean;
  environmentalPermitNumber: string;
  
  // Risk Assessment
  riskLevel: RiskLevel;
  riskLevelDisplay: string;
  riskAssessmentSummary: string;
  emergencyProcedures: string;
  specialInstructions: string;
  
  // Completion
  completionNotes: string;
  isCompletedSafely: boolean;
  lessonsLearned: string;
  
  // Audit
  createdAt: string;
  updatedAt?: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  
  // Related Data
  attachments: WorkPermitAttachmentDto[];
  approvals: WorkPermitApprovalDto[];
  hazards: WorkPermitHazardDto[];
  precautions: WorkPermitPrecautionDto[];
  
  // Approval Progress Information
  requiredApprovalLevels: string[];
  receivedApprovalLevels: string[];
  missingApprovalLevels: string[];
  approvalProgress: number; // Percentage: (received / required) * 100
  
  // Computed Properties
  isOverdue: boolean;
  isHighRisk: boolean;
  daysUntilExpiry: number;
  completionPercentage: number;
  precautionCompletionPercentage: number;
}

export interface WorkPermitAttachmentDto {
  id: number;
  workPermitId: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  attachmentType: WorkPermitAttachmentType;
  attachmentTypeDisplay: string;
  description: string;
  uploadedBy: string;
  uploadedAt: string;
}

export interface WorkPermitApprovalDto {
  id: number;
  workPermitId: number;
  approvedById: number;
  approvedByName: string;
  // Aliases for backward compatibility
  approverName: string;
  approverRole: string;
  status: string;
  approvalLevel: string;
  approvedAt: string;
  isApproved: boolean;
  comments: string;
  approvalOrder: number;
  k3CertificateNumber: string;
  hasAuthorityToApprove: boolean;
  authorityLevel: string;
}

export interface WorkPermitHazardDto {
  id: number;
  workPermitId: number;
  hazardDescription: string;
  category: HazardCategory;
  categoryDisplay: string;
  riskLevel: RiskLevel;
  riskLevelDisplay: string;
  likelihood: number;
  severity: number;
  riskScore: number;
  controlMeasures: string;
  residualRiskLevel: RiskLevel;
  residualRiskLevelDisplay: string;
  responsiblePerson: string;
  isControlImplemented: boolean;
  controlImplementedDate?: string;
  implementationNotes: string;
}

export interface WorkPermitPrecautionDto {
  id: number;
  workPermitId: number;
  precautionDescription: string;
  category: PrecautionCategory;
  categoryDisplay: string;
  isRequired: boolean;
  isCompleted: boolean;
  completedAt?: string;
  completedBy: string;
  completionNotes: string;
  priority: number;
  responsiblePerson: string;
  verificationMethod: string;
  requiresVerification: boolean;
  isVerified: boolean;
  verifiedAt?: string;
  verifiedBy: string;
  isK3Requirement: boolean;
  k3StandardReference: string;
  isMandatoryByLaw: boolean;
}

// Dashboard and Analytics DTOs
export interface WorkPermitDashboardDto {
  totalPermits: number;
  draftPermits: number;
  pendingApprovalPermits: number;
  approvedPermits: number;
  inProgressPermits: number;
  completedPermits: number;
  rejectedPermits: number;
  cancelledPermits: number;
  expiredPermits: number;
  
  highRiskPermits: number;
  criticalRiskPermits: number;
  overduePermits: number;
  permitsDueToday: number;
  permitsDueThisWeek: number;
  
  permitsByType: WorkPermitTypeStatDto[];
  monthlyTrends: WorkPermitMonthlyTrendDto[];
  recentPermits: WorkPermitDto[];
  highPriorityPermits: WorkPermitDto[];
}

export interface WorkPermitTypeStatDto {
  type: string;
  count: number;
  percentage: number;
}

export interface WorkPermitMonthlyTrendDto {
  month: string;
  totalPermits: number;
  completedPermits: number;
  safelyCompletedPermits: number;
}

export interface WorkPermitStatisticsDto {
  totalPermits: number;
  completedPermits: number;
  safelyCompletedPermits: number;
  overduePermits: number;
  highRiskPermits: number;
  averageCompletionTime: number;
  completionRate: number;
  safetyRate: number;
  
  permitsByType: Record<string, number>;
  permitsByStatus: Record<string, number>;
  permitsByDepartment: Record<string, number>;
  monthlyTrends: MonthlyTrendDto[];
}

export interface MonthlyTrendDto {
  month: string;
  totalPermits: number;
  completedPermits: number;
  safelyCompletedPermits: number;
}

// Request/Response types
export interface CreateWorkPermitRequest {
  title: string;
  description: string;
  type: WorkPermitType;
  priority: WorkPermitPriority;
  workLocation: string;
  geoLocation?: GeoLocationDto;
  plannedStartDate: string;
  plannedEndDate: string;
  estimatedDuration: number;
  contactPhone: string;
  workSupervisor: string;
  safetyOfficer: string;
  workScope: string;
  equipmentToBeUsed: string;
  materialsInvolved: string;
  numberOfWorkers: number;
  contractorCompany: string;
  requiresHotWorkPermit: boolean;
  requiresConfinedSpaceEntry: boolean;
  requiresElectricalIsolation: boolean;
  requiresHeightWork: boolean;
  requiresRadiationWork: boolean;
  requiresExcavation: boolean;
  requiresFireWatch: boolean;
  requiresGasMonitoring: boolean;
  k3LicenseNumber: string;
  companyWorkPermitNumber: string;
  isJamsostekCompliant: boolean;
  hasSMK3Compliance: boolean;
  environmentalPermitNumber: string;
  riskLevel: RiskLevel;
  riskAssessmentSummary: string;
  emergencyProcedures: string;
}

export interface UpdateWorkPermitRequest extends CreateWorkPermitRequest {
  id: number;
}

export interface GetWorkPermitsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: WorkPermitStatus;
  type?: WorkPermitType;
  priority?: WorkPermitPriority;
  riskLevel?: RiskLevel;
  department?: string;
  location?: string;
  startDate?: string;
  endDate?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface GetWorkPermitsResponse {
  items: WorkPermitDto[];
  totalCount: number;
  pageCount: number;
  currentPage: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Form types
export interface WorkPermitFormData extends CreateWorkPermitRequest {
  hazards: Partial<WorkPermitHazardDto>[];
  precautions: Partial<WorkPermitPrecautionDto>[];
}

// Filter types
export interface WorkPermitFilters {
  search: string;
  status: WorkPermitStatus | '';
  type: WorkPermitType | '';
  priority: WorkPermitPriority | '';
  riskLevel: RiskLevel | '';
  department: string;
  location: string;
  startDate: string;
  endDate: string;
  sortBy: string;
  sortDescending: boolean;
}

// Constants
export const WORK_PERMIT_TYPES: { value: WorkPermitType; label: string; description: string }[] = [
  { value: 'General', label: 'General Work Permit', description: 'General HSE work permit for standard activities' },
  { value: 'HotWork', label: 'Hot Work Permit', description: 'For welding, cutting, grinding, and other hot work' },
  { value: 'ColdWork', label: 'Cold Work Permit', description: 'For maintenance and construction without heat sources' },
  { value: 'ConfinedSpace', label: 'Confined Space Entry', description: 'For work in confined spaces requiring special precautions' },
  { value: 'ElectricalWork', label: 'Electrical Work Permit', description: 'For electrical work requiring isolation and safety measures' },
  { value: 'Special', label: 'Special Work Permit', description: 'For radioactive, heights, excavation, and other specialized work' },
];

export const WORK_PERMIT_STATUSES: { value: WorkPermitStatus; label: string; color: string }[] = [
  { value: 'Draft', label: 'Draft', color: 'secondary' },
  { value: 'PendingApproval', label: 'Pending Approval', color: 'warning' },
  { value: 'Approved', label: 'Approved', color: 'success' },
  { value: 'Rejected', label: 'Rejected', color: 'danger' },
  { value: 'InProgress', label: 'In Progress', color: 'info' },
  { value: 'Completed', label: 'Completed', color: 'success' },
  { value: 'Cancelled', label: 'Cancelled', color: 'secondary' },
  { value: 'Expired', label: 'Expired', color: 'danger' },
];

export const WORK_PERMIT_PRIORITIES: { value: WorkPermitPriority; label: string; color: string }[] = [
  { value: 'Low', label: 'Low', color: 'success' },
  { value: 'Medium', label: 'Medium', color: 'warning' },
  { value: 'High', label: 'High', color: 'danger' },
  { value: 'Critical', label: 'Critical', color: 'danger' },
];

export const RISK_LEVELS: { value: RiskLevel; label: string; color: string }[] = [
  { value: 'Low', label: 'Low', color: 'success' },
  { value: 'Medium', label: 'Medium', color: 'warning' },
  { value: 'High', label: 'High', color: 'danger' },
  { value: 'Critical', label: 'Critical', color: 'danger' },
];

export const ATTACHMENT_TYPES: { value: WorkPermitAttachmentType; label: string }[] = [
  { value: 'WorkPlan', label: 'Work Plan' },
  { value: 'SafetyProcedure', label: 'Safety Procedure' },
  { value: 'RiskAssessment', label: 'Risk Assessment' },
  { value: 'MethodStatement', label: 'Method Statement' },
  { value: 'CertificateOfIsolation', label: 'Certificate of Isolation' },
  { value: 'PermitToWork', label: 'Permit to Work' },
  { value: 'PhotoEvidence', label: 'Photo Evidence' },
  { value: 'ComplianceDocument', label: 'Compliance Document' },
  { value: 'K3License', label: 'K3 License' },
  { value: 'EnvironmentalPermit', label: 'Environmental Permit' },
  { value: 'CompanyPermit', label: 'Company Permit' },
  { value: 'Other', label: 'Other' },
];