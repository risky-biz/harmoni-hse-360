import React from 'react';
import {
  CBadge,
  CTooltip
} from '@coreui/react';
import { MedicalConditionDto, MedicalConditionSeverity } from '../../types/health';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faExclamationTriangle,
  faHeartbeat
} from '@fortawesome/free-solid-svg-icons';

interface MedicalConditionBadgeProps {
  condition: MedicalConditionDto;
  showTooltip?: boolean;
  size?: 'sm' | 'lg';
  variant?: 'normal' | 'minimal' | 'emergency';
}

const MedicalConditionBadge: React.FC<MedicalConditionBadgeProps> = ({
  condition,
  showTooltip = true,
  size,
  variant = 'normal'
}) => {
  const getSeverityColor = (severity: MedicalConditionSeverity) => {
    switch (severity) {
      case MedicalConditionSeverity.Critical: return 'danger';
      case MedicalConditionSeverity.High: return 'warning';
      case MedicalConditionSeverity.Medium: return 'info';
      case MedicalConditionSeverity.Low: return 'success';
      default: return 'secondary';
    }
  };

  const getSeverityIcon = (severity: MedicalConditionSeverity) => {
    switch (severity) {
      case MedicalConditionSeverity.Critical:
      case MedicalConditionSeverity.High:
        return faExclamationTriangle;
      default:
        return faHeartbeat;
    }
  };

  const getBadgeContent = () => {
    switch (variant) {
      case 'minimal':
        return (
          <>
            <FontAwesomeIcon 
              icon={getSeverityIcon(condition.severity)} 
              size="sm" 
               
              className="me-1" 
            />
            {condition.name}
          </>
        );
      
      case 'emergency':
        return condition.requiresEmergencyAction ? (
          <>
            <FontAwesomeIcon icon={faExclamationTriangle} size="sm"  className="me-1" />
            EMERGENCY: {condition.name}
          </>
        ) : null;
      
      default:
        return (
          <>
            <FontAwesomeIcon 
              icon={getSeverityIcon(condition.severity)} 
              size="sm" 
               
              className="me-1" 
            />
            {condition.name} ({condition.severity})
          </>
        );
    }
  };

  const getTooltipContent = () => {
    return (
      <div>
        <div><strong>{condition.name}</strong></div>
        <div className="small">Type: {condition.type}</div>
        <div className="small">Severity: {condition.severity}</div>
        {condition.description && (
          <div className="small mt-1">{condition.description}</div>
        )}
        {condition.requiresEmergencyAction && (
          <div className="small mt-1 text-warning">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
            Requires Emergency Action
          </div>
        )}
        {condition.medicationRequired && (
          <div className="small mt-1">
            <strong>Medication:</strong> {condition.medicationRequired}
          </div>
        )}
        {condition.emergencyProtocol && (
          <div className="small mt-1">
            <strong>Emergency Protocol:</strong> {condition.emergencyProtocol}
          </div>
        )}
      </div>
    );
  };

  // Don't render anything for emergency variant if no emergency action required
  if (variant === 'emergency' && !condition.requiresEmergencyAction) {
    return null;
  }

  const badge = (
    <CBadge 
      color={condition.requiresEmergencyAction ? 'danger' : getSeverityColor(condition.severity)}
      className={`${condition.requiresEmergencyAction ? 'text-white' : ''} ${size === 'sm' ? 'small' : ''}`}
    >
      {getBadgeContent()}
    </CBadge>
  );

  if (showTooltip) {
    return (
      <CTooltip content={getTooltipContent()} placement="top">
        <span className="cursor-pointer">{badge}</span>
      </CTooltip>
    );
  }

  return badge;
};

export default MedicalConditionBadge;