import { CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { SEVERITY_ICONS, STATUS_ICONS } from './iconMappings';

// Standardized color schemes for severity levels
export const SEVERITY_CONFIG = {
  Critical: { 
    color: 'danger' as const, 
    label: 'Critical',
    icon: SEVERITY_ICONS.Critical
  },
  Serious: { 
    color: 'warning' as const, 
    label: 'Serious',
    icon: SEVERITY_ICONS.Serious
  },
  Moderate: { 
    color: 'info' as const, 
    label: 'Moderate',
    icon: SEVERITY_ICONS.Moderate
  },
  Minor: { 
    color: 'success' as const, 
    label: 'Minor',
    icon: SEVERITY_ICONS.Minor
  }
};

// Standardized color schemes for status levels
export const STATUS_CONFIG = {
  Reported: { 
    color: 'primary' as const, 
    label: 'Reported',
    icon: STATUS_ICONS.Reported
  },
  UnderInvestigation: { 
    color: 'warning' as const, 
    label: 'Under Investigation',
    icon: STATUS_ICONS.UnderInvestigation
  },
  AwaitingAction: { 
    color: 'danger' as const, 
    label: 'Awaiting Action',
    icon: STATUS_ICONS.AwaitingAction
  },
  Resolved: { 
    color: 'success' as const, 
    label: 'Resolved',
    icon: STATUS_ICONS.Resolved
  },
  Closed: { 
    color: 'secondary' as const, 
    label: 'Closed',
    icon: STATUS_ICONS.Closed
  }
};

// Utility function to get severity badge
export const getSeverityBadge = (severity: string) => {
  const config = SEVERITY_CONFIG[severity as keyof typeof SEVERITY_CONFIG];
  if (!config) {
    return <CBadge color="secondary">{severity}</CBadge>;
  }
  
  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center gap-1">
      <FontAwesomeIcon icon={config.icon} size="sm" />
      <span>{config.label}</span>
    </CBadge>
  );
};

// Utility function to get status badge
export const getStatusBadge = (status: string) => {
  const config = STATUS_CONFIG[status as keyof typeof STATUS_CONFIG];
  if (!config) {
    return <CBadge color="secondary">{status}</CBadge>;
  }
  
  return (
    <CBadge color={config.color} className="d-inline-flex align-items-center gap-1">
      <FontAwesomeIcon icon={config.icon} size="sm" />
      <span>{config.label}</span>
    </CBadge>
  );
};

// Get just the color for a severity (for use in other components)
export const getSeverityColor = (severity: string): string => {
  const config = SEVERITY_CONFIG[severity as keyof typeof SEVERITY_CONFIG];
  return config?.color || 'secondary';
};

// Get just the color for a status (for use in other components)
export const getStatusColor = (status: string): string => {
  const config = STATUS_CONFIG[status as keyof typeof STATUS_CONFIG];
  return config?.color || 'secondary';
};

// Get icon for severity
export const getSeverityIcon = (severity: string) => {
  const config = SEVERITY_CONFIG[severity as keyof typeof SEVERITY_CONFIG];
  return config?.icon || SEVERITY_ICONS.Critical;
};

// Get icon for status
export const getStatusIcon = (status: string) => {
  const config = STATUS_CONFIG[status as keyof typeof STATUS_CONFIG];
  return config?.icon || STATUS_ICONS.Reported;
};

// Re-export date utilities with standardized format
export { formatDate, formatDateOnly, formatDateTime, formatRelativeTime } from './dateUtils';