// Inspection Status Enum
export enum InspectionStatus {
  Draft = 1,
  Scheduled = 2,
  InProgress = 3,
  Completed = 4,
  Overdue = 5,
  Cancelled = 6,
  Archived = 7
}

// Inspection Type Enum
export enum InspectionType {
  Safety = 1,
  Environmental = 2,
  Equipment = 3,
  Compliance = 4,
  Fire = 5,
  Chemical = 6,
  Ergonomic = 7,
  Emergency = 8
}

// Inspection Category Enum
export enum InspectionCategory {
  Routine = 1,
  Planned = 2,
  Unplanned = 3,
  Regulatory = 4,
  Audit = 5,
  Incident = 6,
  Maintenance = 7
}

// Inspection Priority Enum
export enum InspectionPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

// Risk Level Enum
export enum RiskLevel {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

// Inspection Item Type Enum
export enum InspectionItemType {
  YesNo = 1,
  Text = 2,
  Number = 3,
  MultipleChoice = 4,
  Checklist = 5,
  Photo = 6
}

// Inspection Item Status Enum
export enum InspectionItemStatus {
  NotStarted = 1,
  InProgress = 2,
  Completed = 3,
  NonCompliant = 4,
  NotApplicable = 5
}

// Finding Type Enum
export enum FindingType {
  Observation = 'Observation',
  NonCompliance = 'NonCompliance',
  Improvement = 'Improvement',
  PositiveFinding = 'PositiveFinding',
  Violation = 'Violation'
}

// Finding Severity Enum
export enum FindingSeverity {
  Minor = 'Minor',
  Major = 'Major',
  Critical = 'Critical',
  Catastrophic = 'Catastrophic'
}

// Finding Status Enum
export enum FindingStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  UnderReview = 'UnderReview',
  Closed = 'Closed',
  Rejected = 'Rejected'
}

// Main Inspection DTO
export interface InspectionDto {
  id: number;
  inspectionNumber: string;
  title: string;
  description: string;
  type: InspectionType;
  typeName: string;
  category: InspectionCategory;
  categoryName: string;
  status: InspectionStatus;
  statusName: string;
  priority: InspectionPriority;
  priorityName: string;
  scheduledDate: string;
  startedDate?: string;
  completedDate?: string;
  inspectorId: number;
  inspectorName: string;
  locationId: number;
  departmentId: number;
  departmentName: string;
  facilityId: number;
  riskLevel: RiskLevel;
  riskLevelName: string;
  summary?: string;
  recommendations?: string;
  estimatedDurationMinutes: number;
  actualDurationMinutes?: number;
  itemsCount: number;
  completedItemsCount: number;
  findingsCount: number;
  criticalFindingsCount: number;
  attachmentsCount: number;
  canEdit: boolean;
  canStart: boolean;
  canComplete: boolean;
  canCancel: boolean;
  isOverdue: boolean;
  createdAt: string;
  lastModifiedAt: string;
  createdBy: string;
  lastModifiedBy: string;
}

// Detailed Inspection DTO
export interface InspectionDetailDto extends InspectionDto {
  items: InspectionItemDto[];
  findings: InspectionFindingDto[];
  attachments: InspectionAttachmentDto[];
  comments: InspectionCommentDto[];
}

// Inspection Item DTO
export interface InspectionItemDto {
  id: number;
  inspectionId: number;
  checklistItemId?: number;
  question: string;
  description?: string;
  type: InspectionItemType;
  typeName: string;
  isRequired: boolean;
  response?: string;
  status: InspectionItemStatus;
  statusName: string;
  notes?: string;
  sortOrder: number;
  expectedValue?: string;
  unit?: string;
  minValue?: number;
  maxValue?: number;
  options?: string[];
  isCompliant: boolean;
  isCompleted: boolean;
  hasResponse: boolean;
}

// Inspection Finding DTO
export interface InspectionFindingDto {
  id: number;
  inspectionId: number;
  findingNumber: string;
  description: string;
  type: FindingType;
  typeName: string;
  severity: FindingSeverity;
  severityName: string;
  riskLevel: RiskLevel;
  riskLevelName: string;
  rootCause?: string;
  immediateAction?: string;
  correctiveAction?: string;
  dueDate?: string;
  responsiblePersonId?: number;
  responsiblePersonName?: string;
  status: FindingStatus;
  statusName: string;
  location?: string;
  equipment?: string;
  regulation?: string;
  closedDate?: string;
  closureNotes?: string;
  isOverdue: boolean;
  canEdit: boolean;
  canClose: boolean;
  hasCorrectiveAction: boolean;
  createdAt: string;
  lastModifiedAt: string;
  createdBy: string;
  lastModifiedBy: string;
  attachments: FindingAttachmentDto[];
}

// Finding Attachment DTO
export interface FindingAttachmentDto {
  id: number;
  findingId: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  fileSizeFormatted: string;
  filePath: string;
  description?: string;
  isPhoto: boolean;
  thumbnailPath?: string;
  isDocument: boolean;
  fileExtension: string;
  createdAt: string;
  lastModifiedAt: string;
  createdBy: string;
  lastModifiedBy: string;
}

// Inspection Attachment DTO
export interface InspectionAttachmentDto {
  id: number;
  inspectionId: number;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  fileSizeFormatted: string;
  filePath: string;
  description?: string;
  category?: string;
  isPhoto: boolean;
  thumbnailPath?: string;
  isDocument: boolean;
  fileExtension: string;
  createdAt: string;
  lastModifiedAt: string;
  createdBy: string;
  lastModifiedBy: string;
}

// Inspection Comment DTO
export interface InspectionCommentDto {
  id: number;
  inspectionId: number;
  userId: number;
  userName: string;
  comment: string;
  isInternal: boolean;
  parentCommentId?: number;
  isReply: boolean;
  hasReplies: boolean;
  createdAt: string;
  lastModifiedAt: string;
  createdBy: string;
  lastModifiedBy: string;
  replies: InspectionCommentDto[];
}

// Dashboard DTO
export interface InspectionDashboardDto {
  totalInspections: number;
  scheduledInspections: number;
  inProgressInspections: number;
  completedInspections: number;
  overdueInspections: number;
  criticalFindings: number;
  averageCompletionTime: number;
  complianceRate: number;
  recentInspections: InspectionDto[];
  criticalFindingsList: InspectionFindingDto[];
  overdueList: InspectionDto[];
  upcomingInspections: InspectionDto[];
  inspectionsByType: InspectionTypeStatistic[];
  inspectionsByStatus: InspectionStatusStatistic[];
  monthlyTrends: MonthlyInspectionTrend[];
}

// Statistics DTOs
export interface InspectionTypeStatistic {
  type: InspectionType;
  typeName: string;
  count: number;
  percentage: number;
}

export interface InspectionStatusStatistic {
  status: InspectionStatus;
  statusName: string;
  count: number;
  percentage: number;
}

export interface MonthlyInspectionTrend {
  month: string;
  year: number;
  scheduled: number;
  completed: number;
  overdue: number;
  criticalFindings: number;
}

// Filter and Sorting Options
export interface InspectionFilters {
  searchTerm?: string;
  status?: InspectionStatus;
  type?: InspectionType;
  category?: InspectionCategory;
  priority?: InspectionPriority;
  inspectorId?: number;
  departmentId?: number;
  startDate?: Date;
  endDate?: Date;
  riskLevel?: RiskLevel;
  isOverdue?: boolean;
}

export interface InspectionSortOptions {
  sortBy: 'title' | 'type' | 'status' | 'priority' | 'inspector' | 'scheduledDate' | 'createdAt';
  sortDescending: boolean;
}

// Common Types
export interface PagedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}