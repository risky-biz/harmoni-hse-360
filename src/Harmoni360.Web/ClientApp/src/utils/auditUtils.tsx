import React from 'react';
import { CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faEdit,
  faCalendarAlt,
  faSpinner,
  faCheckCircle,
  faExclamationTriangle,
  faTimesCircle,
  faArchive,
  faEye,
  faShieldAlt,
  faIndustry,
  faWrench,
  faClipboardCheck,
  faFire,
  faFlask,
  faUsers,
  faExclamationCircle,
  faCogs,
  faLowVision,
  faBalanceScale,
  faSignalSlash,
  faCheckSquare,
  faExclamation,
  faLightbulb,
  faThumbsUp,
  faBan,
} from '@fortawesome/free-solid-svg-icons';
import { AuditStatus, AuditType, AuditPriority, FindingType, FindingSeverity, FindingStatus, AuditScore } from '../types/audit';
import { formatDistanceToNow, format } from 'date-fns';

// Status Badge Components
export const getAuditStatusBadge = (status: AuditStatus) => {
  const config = {
    'Draft': { color: 'secondary', icon: faEdit },
    'Scheduled': { color: 'info', icon: faCalendarAlt },
    'InProgress': { color: 'warning', icon: faSpinner },
    'Completed': { color: 'success', icon: faCheckCircle },
    'Overdue': { color: 'danger', icon: faExclamationTriangle },
    'Cancelled': { color: 'danger', icon: faTimesCircle },
    'Archived': { color: 'secondary', icon: faArchive },
    'UnderReview': { color: 'info', icon: faEye },
  }[status] || { color: 'secondary', icon: faEdit };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {status}
    </CBadge>
  );
};

export const getAuditTypeBadge = (type: AuditType) => {
  const config = {
    'Safety': { color: 'success', icon: faShieldAlt },
    'Environmental': { color: 'success', icon: faIndustry },
    'Equipment': { color: 'warning', icon: faWrench },
    'Compliance': { color: 'primary', icon: faClipboardCheck },
    'Fire': { color: 'danger', icon: faFire },
    'Chemical': { color: 'warning', icon: faFlask },
    'Ergonomic': { color: 'info', icon: faUsers },
    'Emergency': { color: 'danger', icon: faExclamationCircle },
    'Management': { color: 'primary', icon: faCogs },
    'Process': { color: 'info', icon: faCogs },
  }[type] || { color: 'secondary', icon: faClipboardCheck };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {type}
    </CBadge>
  );
};

export const getAuditPriorityBadge = (priority: AuditPriority) => {
  const config = {
    'Low': { color: 'success', icon: faLowVision },
    'Medium': { color: 'warning', icon: faBalanceScale },
    'High': { color: 'danger', icon: faExclamationTriangle },
    'Critical': { color: 'danger', icon: faSignalSlash },
  }[priority] || { color: 'secondary', icon: faBalanceScale };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {priority}
    </CBadge>
  );
};

export const getAuditScoreBadge = (score: AuditScore) => {
  const config = {
    'Excellent': { color: 'success', text: 'Excellent (90-100%)', icon: faThumbsUp },
    'Good': { color: 'info', text: 'Good (80-89%)', icon: faCheckCircle },
    'Satisfactory': { color: 'warning', text: 'Satisfactory (70-79%)', icon: faCheckSquare },
    'NeedsImprovement': { color: 'warning', text: 'Needs Improvement (60-69%)', icon: faExclamation },
    'Unsatisfactory': { color: 'danger', text: 'Unsatisfactory (<60%)', icon: faBan },
  }[score] || { color: 'secondary', text: 'Not Scored', icon: faCheckSquare };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {config.text}
    </CBadge>
  );
};

export const getFindingTypeBadge = (type: FindingType) => {
  const config = {
    'NonConformance': { color: 'danger', icon: faTimesCircle, text: 'Non-Conformance' },
    'Observation': { color: 'info', icon: faEye, text: 'Observation' },
    'OpportunityForImprovement': { color: 'warning', icon: faLightbulb, text: 'Opportunity' },
    'PositiveFinding': { color: 'success', icon: faThumbsUp, text: 'Positive Finding' },
    'CriticalNonConformance': { color: 'danger', icon: faSignalSlash, text: 'Critical Non-Conformance' },
  }[type] || { color: 'secondary', icon: faEye, text: type };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {config.text}
    </CBadge>
  );
};

export const getFindingSeverityBadge = (severity: FindingSeverity) => {
  const config = {
    'Minor': { color: 'success', icon: faCheckCircle },
    'Moderate': { color: 'warning', icon: faExclamation },
    'Major': { color: 'danger', icon: faExclamationTriangle },
    'Critical': { color: 'danger', icon: faSignalSlash },
  }[severity] || { color: 'secondary', icon: faCheckCircle };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {severity}
    </CBadge>
  );
};

export const getFindingStatusBadge = (status: FindingStatus) => {
  const config = {
    'Open': { color: 'danger', icon: faExclamationCircle },
    'InProgress': { color: 'warning', icon: faSpinner },
    'Resolved': { color: 'info', icon: faCheckSquare },
    'Verified': { color: 'success', icon: faCheckCircle },
    'Closed': { color: 'secondary', icon: faArchive },
  }[status] || { color: 'secondary', icon: faExclamationCircle };

  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center">
      <FontAwesomeIcon icon={config.icon} className="me-1" />
      {status}
    </CBadge>
  );
};

// Utility Functions
export const formatDate = (date: string | Date) => {
  return format(new Date(date), 'MMM dd, yyyy');
};

export const formatDateTime = (date: string | Date) => {
  return format(new Date(date), 'MMM dd, yyyy HH:mm');
};

export const formatDateTimeShort = (date: string | Date) => {
  return format(new Date(date), 'dd/MM/yyyy HH:mm');
};

export const formatRelativeTime = (date: string | Date) => {
  return formatDistanceToNow(new Date(date), { addSuffix: true });
};

export const isAuditOverdue = (audit: any) => {
  if (!audit.scheduledDate || audit.status === 'Completed' || audit.status === 'Cancelled') {
    return false;
  }
  return new Date(audit.scheduledDate) < new Date() && audit.status !== 'Completed';
};

export const isAuditExpiringSoon = (audit: any, daysThreshold = 7) => {
  if (!audit.scheduledDate || audit.status === 'Completed' || audit.status === 'Cancelled') {
    return false;
  }
  const scheduledDate = new Date(audit.scheduledDate);
  const warningDate = new Date();
  warningDate.setDate(warningDate.getDate() + daysThreshold);
  return scheduledDate <= warningDate && scheduledDate >= new Date();
};

export const getAuditProgress = (audit: any) => {
  if (!audit.items || audit.items.length === 0) {
    return 0;
  }
  
  const completedItems = audit.items.filter((item: any) => 
    item.status === 'Completed' || item.status === 'NonCompliant' || item.status === 'NotApplicable'
  ).length;
  
  return Math.round((completedItems / audit.items.length) * 100);
};

export const calculateAuditScore = (audit: any) => {
  if (!audit.items || audit.items.length === 0) {
    return null;
  }
  
  const scoredItems = audit.items.filter((item: any) => 
    item.actualPoints !== null && item.maxPoints !== null && item.maxPoints > 0
  );
  
  if (scoredItems.length === 0) {
    return null;
  }
  
  const totalPoints = scoredItems.reduce((sum: number, item: any) => sum + item.actualPoints, 0);
  const maxPoints = scoredItems.reduce((sum: number, item: any) => sum + item.maxPoints, 0);
  
  return Math.round((totalPoints / maxPoints) * 100);
};

export const getComplianceRate = (audit: any) => {
  if (!audit.items || audit.items.length === 0) {
    return null;
  }
  
  const assessedItems = audit.items.filter((item: any) => 
    item.isCompliant !== null && item.status !== 'NotApplicable'
  );
  
  if (assessedItems.length === 0) {
    return null;
  }
  
  const compliantItems = assessedItems.filter((item: any) => item.isCompliant === true).length;
  
  return Math.round((compliantItems / assessedItems.length) * 100);
};

export const getFindingsSummary = (findings: any[]) => {
  if (!findings || findings.length === 0) {
    return { total: 0, open: 0, critical: 0, major: 0, moderate: 0, minor: 0 };
  }
  
  return {
    total: findings.length,
    open: findings.filter(f => f.status === 'Open' || f.status === 'InProgress').length,
    critical: findings.filter(f => f.severity === 'Critical').length,
    major: findings.filter(f => f.severity === 'Major').length,
    moderate: findings.filter(f => f.severity === 'Moderate').length,
    minor: findings.filter(f => f.severity === 'Minor').length,
  };
};

export const canEditAudit = (audit: any) => {
  return audit.status === 'Draft' || audit.status === 'Scheduled';
};

export const canStartAudit = (audit: any) => {
  return audit.status === 'Scheduled';
};

export const canCompleteAudit = (audit: any) => {
  return audit.status === 'InProgress';
};

export const canCancelAudit = (audit: any) => {
  return ['Draft', 'Scheduled', 'InProgress'].includes(audit.status);
};

export const canDeleteAudit = (audit: any) => {
  return audit.status === 'Draft';
};