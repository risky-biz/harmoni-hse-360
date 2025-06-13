import { CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faExclamationTriangle, 
  faShieldAlt, 
  faCheckCircle, 
  faTimesCircle,
  faExclamationCircle,
  faClock,
  faQuestion
} from '@fortawesome/free-solid-svg-icons';

// Standardized color schemes for risk levels (following 5x5 matrix)
export const RISK_LEVEL_CONFIG = {
  VeryLow: {
    color: 'success' as const,
    label: 'Very Low',
    icon: faCheckCircle,
    description: 'Acceptable risk',
  },
  Low: {
    color: 'info' as const,
    label: 'Low',
    icon: faShieldAlt,
    description: 'Tolerable risk',
  },
  Medium: {
    color: 'warning' as const,
    label: 'Medium',
    icon: faExclamationCircle,
    description: 'Moderate risk',
  },
  High: {
    color: 'danger' as const,
    label: 'High',
    icon: faExclamationTriangle,
    description: 'High risk',
  },
  Critical: {
    color: 'dark' as const,
    label: 'Critical',
    icon: faTimesCircle,
    description: 'Unacceptable risk',
  },
};

// Assessment type configurations
export const ASSESSMENT_TYPE_CONFIG = {
  General: {
    color: 'primary' as const,
    label: 'General',
    description: 'Standard risk assessment',
  },
  HIRA: {
    color: 'warning' as const,
    label: 'HIRA',
    description: 'Hazard Identification & Risk Assessment',
  },
  JSA: {
    color: 'info' as const,
    label: 'JSA',
    description: 'Job Safety Analysis',
  },
  Environmental: {
    color: 'success' as const,
    label: 'Environmental',
    description: 'Environmental impact assessment',
  },
  Fire: {
    color: 'danger' as const,
    label: 'Fire Safety',
    description: 'Fire risk assessment',
  },
};

// Approval status configurations
export const APPROVAL_STATUS_CONFIG = {
  Approved: {
    color: 'success' as const,
    label: 'Approved',
    icon: faCheckCircle,
  },
  PendingApproval: {
    color: 'warning' as const,
    label: 'Pending Approval',
    icon: faClock,
  },
  Draft: {
    color: 'secondary' as const,
    label: 'Draft',
    icon: faQuestion,
  },
};

/**
 * Returns a standardized risk level badge
 */
export const getRiskLevelBadge = (riskLevel: string) => {
  const config = RISK_LEVEL_CONFIG[riskLevel as keyof typeof RISK_LEVEL_CONFIG];
  
  if (!config) {
    return (
      <CBadge color="secondary">
        {riskLevel}
      </CBadge>
    );
  }

  return (
    <CBadge color={config.color} className="d-flex align-items-center gap-1">
      <FontAwesomeIcon icon={config.icon} size="xs" />
      {config.label}
    </CBadge>
  );
};

/**
 * Returns a standardized assessment type badge
 */
export const getAssessmentTypeBadge = (assessmentType: string) => {
  const config = ASSESSMENT_TYPE_CONFIG[assessmentType as keyof typeof ASSESSMENT_TYPE_CONFIG];
  
  if (!config) {
    return (
      <CBadge color="secondary">
        {assessmentType}
      </CBadge>
    );
  }

  return (
    <CBadge color={config.color}>
      {config.label}
    </CBadge>
  );
};

/**
 * Returns a standardized approval status badge
 */
export const getApprovalStatusBadge = (isApproved: boolean) => {
  const status = isApproved ? 'Approved' : 'PendingApproval';
  const config = APPROVAL_STATUS_CONFIG[status];

  return (
    <CBadge color={config.color} className="d-flex align-items-center gap-1">
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
 * Formats a date to short format (date only)
 */
export const formatDateShort = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  } catch {
    return 'Invalid Date';
  }
};

/**
 * Returns appropriate color for risk level
 */
export const getRiskLevelColor = (riskLevel: string): string => {
  const config = RISK_LEVEL_CONFIG[riskLevel as keyof typeof RISK_LEVEL_CONFIG];
  return config?.color || 'secondary';
};

/**
 * Returns appropriate color for assessment type
 */
export const getAssessmentTypeColor = (assessmentType: string): string => {
  const config = ASSESSMENT_TYPE_CONFIG[assessmentType as keyof typeof ASSESSMENT_TYPE_CONFIG];
  return config?.color || 'secondary';
};

/**
 * Calculate risk score color based on 5x5 matrix
 */
export const getRiskScoreColor = (riskScore: number): string => {
  if (riskScore >= 17) return 'danger'; // Critical (17-25)
  if (riskScore >= 10) return 'warning'; // High (10-16)
  if (riskScore >= 5) return 'info'; // Medium (5-9)
  if (riskScore >= 1) return 'success'; // Low (1-4)
  return 'secondary'; // Unknown
};

/**
 * Get risk score badge with appropriate styling
 */
export const getRiskScoreBadge = (riskScore: number) => {
  const color = getRiskScoreColor(riskScore);
  return (
    <CBadge color={color} shape="rounded-pill">
      {riskScore}
    </CBadge>
  );
};

/**
 * Check if assessment is due for review
 */
export const isAssessmentDueForReview = (nextReviewDate: string): boolean => {
  try {
    const reviewDate = new Date(nextReviewDate);
    const today = new Date();
    return reviewDate <= today;
  } catch {
    return false;
  }
};

/**
 * Get days until review
 */
export const getDaysUntilReview = (nextReviewDate: string): number => {
  try {
    const reviewDate = new Date(nextReviewDate);
    const today = new Date();
    const timeDiff = reviewDate.getTime() - today.getTime();
    return Math.ceil(timeDiff / (1000 * 3600 * 24));
  } catch {
    return 0;
  }
};

/**
 * Get review status badge
 */
export const getReviewStatusBadge = (nextReviewDate: string) => {
  const daysUntil = getDaysUntilReview(nextReviewDate);
  
  if (daysUntil < 0) {
    return (
      <CBadge color="danger" className="d-flex align-items-center gap-1">
        <FontAwesomeIcon icon={faExclamationTriangle} size="xs" />
        Overdue
      </CBadge>
    );
  } else if (daysUntil <= 7) {
    return (
      <CBadge color="warning" className="d-flex align-items-center gap-1">
        <FontAwesomeIcon icon={faClock} size="xs" />
        Due Soon
      </CBadge>
    );
  } else {
    return (
      <CBadge color="success" className="d-flex align-items-center gap-1">
        <FontAwesomeIcon icon={faCheckCircle} size="xs" />
        Current
      </CBadge>
    );
  }
};

/**
 * Assessment type descriptions for tooltips
 */
export const getAssessmentTypeDescription = (assessmentType: string): string => {
  const config = ASSESSMENT_TYPE_CONFIG[assessmentType as keyof typeof ASSESSMENT_TYPE_CONFIG];
  return config?.description || assessmentType;
};

/**
 * Risk level descriptions for tooltips
 */
export const getRiskLevelDescription = (riskLevel: string): string => {
  const config = RISK_LEVEL_CONFIG[riskLevel as keyof typeof RISK_LEVEL_CONFIG];
  return config?.description || riskLevel;
};