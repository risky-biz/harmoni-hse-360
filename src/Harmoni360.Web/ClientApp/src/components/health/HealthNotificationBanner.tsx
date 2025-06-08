import React from 'react';
import {
  CAlert,
  CBadge
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faClipboardList } from '@fortawesome/free-solid-svg-icons';

interface HealthNotificationBannerProps {
  userId?: string;
  showGlobal?: boolean;
}

const HealthNotificationBanner: React.FC<HealthNotificationBannerProps> = () => {

  // Simplified health notifications
  return (
    <div className="health-notification-banner mb-3">
      <CAlert color="info" className="d-flex align-items-center justify-content-between">
        <div className="d-flex align-items-center">
          <FontAwesomeIcon icon={faClipboardList} className="me-2 text-info" />
          <div>
            <strong>Health System Active</strong>
            <div className="small">
              Health monitoring and emergency contact system is operational
            </div>
          </div>
        </div>
        <CBadge color="success">Active</CBadge>
      </CAlert>
    </div>
  );
};

export default HealthNotificationBanner;