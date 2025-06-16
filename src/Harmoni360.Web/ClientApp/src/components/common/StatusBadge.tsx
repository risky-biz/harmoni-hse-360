import React from 'react';
import { CBadge } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { 
  faExclamationTriangle, 
  faExclamationCircle, 
  faExclamation, 
  faCheckCircle, 
  faInfoCircle,
  faClock,
  faEye,
  faCheck,
  faTimes
} from '@fortawesome/free-solid-svg-icons';
import { 
  RiskLevel, 
  StatusType,
  getRiskLevelColor,
  getStatusColor,
  getRiskLevelBadgeClass,
  getStatusBadgeClass 
} from '../../utils/statusColors';

// Risk Badge Props
interface RiskBadgeProps {
  level: RiskLevel;
  showIcon?: boolean;
  showPattern?: boolean;
  size?: 'sm' | 'md' | 'lg';
  pill?: boolean;
  outline?: boolean;
  className?: string;
  children?: React.ReactNode;
}

// Status Badge Props
interface StatusBadgeProps {
  status: StatusType;
  showIcon?: boolean;
  size?: 'sm' | 'md' | 'lg';
  pill?: boolean;
  outline?: boolean;
  className?: string;
  children?: React.ReactNode;
}

// Icon mappings for risk levels
const getRiskIcon = (level: RiskLevel): IconDefinition => {
  const iconMap: Record<RiskLevel, IconDefinition> = {
    'Critical': faExclamationTriangle,
    'High': faExclamationCircle,
    'Medium': faExclamation,
    'Low': faCheckCircle,
    'None': faInfoCircle
  };
  return iconMap[level];
};

// Icon mappings for status types
const getStatusIcon = (status: StatusType): IconDefinition => {
  const iconMap: Record<StatusType, IconDefinition> = {
    'Draft': faInfoCircle,
    'InProgress': faClock,
    'UnderReview': faEye,
    'Completed': faCheck,
    'Overdue': faExclamationTriangle,
    'Cancelled': faTimes
  };
  return iconMap[status];
};

// Risk Badge Component
export const RiskBadge: React.FC<RiskBadgeProps> = ({
  level,
  showIcon = true,
  showPattern = false,
  size = 'md',
  pill = false,
  outline = false,
  className = '',
  children
}) => {
  const badgeClass = getRiskLevelBadgeClass(level);
  const icon = showIcon ? getRiskIcon(level) : null;
  
  // Build CSS classes
  const cssClasses = [
    badgeClass,
    size === 'sm' ? 'badge-sm' : size === 'lg' ? 'badge-lg' : '',
    pill ? 'badge-pill' : '',
    outline ? `badge-outline-${badgeClass.replace('badge-', '')}` : '',
    showPattern ? `pattern-${level.toLowerCase()}` : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <CBadge 
      className={cssClasses}
      style={{
        backgroundColor: outline ? 'transparent' : getRiskLevelColor(level),
        borderColor: getRiskLevelColor(level)
      }}
      aria-label={`Risk level: ${level}`}
    >
      {icon && (
        <FontAwesomeIcon 
          icon={icon} 
          className="me-1" 
          aria-hidden="true"
        />
      )}
      {children || level}
    </CBadge>
  );
};

// Status Badge Component
export const StatusBadge: React.FC<StatusBadgeProps> = ({
  status,
  showIcon = true,
  size = 'md',
  pill = false,
  outline = false,
  className = '',
  children
}) => {
  const badgeClass = getStatusBadgeClass(status);
  const icon = showIcon ? getStatusIcon(status) : null;
  
  // Format status text for display
  const formatStatusText = (status: StatusType): string => {
    switch (status) {
      case 'InProgress':
        return 'In Progress';
      case 'UnderReview':
        return 'Under Review';
      default:
        return status;
    }
  };

  // Build CSS classes
  const cssClasses = [
    badgeClass,
    size === 'sm' ? 'badge-sm' : size === 'lg' ? 'badge-lg' : '',
    pill ? 'badge-pill' : '',
    outline ? `badge-outline-${badgeClass.replace('badge-', '')}` : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <CBadge 
      className={cssClasses}
      style={{
        backgroundColor: outline ? 'transparent' : getStatusColor(status),
        borderColor: getStatusColor(status)
      }}
      aria-label={`Status: ${formatStatusText(status)}`}
    >
      {icon && (
        <FontAwesomeIcon 
          icon={icon} 
          className="me-1" 
          aria-hidden="true"
        />
      )}
      {children || formatStatusText(status)}
    </CBadge>
  );
};

// Generic Badge Props for custom badges
interface CustomBadgeProps {
  color: string;
  backgroundColor?: string;
  text: string;
  icon?: IconDefinition;
  size?: 'sm' | 'md' | 'lg';
  pill?: boolean;
  outline?: boolean;
  className?: string;
  children?: React.ReactNode;
}

// Custom Badge Component for special cases
export const CustomBadge: React.FC<CustomBadgeProps> = ({
  color,
  backgroundColor,
  text,
  icon,
  size = 'md',
  pill = false,
  outline = false,
  className = '',
  children
}) => {
  // Build CSS classes
  const cssClasses = [
    'badge',
    size === 'sm' ? 'badge-sm' : size === 'lg' ? 'badge-lg' : '',
    pill ? 'badge-pill' : '',
    className
  ].filter(Boolean).join(' ');

  const badgeStyle = outline 
    ? {
        backgroundColor: 'transparent',
        color: color,
        border: `1px solid ${color}`
      }
    : {
        backgroundColor: backgroundColor || color,
        color: 'white'
      };

  return (
    <CBadge 
      className={cssClasses}
      style={badgeStyle}
      aria-label={text}
    >
      {icon && (
        <FontAwesomeIcon 
          icon={icon} 
          className="me-1" 
          aria-hidden="true"
        />
      )}
      {children || text}
    </CBadge>
  );
};

// Priority Badge for different priority levels
interface PriorityBadgeProps {
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  showIcon?: boolean;
  size?: 'sm' | 'md' | 'lg';
  pill?: boolean;
  outline?: boolean;
  className?: string;
}

export const PriorityBadge: React.FC<PriorityBadgeProps> = ({
  priority,
  showIcon = true,
  size = 'md',
  pill = false,
  outline = false,
  className = ''
}) => {
  // Map priority to risk level for consistent colors
  const riskLevel: RiskLevel = priority === 'Critical' ? 'Critical' :
                               priority === 'High' ? 'High' :
                               priority === 'Medium' ? 'Medium' : 'Low';

  return (
    <RiskBadge
      level={riskLevel}
      showIcon={showIcon}
      size={size}
      pill={pill}
      outline={outline}
      className={className}
    >
      {priority} Priority
    </RiskBadge>
  );
};

// Emergency Badge for critical emergency situations
interface EmergencyBadgeProps {
  text?: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
  animated?: boolean;
}

export const EmergencyBadge: React.FC<EmergencyBadgeProps> = ({
  text = 'EMERGENCY',
  size = 'lg',
  className = '',
  animated = true
}) => {
  const cssClasses = [
    'badge-danger',
    size === 'sm' ? 'badge-sm' : size === 'lg' ? 'badge-lg' : '',
    animated ? 'badge-emergency-pulse' : '',
    className
  ].filter(Boolean).join(' ');

  return (
    <CBadge 
      className={cssClasses}
      style={{
        backgroundColor: 'var(--theme-emergency-bg)',
        color: 'var(--theme-emergency-text)',
        fontWeight: 'bold',
        textTransform: 'uppercase',
        letterSpacing: '0.5px'
      }}
      aria-label={`Emergency: ${text}`}
      role="alert"
    >
      <FontAwesomeIcon 
        icon={faExclamationTriangle} 
        className="me-1" 
        aria-hidden="true"
      />
      {text}
    </CBadge>
  );
};

// Exports
export default StatusBadge;

// Add emergency pulse animation styles
if (typeof document !== 'undefined' && !document.getElementById('badge-emergency-styles')) {
  const styleElement = document.createElement('style');
  styleElement.id = 'badge-emergency-styles';
  styleElement.textContent = `
    .badge-emergency-pulse {
      animation: emergency-pulse 1.5s infinite;
    }

    @keyframes emergency-pulse {
      0% {
        transform: scale(1);
        opacity: 1;
      }
      50% {
        transform: scale(1.05);
        opacity: 0.9;
      }
      100% {
        transform: scale(1);
        opacity: 1;
      }
    }
  `;
  document.head.appendChild(styleElement);
}