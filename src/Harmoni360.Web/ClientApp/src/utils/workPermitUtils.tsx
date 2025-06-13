import { CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faClipboardCheck,
  faPlay,
  faCheckCircle,
  faTimesCircle,
  faPause,
  faExclamationTriangle,
  faHourglass,
  faClock,
  faShieldAlt,
  faHardHat,
  faFire,
  faRadiation,
  faIndustry,
  faWrench
} from '@fortawesome/free-solid-svg-icons';

// Standardized color schemes for work permit statuses
export const WORK_PERMIT_STATUS_CONFIG = {
  Draft: {
    color: 'secondary' as const,
    label: 'Draft',
    icon: faClipboardCheck,
  },
  Submitted: {
    color: 'info' as const,
    label: 'Submitted',
    icon: faHourglass,
  },
  Approved: {
    color: 'success' as const,
    label: 'Approved',
    icon: faCheckCircle,
  },
  Rejected: {
    color: 'danger' as const,
    label: 'Rejected',
    icon: faTimesCircle,
  },
  InProgress: {
    color: 'warning' as const,
    label: 'In Progress',
    icon: faPlay,
  },
  Completed: {
    color: 'success' as const,
    label: 'Completed',
    icon: faCheckCircle,
  },
  Cancelled: {
    color: 'dark' as const,
    label: 'Cancelled',
    icon: faTimesCircle,
  },
  Expired: {
    color: 'danger' as const,
    label: 'Expired',
    icon: faClock,
  },
};

// Standardized color schemes for work permit types
export const WORK_PERMIT_TYPE_CONFIG = {
  HotWork: {
    color: 'danger' as const,
    label: 'Hot Work',
    icon: faFire,
  },
  ConfinedSpace: {
    color: 'warning' as const,
    label: 'Confined Space',
    icon: faShieldAlt,
  },
  WorkingAtHeight: {
    color: 'info' as const,
    label: 'Working at Height',
    icon: faHardHat,
  },
  Electrical: {
    color: 'primary' as const,
    label: 'Electrical',
    icon: faIndustry,
  },
  Mechanical: {
    color: 'dark' as const,
    label: 'Mechanical',
    icon: faWrench,
  },
  Chemical: {
    color: 'warning' as const,
    label: 'Chemical',
    icon: faRadiation,
  },
  General: {
    color: 'secondary' as const,
    label: 'General',
    icon: faClipboardCheck,
  },
};

// Standardized color schemes for risk levels
export const RISK_LEVEL_CONFIG = {
  Low: {
    color: 'success' as const,
    label: 'Low Risk',
    icon: faCheckCircle,
  },
  Medium: {
    color: 'warning' as const,
    label: 'Medium Risk',
    icon: faExclamationTriangle,
  },
  High: {
    color: 'danger' as const,
    label: 'High Risk',
    icon: faExclamationTriangle,
  },
};

// Utility function to get work permit status badge
export const getWorkPermitStatusBadge = (status: string) => {
  const config = WORK_PERMIT_STATUS_CONFIG[status as keyof typeof WORK_PERMIT_STATUS_CONFIG];
  if (!config) {
    return <CBadge color="secondary">{status}</CBadge>;
  }

  return (
    <CBadge
      color={config.color}
      className="d-inline-flex align-items-center gap-1"
    >
      <FontAwesomeIcon icon={config.icon} size="sm" />
      <span>{config.label}</span>
    </CBadge>
  );
};

// Utility function to get work permit type badge
export const getWorkPermitTypeBadge = (type: string) => {
  const config = WORK_PERMIT_TYPE_CONFIG[type as keyof typeof WORK_PERMIT_TYPE_CONFIG];
  if (!config) {
    return <CBadge color="secondary">{type}</CBadge>;
  }

  return (
    <CBadge
      color={config.color}
      className="d-inline-flex align-items-center gap-1"
    >
      <FontAwesomeIcon icon={config.icon} size="sm" />
      <span>{config.label}</span>
    </CBadge>
  );
};

// Utility function to get risk level badge
export const getRiskLevelBadge = (riskLevel: string) => {
  const config = RISK_LEVEL_CONFIG[riskLevel as keyof typeof RISK_LEVEL_CONFIG];
  if (!config) {
    return <CBadge color="secondary">{riskLevel}</CBadge>;
  }

  return (
    <CBadge
      color={config.color}
      className="d-inline-flex align-items-center gap-1"
    >
      <FontAwesomeIcon icon={config.icon} size="sm" />
      <span>{config.label}</span>
    </CBadge>
  );
};

// Get just the color for a status (for use in other components)
export const getWorkPermitStatusColor = (status: string): string => {
  const config = WORK_PERMIT_STATUS_CONFIG[status as keyof typeof WORK_PERMIT_STATUS_CONFIG];
  return config?.color || 'secondary';
};

// Get just the color for a type (for use in other components)
export const getWorkPermitTypeColor = (type: string): string => {
  const config = WORK_PERMIT_TYPE_CONFIG[type as keyof typeof WORK_PERMIT_TYPE_CONFIG];
  return config?.color || 'secondary';
};

// Get icon for status
export const getWorkPermitStatusIcon = (status: string) => {
  const config = WORK_PERMIT_STATUS_CONFIG[status as keyof typeof WORK_PERMIT_STATUS_CONFIG];
  return config?.icon || faClipboardCheck;
};

// Get icon for type
export const getWorkPermitTypeIcon = (type: string) => {
  const config = WORK_PERMIT_TYPE_CONFIG[type as keyof typeof WORK_PERMIT_TYPE_CONFIG];
  return config?.icon || faClipboardCheck;
};

// Check if work permit is expired
export const isWorkPermitExpired = (validUntil: string | undefined | null): boolean => {
  if (!validUntil) return false;
  return new Date(validUntil) < new Date();
};

// Check if work permit is expiring soon (within 24 hours)
export const isWorkPermitExpiringSoon = (validUntil: string | undefined | null): boolean => {
  if (!validUntil) return false;
  const expiryDate = new Date(validUntil);
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  return expiryDate > new Date() && expiryDate <= tomorrow;
};

// Re-export date utilities with standardized format
export {
  formatDate,
  formatDateOnly,
  formatDateTime,
  formatRelativeTime,
} from './dateUtils';