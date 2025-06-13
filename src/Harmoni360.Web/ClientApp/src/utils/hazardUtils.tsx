import { CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { HAZARD_ICONS } from './iconMappings';

// Standardized color schemes for hazard severity levels
export const HAZARD_SEVERITY_CONFIG = {
  Catastrophic: {
    color: 'danger' as const,
    label: 'Catastrophic',
    icon: HAZARD_ICONS.warning,
  },
  Major: {
    color: 'warning' as const,
    label: 'Major',
    icon: HAZARD_ICONS.warning,
  },
  Moderate: {
    color: 'info' as const,
    label: 'Moderate',
    icon: HAZARD_ICONS.general,
  },
  Minor: {
    color: 'success' as const,
    label: 'Minor',
    icon: HAZARD_ICONS.reporting,
  },
  Negligible: {
    color: 'light' as const,
    label: 'Negligible',
    icon: HAZARD_ICONS.general,
  },
};

// Standardized color schemes for hazard status levels
export const HAZARD_STATUS_CONFIG = {
  Reported: {
    color: 'primary' as const,
    label: 'Reported',
    icon: HAZARD_ICONS.reporting,
  },
  UnderAssessment: {
    color: 'info' as const,
    label: 'Under Assessment',
    icon: HAZARD_ICONS.general,
  },
  ActionRequired: {
    color: 'danger' as const,
    label: 'Action Required',
    icon: HAZARD_ICONS.warning,
  },
  Mitigating: {
    color: 'warning' as const,
    label: 'Mitigating',
    icon: HAZARD_ICONS.general,
  },
  Monitoring: {
    color: 'info' as const,
    label: 'Monitoring',
    icon: HAZARD_ICONS.general,
  },
  Resolved: {
    color: 'success' as const,
    label: 'Resolved',
    icon: HAZARD_ICONS.reporting,
  },
  Closed: {
    color: 'secondary' as const,
    label: 'Closed',
    icon: HAZARD_ICONS.general,
  },
};

/**
 * Returns a standardized severity badge for hazards
 */
export const getSeverityBadge = (severity: string) => {
  const config = HAZARD_SEVERITY_CONFIG[severity as keyof typeof HAZARD_SEVERITY_CONFIG];
  
  if (!config) {
    return (
      <CBadge color="secondary" className="hazard-badge">
        {severity}
      </CBadge>
    );
  }

  return (
    <CBadge 
      color={config.color} 
      className="d-flex align-items-center gap-1 hazard-badge"
      style={{
        backgroundColor: config.color === 'light' ? '#f8f9fa !important' : undefined,
        color: config.color === 'light' ? '#495057 !important' : undefined,
        border: config.color === 'light' ? '1px solid #dee2e6' : undefined
      }}
    >
      <FontAwesomeIcon icon={config.icon} size="xs" />
      {config.label}
    </CBadge>
  );
};

/**
 * Returns a standardized status badge for hazards
 */
export const getStatusBadge = (status: string) => {
  const config = HAZARD_STATUS_CONFIG[status as keyof typeof HAZARD_STATUS_CONFIG];
  
  if (!config) {
    return (
      <CBadge color="secondary" className="hazard-badge">
        {status}
      </CBadge>
    );
  }

  return (
    <CBadge 
      color={config.color} 
      className="d-flex align-items-center gap-1 hazard-badge"
      style={{
        backgroundColor: config.color === 'light' ? '#f8f9fa !important' : undefined,
        color: config.color === 'light' ? '#495057 !important' : undefined,
        border: config.color === 'light' ? '1px solid #dee2e6' : undefined
      }}
    >
      <FontAwesomeIcon icon={config.icon} size="xs" />
      {config.label}
    </CBadge>
  );
};

/**
 * Formats a date to a user-friendly string
 */
export const formatDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return 'Invalid Date';
  }
};

/**
 * Returns appropriate color for severity
 */
export const getSeverityColor = (severity: string): string => {
  const config = HAZARD_SEVERITY_CONFIG[severity as keyof typeof HAZARD_SEVERITY_CONFIG];
  return config?.color || 'secondary';
};

/**
 * Returns appropriate color for status
 */
export const getStatusColor = (status: string): string => {
  const config = HAZARD_STATUS_CONFIG[status as keyof typeof HAZARD_STATUS_CONFIG];
  return config?.color || 'secondary';
};