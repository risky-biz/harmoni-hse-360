// Centralized status color management for HarmoniHSE360
// Provides consistent color mappings across all HSSE modules

// Risk level types as used across the application
export type RiskLevel = 'Critical' | 'High' | 'Medium' | 'Low' | 'None';

// Status types for workflow management
export type StatusType = 'Draft' | 'InProgress' | 'UnderReview' | 'Completed' | 'Overdue' | 'Cancelled';

// Audit-specific status types
export type AuditStatus = 'Draft' | 'Scheduled' | 'InProgress' | 'Completed' | 'Overdue' | 'Cancelled';

// Training status types
export type TrainingStatus = 'Draft' | 'Scheduled' | 'InProgress' | 'Completed' | 'Expired' | 'Cancelled';

// Work permit status types
export type WorkPermitStatus = 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'InProgress' | 'Completed' | 'Expired' | 'Cancelled';

/**
 * Get the appropriate CSS color variable for a risk level
 * Returns theme-aware CSS custom properties that automatically adapt to light/dark mode
 */
export const getRiskLevelColor = (riskLevel: RiskLevel): string => {
  const riskColors: Record<RiskLevel, string> = {
    'Critical': 'var(--theme-risk-critical)',
    'High': 'var(--theme-risk-high)',
    'Medium': 'var(--theme-risk-medium)',
    'Low': 'var(--theme-risk-low)',
    'None': 'var(--theme-risk-none)'
  };
  return riskColors[riskLevel] || riskColors.None;
};

/**
 * Get the appropriate background color variable for a risk level
 */
export const getRiskLevelBackgroundColor = (riskLevel: RiskLevel): string => {
  const riskBgColors: Record<RiskLevel, string> = {
    'Critical': 'var(--theme-risk-critical-bg)',
    'High': 'var(--theme-risk-high-bg)',
    'Medium': 'var(--theme-risk-medium-bg)',
    'Low': 'var(--theme-risk-low-bg)',
    'None': 'var(--theme-risk-none-bg)'
  };
  return riskBgColors[riskLevel] || riskBgColors.None;
};

/**
 * Get the appropriate CSS color variable for a status
 */
export const getStatusColor = (status: StatusType): string => {
  const statusColors: Record<StatusType, string> = {
    'Draft': 'var(--theme-status-draft)',
    'InProgress': 'var(--theme-status-progress)',
    'UnderReview': 'var(--theme-status-review)',
    'Completed': 'var(--theme-status-complete)',
    'Overdue': 'var(--theme-status-overdue)',
    'Cancelled': 'var(--theme-status-cancelled)'
  };
  return statusColors[status] || statusColors.Draft;
};

/**
 * Get the appropriate background color variable for a status
 */
export const getStatusBackgroundColor = (status: StatusType): string => {
  const statusBgColors: Record<StatusType, string> = {
    'Draft': 'var(--theme-status-draft-bg)',
    'InProgress': 'var(--theme-status-progress-bg)',
    'UnderReview': 'var(--theme-status-review-bg)',
    'Completed': 'var(--theme-status-complete-bg)',
    'Overdue': 'var(--theme-status-overdue-bg)',
    'Cancelled': 'var(--theme-status-cancelled-bg)'
  };
  return statusBgColors[status] || statusBgColors.Draft;
};

/**
 * Get Bootstrap-compatible badge class for risk level
 * Maintains compatibility with existing CoreUI badge components
 */
export const getRiskLevelBadgeClass = (riskLevel: RiskLevel): string => {
  const badgeClasses: Record<RiskLevel, string> = {
    'Critical': 'badge-danger',
    'High': 'badge-warning',
    'Medium': 'badge-warning',
    'Low': 'badge-success',
    'None': 'badge-secondary'
  };
  return badgeClasses[riskLevel] || badgeClasses.None;
};

/**
 * Get Bootstrap-compatible badge class for status
 */
export const getStatusBadgeClass = (status: StatusType): string => {
  const badgeClasses: Record<StatusType, string> = {
    'Draft': 'badge-info',
    'InProgress': 'badge-warning',
    'UnderReview': 'badge-primary',
    'Completed': 'badge-success',
    'Overdue': 'badge-danger',
    'Cancelled': 'badge-secondary'
  };
  return badgeClasses[status] || badgeClasses.Draft;
};

/**
 * Get icon class for risk level (for color-blind accessibility)
 */
export const getRiskLevelIcon = (riskLevel: RiskLevel): string => {
  const iconClasses: Record<RiskLevel, string> = {
    'Critical': 'fa-exclamation-triangle',
    'High': 'fa-exclamation-circle',
    'Medium': 'fa-exclamation',
    'Low': 'fa-check-circle',
    'None': 'fa-info-circle'
  };
  return iconClasses[riskLevel] || iconClasses.None;
};

/**
 * Get pattern class for risk level (for color-blind accessibility)
 */
export const getRiskLevelPattern = (riskLevel: RiskLevel): string => {
  const patternClasses: Record<RiskLevel, string> = {
    'Critical': 'pattern-critical',
    'High': 'pattern-warning',
    'Medium': 'pattern-warning',
    'Low': 'pattern-success',
    'None': ''
  };
  return patternClasses[riskLevel] || '';
};

/**
 * Map audit status to generic status type
 */
export const mapAuditStatusToStatusType = (auditStatus: AuditStatus): StatusType => {
  const statusMap: Record<AuditStatus, StatusType> = {
    'Draft': 'Draft',
    'Scheduled': 'Draft',
    'InProgress': 'InProgress',
    'Completed': 'Completed',
    'Overdue': 'Overdue',
    'Cancelled': 'Cancelled'
  };
  return statusMap[auditStatus] || 'Draft';
};

/**
 * Map training status to generic status type
 */
export const mapTrainingStatusToStatusType = (trainingStatus: TrainingStatus): StatusType => {
  const statusMap: Record<TrainingStatus, StatusType> = {
    'Draft': 'Draft',
    'Scheduled': 'Draft',
    'InProgress': 'InProgress',
    'Completed': 'Completed',
    'Expired': 'Overdue',
    'Cancelled': 'Cancelled'
  };
  return statusMap[trainingStatus] || 'Draft';
};

/**
 * Map work permit status to generic status type
 */
export const mapWorkPermitStatusToStatusType = (permitStatus: WorkPermitStatus): StatusType => {
  const statusMap: Record<WorkPermitStatus, StatusType> = {
    'Draft': 'Draft',
    'Submitted': 'UnderReview',
    'UnderReview': 'UnderReview',
    'Approved': 'InProgress',
    'Rejected': 'Cancelled',
    'InProgress': 'InProgress',
    'Completed': 'Completed',
    'Expired': 'Overdue',
    'Cancelled': 'Cancelled'
  };
  return statusMap[permitStatus] || 'Draft';
};

/**
 * Get audit status color using the centralized system
 */
export const getAuditStatusColor = (status: AuditStatus): string => {
  return getStatusColor(mapAuditStatusToStatusType(status));
};

/**
 * Get training status color using the centralized system
 */
export const getTrainingStatusColor = (status: TrainingStatus): string => {
  return getStatusColor(mapTrainingStatusToStatusType(status));
};

/**
 * Get work permit status color using the centralized system
 */
export const getWorkPermitStatusColor = (status: WorkPermitStatus): string => {
  return getStatusColor(mapWorkPermitStatusToStatusType(status));
};

/**
 * Get audit status badge class
 */
export const getAuditStatusBadgeClass = (status: AuditStatus): string => {
  return getStatusBadgeClass(mapAuditStatusToStatusType(status));
};

/**
 * Get training status badge class
 */
export const getTrainingStatusBadgeClass = (status: TrainingStatus): string => {
  return getStatusBadgeClass(mapTrainingStatusToStatusType(status));
};

/**
 * Get work permit status badge class
 */
export const getWorkPermitStatusBadgeClass = (status: WorkPermitStatus): string => {
  return getStatusBadgeClass(mapWorkPermitStatusToStatusType(status));
};