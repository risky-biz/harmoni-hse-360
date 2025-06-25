import { WasteClassification, WasteReportStatus } from './enums';

export interface WasteReportDto {
  id: number;
  title: string;
  description: string;
  classification: WasteClassification;
  classificationDisplay: string;
  status: WasteReportStatus;
  statusDisplay: string;
  reportDate: string;
  reportedBy: string;
  location?: string;
  estimatedQuantity?: number;
  quantityUnit?: string;
  disposalMethod?: string;
  disposalDate?: string;
  disposedBy?: string;
  disposalCost?: number;
  contractorName?: string;
  manifestNumber?: string;
  treatment?: string;
  notes?: string;
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
  updatedBy?: string;
  
  // Navigation properties
  comments: WasteCommentDto[];
  
  // Computed Properties
  canEdit: boolean;
  canDispose: boolean;
  canApprove: boolean;
  canReject: boolean;
  canArchive: boolean;
  isOverdue: boolean;
  isHighRisk: boolean;
  hasComments: boolean;
  commentsCount: number;
  daysUntilDisposal: number;
  daysOverdue: number;
}

export interface WasteReportSummaryDto {
  id: number;
  title: string;
  type: string;
  status: string;
  reportDate: string;
  reportedBy: string;
  location?: string;
  estimatedQuantity?: number;
  quantityUnit?: string;
  disposalDate?: string;
  disposalCost?: number;
  commentsCount: number;
  isOverdue: boolean;
  canEdit: boolean;
  canDispose: boolean;
  createdAt: string;
}

export interface WasteCommentDto {
  id: number;
  wasteReportId: number;
  comment: string;
  commentedBy: string;
  commentedAt: string;
  category?: string;
  isInternal: boolean;
}

export interface WasteReportDashboardDto {
  totalReports: number;
  draftReports: number;
  underReviewReports: number;
  approvedReports: number;
  disposedReports: number;
  rejectedReports: number;
  overdueReports: number;
  
  hazardousWasteReports: number;
  chemicalWasteReports: number;
  reportsDueToday: number;
  reportsDueThisWeek: number;
  
  totalEstimatedQuantity: number;
  totalDisposalCost: number;
  averageDisposalTime: number;
  
  reportsByType: WasteTypeStatDto[];
  monthlyTrends: WasteMonthlyTrendDto[];
  recentReports: WasteReportSummaryDto[];
  highPriorityReports: WasteReportSummaryDto[];
}

export interface WasteTypeStatDto {
  type: string;
  count: number;
  percentage: number;
  totalQuantity: number;
  totalCost: number;
}

export interface WasteMonthlyTrendDto {
  month: string;
  totalReports: number;
  disposedReports: number;
  totalQuantity: number;
  totalCost: number;
}

// Form interfaces
export interface CreateWasteReportForm {
  title: string;
  description: string;
  classification: WasteClassification;
  location?: string;
  estimatedQuantity?: number;
  quantityUnit?: string;
  disposalMethod?: string;
  disposalDate?: string;
  disposedBy?: string;
  disposalCost?: number;
  contractorName?: string;
  manifestNumber?: string;
  treatment?: string;
  notes?: string;
  attachments?: File[];
}

export interface EditWasteReportForm extends CreateWasteReportForm {
  id: number;
}

// Filter interfaces
export interface WasteReportFilters {
  classification?: WasteClassification;
  status?: WasteReportStatus;
  fromDate?: string;
  toDate?: string;
  location?: string;
  reporterId?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

// Request interfaces
export interface AddWasteCommentRequest {
  comment: string;
  category?: string;
  type?: string;
}

export interface WasteAuditLogDto {
  id: number;
  wasteReportId: number;
  action: string;
  fieldName?: string;
  oldValue?: string;
  newValue?: string;
  changeDescription?: string;
  changedBy: string;
  changedAt: string;
  ipAddress?: string;
  userAgent?: string;
  complianceNotes?: string;
  isCriticalAction: boolean;
}