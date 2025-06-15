import React from 'react';
import {
  CAlert,
  CBadge
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faExclamationTriangle
} from '@fortawesome/free-solid-svg-icons';

interface HealthAlertProps {
  alert: any; // Using any to avoid complex type mapping
}

const HealthAlert: React.FC<HealthAlertProps> = ({ alert }) => {
  // Simplified health alert display
  if (!alert) {
    return null;
  }

  return (
    <div>
      <CAlert color="warning" className="d-flex align-items-start mb-3">
        <FontAwesomeIcon 
          icon={faExclamationTriangle} 
          className="flex-shrink-0 me-2" 
        />
        <div className="flex-grow-1">
          <strong className="me-2">Health Information Available</strong>
          <div className="mt-1">
            <CBadge color="info" className="me-1">
              Medical conditions on file
            </CBadge>
          </div>
        </div>
      </CAlert>
    </div>
  );
};

export default HealthAlert;