// User types (shared)
export interface UserDto {
  id: number;
  name: string;
  email: string;
  department?: string;
  position?: string;
  employeeId?: string;
}

// Core Hazard types
export interface HazardDto {
  id: number;
  title: string;
  description: string;
  category: string;
  type: string;
  location: string;
  latitude?: number;
  longitude?: number;
  status: string;
  severity: string;
  identifiedDate: string;
  expectedResolutionDate?: string;
  
  // Reporter info
  reporterName: string;
  reporterEmail?: string;
  reporterDepartment: string;
  
  // Current risk assessment summary
  currentRiskLevel?: string;
  currentRiskScore?: number;
  lastAssessmentDate?: string;
  
  // Related counts
  attachmentsCount: number;
  riskAssessmentsCount: number;
  mitigationActionsCount: number;
  pendingActionsCount: number;
  
  // Audit fields
  createdAt: string;
  createdBy?: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  
  // Optional detailed objects for expanded views
  reporter?: UserDto;
  currentRiskAssessment?: RiskAssessmentDto;
}

export interface HazardDetailDto extends HazardDto {
  attachments: HazardAttachmentDto[];
  riskAssessments: RiskAssessmentDto[];
  mitigationActions: HazardMitigationActionDto[];
  reassessments: HazardReassessmentDto[];
}

export interface HazardAttachmentDto {
  id: number;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploadedBy: string;
  uploadedAt: string;
  description?: string;
  downloadUrl: string;
}

export interface RiskAssessmentDto {
  id: number;
  hazardId: number;
  type: string;
  assessorName: string;
  assessmentDate: string;
  
  // Risk scoring
  probabilityScore: number;
  severityScore: number;
  riskScore: number;
  riskLevel: string;
  
  // Assessment details
  potentialConsequences: string;
  existingControls: string;
  recommendedActions: string;
  additionalNotes: string;
  
  // Review cycle
  nextReviewDate: string;
  isActive: boolean;
  
  // Approval
  isApproved: boolean;
  approvedByName?: string;
  approvedAt?: string;
  approvalNotes?: string;
  
  // Optional detailed objects
  assessor?: UserDto;
  approvedBy?: UserDto;
}

export interface HazardMitigationActionDto {
  id: number;
  hazardId: number;
  actionDescription: string;
  type: string;
  status: string;
  priority: string;
  targetDate: string;
  completedDate?: string;
  assignedToName: string;
  completionNotes?: string;
  
  // Cost tracking
  estimatedCost?: number;
  actualCost?: number;
  
  // Effectiveness
  effectivenessRating?: number;
  effectivenessNotes?: string;
  
  // Verification
  requiresVerification: boolean;
  verifiedByName?: string;
  verifiedAt?: string;
  verificationNotes?: string;
  
  // Optional detailed objects
  assignedTo?: UserDto;
  verifiedBy?: UserDto;
}

export interface HazardReassessmentDto {
  id: number;
  hazardId: number;
  scheduledDate: string;
  reason: string;
  isCompleted: boolean;
  completedAt?: string;
  completedByName?: string;
  completionNotes?: string;
  createdAt: string;
  
  // Optional detailed objects
  completedBy?: UserDto;
}

// Request types
export interface CreateHazardRequest {
  title: string;
  description: string;
  category: string;
  type: string;
  location: string;
  latitude?: number;
  longitude?: number;
  severity: string;
  reporterId: number;
  reporterDepartment: string;
  expectedResolutionDate?: string;
  attachments?: File[];
}

export interface UpdateHazardRequest {
  id: number;
  title: string;
  description: string;
  category: string;
  type: string;
  location: string;
  latitude?: number;
  longitude?: number;
  status: string;
  severity: string;
  expectedResolutionDate?: string;
  statusChangeReason?: string;
  newAttachments?: File[];
  attachmentsToRemove?: number[];
}

export interface CreateRiskAssessmentRequest {
  hazardId: number;
  type: string;
  assessorId: number;
  probabilityScore: number;
  severityScore: number;
  potentialConsequences: string;
  existingControls: string;
  recommendedActions: string;
  additionalNotes?: string;
  nextReviewDate: string;
  setAsCurrent?: boolean;
}

export interface CreateMitigationActionRequest {
  hazardId: number;
  actionDescription: string;
  type: string;
  priority: string;
  targetDate: string;
  assignedToId: number;
  estimatedCost?: number;
  requiresVerification?: boolean;
  notes?: string;
}

// Query types
export interface GetHazardsParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  category?: string;
  type?: string;
  status?: string;
  severity?: string;
  riskLevel?: string;
  location?: string;
  department?: string;
  reporterId?: number;
  assignedToId?: number;
  identifiedDateFrom?: string;
  identifiedDateTo?: string;
  expectedResolutionDateFrom?: string;
  expectedResolutionDateTo?: string;
  latitude?: number;
  longitude?: number;
  radiusKm?: number;
  sortBy?: string;
  sortDirection?: string;
  includeAttachments?: boolean;
  includeRiskAssessments?: boolean;
  includeMitigationActions?: boolean;
  includeReporter?: boolean;
  onlyUnassessed?: boolean;
  onlyOverdue?: boolean;
  onlyHighRisk?: boolean;
  onlyMyHazards?: boolean;
}

export interface GetHazardsResponse {
  hazards: HazardDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  summary: HazardsSummary;
}

export interface HazardsSummary {
  totalHazards: number;
  openHazards: number;
  highRiskHazards: number;
  overdueActions: number;
  unassessedHazards: number;
  hazardsByCategory: Record<string, number>;
  hazardsBySeverity: Record<string, number>;
  hazardsByStatus: Record<string, number>;
}

// Dashboard types
export interface HazardDashboardDto {
  overview: HazardOverviewMetrics;
  riskAnalysis: RiskMetrics;
  performance: PerformanceMetrics;
  trends: TrendAnalytics;
  locationData: LocationAnalytics;
  compliance: ComplianceMetrics;
  recentActivities: RecentActivityDto[];
  alerts: HazardAlertDto[];
}

export interface HazardOverviewMetrics {
  totalHazards: number;
  openHazards: number;
  resolvedHazards: number;
  highRiskHazards: number;
  criticalRiskHazards: number;
  overdueActions: number;
  unassessedHazards: number;
  newHazardsThisMonth: number;
  totalHazardsChange: number;
  highRiskChange: number;
  resolutionRateChange: number;
}

export interface RiskMetrics {
  riskLevelDistribution: Record<string, number>;
  categoryDistribution: Record<string, number>;
  severityDistribution: Record<string, number>;
  averageRiskScore: number;
  riskAssessmentsCompleted: number;
  riskAssessmentsPending: number;
  riskAssessmentCompletionRate: number;
}

export interface PerformanceMetrics {
  averageResolutionTime: number;
  mitigationActionCompletionRate: number;
  totalMitigationActions: number;
  completedMitigationActions: number;
  overdueMitigationActions: number;
  averageActionEffectiveness: number;
  costSavingsFromMitigation: number;
  actionTypeDistribution: Record<string, number>;
}

export interface TrendAnalytics {
  hazardReportingTrend: DataPointDto[];
  riskLevelTrend: DataPointDto[];
  resolutionTimeTrend: DataPointDto[];
  categoryTrend: DataPointDto[];
  trendDirection: string;
  keyInsights: string[];
}

export interface LocationAnalytics {
  hotspotLocations: HazardLocationDto[];
  departmentDistribution: Record<string, number>;
  geographicClusters: GeographicClusterDto[];
  mostAffectedArea: string;
  locationsWithHazards: number;
}

export interface ComplianceMetrics {
  overallComplianceScore: number;
  complianceViolations: number;
  auditFindings: number;
  regulatoryReportingCompliance: number;
  nonComplianceAreas: string[];
  lastComplianceReview?: string;
  nextComplianceReview?: string;
}

export interface RecentActivityDto {
  id: number;
  activityType: string;
  title: string;
  description: string;
  timestamp: string;
  performedBy: string;
  severity: string;
  relatedEntityId: number;
}

export interface HazardAlertDto {
  id: number;
  alertType: string;
  title: string;
  message: string;
  severity: string;
  createdAt: string;
  hazardId: number;
  hazardTitle: string;
  isAcknowledged: boolean;
}

export interface DataPointDto {
  label: string;
  value: number;
  secondaryValue?: string;
}

export interface HazardLocationDto {
  location: string;
  hazardCount: number;
  highRiskCount: number;
  latitude?: number;
  longitude?: number;
  department: string;
}

export interface GeographicClusterDto {
  centerLatitude: number;
  centerLongitude: number;
  hazardCount: number;
  radiusMeters: number;
  primaryCategory: string;
  riskLevel: string;
}

export interface GetHazardDashboardParams {
  dateFrom?: string;
  dateTo?: string;
  department?: string;
  includeTrends?: boolean;
  includeLocationAnalytics?: boolean;
  includeComplianceMetrics?: boolean;
  includePerformanceMetrics?: boolean;
  personalizedView?: boolean;
}

// Constants for dropdowns
export const HAZARD_CATEGORIES = [
  'Physical',
  'Chemical', 
  'Biological',
  'Ergonomic',
  'Psychological',
  'Environmental',
  'Fire',
  'Electrical',
  'Mechanical',
  'Radiation'
] as const;

export const HAZARD_TYPES = [
  'Slip',
  'Trip', 
  'Fall',
  'Cut',
  'Burn',
  'Exposure',
  'Collision',
  'Entrapment',
  'Explosion',
  'Fire',
  'Other'
] as const;

export const HAZARD_STATUSES = [
  'Reported',
  'UnderAssessment',
  'ActionRequired',
  'Mitigating',
  'Monitoring',
  'Resolved',
  'Closed'
] as const;

export const HAZARD_SEVERITIES = [
  'Negligible',
  'Minor',
  'Moderate',
  'Major',
  'Catastrophic'
] as const;

export const RISK_LEVELS = [
  'VeryLow',
  'Low',
  'Medium',
  'High',
  'Critical'
] as const;

export const RISK_ASSESSMENT_TYPES = [
  'General',
  'JSA',
  'HIRA',
  'Environmental',
  'Fire'
] as const;

export const MITIGATION_ACTION_TYPES = [
  'Elimination',
  'Substitution',
  'Engineering',
  'Administrative',
  'PPE'
] as const;

export const MITIGATION_PRIORITIES = [
  'Low',
  'Medium',
  'High',
  'Critical'
] as const;