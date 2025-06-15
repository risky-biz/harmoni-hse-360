// Audit Types
export type AuditType = 'Safety' | 'Environmental' | 'Equipment' | 'Process' | 'Compliance' | 'Fire' | 'Chemical' | 'Ergonomic' | 'Emergency' | 'Management';

export type AuditStatus = 'Draft' | 'Scheduled' | 'InProgress' | 'Completed' | 'Overdue' | 'Cancelled' | 'Archived' | 'UnderReview';

export type AuditPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export type AuditCategory = 'Routine' | 'Planned' | 'Unplanned' | 'Regulatory' | 'Internal' | 'External' | 'Incident' | 'Maintenance';

export type AuditScore = 'Excellent' | 'Good' | 'Satisfactory' | 'NeedsImprovement' | 'Unsatisfactory';

export type RiskLevel = 'Low' | 'Medium' | 'High' | 'Critical';

export type AuditResult = 'Compliant' | 'PartiallyCompliant' | 'NonCompliant' | 'NotApplicable' | 'RequiresFollowUp';

export type FindingType = 'NonCompliance' | 'Opportunity' | 'Observation';

export type FindingSeverity = 'Minor' | 'Moderate' | 'Major' | 'Critical';

export type FindingStatus = 'Open' | 'InProgress' | 'Resolved' | 'Verified' | 'Closed';

export type AuditAttachmentType = 'Evidence' | 'Checklist' | 'Report' | 'Photo' | 'Document' | 'Certificate' | 'Standard' | 'Procedure' | 'Other';

// Core DTOs
export interface AuditItemDto {
  id: number;
  auditId: number;
  checklistItemNumber: string;
  checklistItemText: string;
  requiredEvidence?: string;
  section?: string;
  subsection?: string;
  category: AuditCategory;
  priority: AuditPriority;
  status: string;
  result?: AuditResult;
  score?: number;
  maxScore: number;
  weight: number;
  assessedDate?: string;
  assessedBy?: string;
  assessedByName?: string;
  comments?: string;
  evidenceNotes?: string;
  complianceNotes?: string;
  correctiveActions?: string;
  isMandatory: boolean;
  isApplicable: boolean;
  requiresEvidence: boolean;
  
  // Computed properties
  isCompliant: boolean;
  isNonCompliant: boolean;
  isNotApplicable: boolean;
  needsAttention: boolean;
  scorePercentage: number;
}

export interface AuditFindingDto {
  id: number;
  auditId: number;
  auditItemId?: number;
  findingNumber: string;
  title: string;
  description: string;
  type: FindingType;
  severity: FindingSeverity;
  priority: AuditPriority;
  status: FindingStatus;
  riskLevel: RiskLevel;
  category: AuditCategory;
  location?: string;
  equipment?: string;
  process?: string;
  immediateAction?: string;
  rootCause?: string;
  correctiveAction?: string;
  preventiveAction?: string;
  responsiblePersonId?: string;
  responsiblePersonName?: string;
  targetCloseDate?: string;
  actualCloseDate?: string;
  verificationRequired: boolean;
  verificationDate?: string;
  verifiedById?: string;
  verifiedByName?: string;
  verificationNotes?: string;
  regulatoryReference?: string;
  standardReference?: string;
  isRegulatory: boolean;
  attachments?: AuditFindingAttachmentDto[];
  
  // Computed properties
  isOverdue: boolean;
  daysToTarget: number;
  isOpen: boolean;
  isClosed: boolean;
  isVerified: boolean;
  isCritical: boolean;
  requiresImmediateAction: boolean;
}

export interface AuditAttachmentDto {
  id: number;
  auditId: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  filePath: string;
  fileSize: number;
  description?: string;
  category?: string;
  uploadedById: string;
  uploadedByName: string;
  uploadedAt: string;
  isEvidence: boolean;
  isPublic: boolean;
}

export interface AuditFindingAttachmentDto {
  id: number;
  findingId: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  filePath: string;
  fileSize: number;
  description?: string;
  category?: string;
  uploadedById: string;
  uploadedByName: string;
  uploadedAt: string;
  isEvidence: boolean;
  isBeforePhoto: boolean;
  isAfterPhoto: boolean;
}

export interface AuditCommentDto {
  id: number;
  auditId: number;
  comment: string;
  commentType: string;
  isInternal: boolean;
  isSystemGenerated: boolean;
  commentedById: string;
  commentedByName: string;
  commentedAt: string;
}

export interface AuditDto {
  id: number;
  auditNumber: string;
  title: string;
  description: string;
  type: AuditType;
  typeDisplay: string;
  category: AuditCategory;
  categoryDisplay: string;
  status: AuditStatus;
  statusDisplay: string;
  priority: AuditPriority;
  priorityDisplay: string;
  riskLevel: RiskLevel;
  riskLevelDisplay: string;
  
  // Schedule & Execution
  scheduledDate: string;
  startedDate?: string;
  completedDate?: string;
  auditorId: string;
  auditorName: string;
  locationId?: number;
  locationName?: string;
  departmentId?: number;
  departmentName?: string;
  facilityId?: number;
  facilityName?: string;
  
  // Assessment Results
  summary?: string;
  recommendations?: string;
  overallScore?: AuditScore;
  overallScoreDisplay?: string;
  estimatedDurationMinutes?: number;
  actualDurationMinutes?: number;
  scorePercentage?: number;
  totalPossiblePoints: number;
  achievedPoints: number;
  
  // Compliance & Standards
  standardsApplied?: string;
  isRegulatory: boolean;
  regulatoryReference?: string;
  
  // Related Data
  items: AuditItemDto[];
  findings: AuditFindingDto[];
  attachments: AuditAttachmentDto[];
  comments: AuditCommentDto[];
  
  // Computed Properties
  isOverdue: boolean;
  canEdit: boolean;
  canStart: boolean;
  canComplete: boolean;
  canCancel: boolean;
  canArchive: boolean;
  hasFindings: boolean;
  hasCriticalFindings: boolean;
  completionPercentage: number;
  
  // Audit fields
  createdAt: string;
  lastModifiedAt?: string;
  createdBy: string;
  lastModifiedBy?: string;
}

export interface AuditSummaryDto {
  id: number;
  auditNumber: string;
  title: string;
  type: AuditType;
  typeDisplay: string;
  status: AuditStatus;
  statusDisplay: string;
  priority: AuditPriority;
  priorityDisplay: string;
  riskLevel: RiskLevel;
  riskLevelDisplay: string;
  scheduledDate: string;
  auditorName: string;
  departmentName?: string;
  locationName?: string;
  completionPercentage: number;
  findingsCount: number;
  criticalFindingsCount: number;
  isOverdue: boolean;
}

// Dashboard DTOs
export interface AuditDashboardDto {
  // Summary Statistics
  totalAudits: number;
  scheduledAudits: number;
  inProgressAudits: number;
  completedAudits: number;
  overdueAudits: number;
  
  // Finding Statistics
  totalFindings: number;
  openFindings: number;
  criticalFindings: number;
  averageCompletionTime: number;
  
  // Trend Data
  auditsTrend: AuditTrendDto[];
  findingsTrend: FindingTrendDto[];
  complianceScore: number;
  
  // Recent Activity
  recentAudits: AuditSummaryDto[];
  upcomingAudits: AuditSummaryDto[];
  overdueAudits: AuditSummaryDto[];
  criticalFindings: AuditFindingDto[];
  
  // Charts Data
  auditsByType: AuditsByTypeDto[];
  auditsByStatus: AuditsByStatusDto[];
  auditsByDepartment: AuditsByDepartmentDto[];
  findingsBySeverity: FindingsBySeverityDto[];
}

export interface AuditTrendDto {
  date: string;
  completed: number;
  scheduled: number;
  overdue: number;
}

export interface FindingTrendDto {
  date: string;
  total: number;
  critical: number;
  resolved: number;
}

export interface AuditsByTypeDto {
  type: AuditType;
  typeDisplay: string;
  count: number;
  percentage: number;
}

export interface AuditsByStatusDto {
  status: AuditStatus;
  statusDisplay: string;
  count: number;
  percentage: number;
}

export interface AuditsByDepartmentDto {
  departmentId: number;
  departmentName: string;
  count: number;
  percentage: number;
}

export interface FindingsBySeverityDto {
  severity: FindingSeverity;
  severityDisplay: string;
  count: number;
  percentage: number;
}

export interface AuditStatisticsDto {
  totalAudits: number;
  avgCompletionTime: number;
  complianceRate: number;
  auditFrequency: number;
  findingResolutionRate: number;
  regulatoryComplianceRate: number;
}

// Request/Response Types
export interface CreateAuditRequest {
  title: string;
  description: string;
  type: AuditType;
  category: AuditCategory;
  priority: AuditPriority;
  riskLevel: RiskLevel;
  scheduledDate: string;
  locationId?: number;
  departmentId?: number;
  facilityId?: number;
  estimatedDurationMinutes?: number;
  isRegulatory: boolean;
  regulatoryReference?: string;
  standardsApplied?: string;
}

export interface UpdateAuditRequest {
  id: number;
  title: string;
  description: string;
  type: AuditType;
  category: AuditCategory;
  priority: AuditPriority;
  riskLevel: RiskLevel;
  scheduledDate: string;
  locationId?: number;
  departmentId?: number;
  facilityId?: number;
  estimatedDurationMinutes?: number;
  isRegulatory: boolean;
  regulatoryReference?: string;
  standardsApplied?: string;
}

export interface GetAuditsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: AuditStatus;
  type?: AuditType;
  category?: AuditCategory;
  priority?: AuditPriority;
  riskLevel?: RiskLevel;
  auditorId?: number;
  departmentId?: number;
  startDate?: string;
  endDate?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface GetAuditsResponse {
  items: AuditSummaryDto[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}