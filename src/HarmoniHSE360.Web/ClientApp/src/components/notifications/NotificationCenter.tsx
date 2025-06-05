import React, { useState } from 'react';
import {
  COffcanvas,
  COffcanvasHeader,
  COffcanvasTitle,
  COffcanvasBody,
  CButton,
  CListGroup,
  CListGroupItem,
  CBadge,
  CSpinner,
} from '@coreui/react';

interface NotificationCenterProps {
  isOpen: boolean;
  onClose: () => void;
}

const NotificationCenter: React.FC<NotificationCenterProps> = ({
  isOpen,
  onClose,
}) => {
  const [isLoading] = useState(false);
  const [notifications] = useState([
    {
      id: '1',
      type: 'hazard',
      severity: 'error',
      title: 'Critical Hazard Reported',
      message: 'Chemical spill in Laboratory A requires immediate attention',
      timestamp: new Date(),
      isRead: false,
    },
    {
      id: '2',
      type: 'risk_assessment',
      severity: 'warning',
      title: 'Risk Assessment Due',
      message: 'Monthly risk assessment for Equipment Room B is overdue',
      timestamp: new Date(),
      isRead: false,
    },
  ]);

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'error':
        return 'danger';
      case 'warning':
        return 'warning';
      case 'info':
        return 'info';
      default:
        return 'secondary';
    }
  };

  return (
    <COffcanvas visible={isOpen} onHide={onClose} placement="end">
      <COffcanvasHeader>
        <COffcanvasTitle>Notifications</COffcanvasTitle>
      </COffcanvasHeader>
      <COffcanvasBody>
        {isLoading ? (
          <div className="text-center py-4">
            <CSpinner />
          </div>
        ) : (
          <CListGroup flush>
            {notifications.map((notification) => (
              <CListGroupItem key={notification.id} className="border-0 px-0">
                <div className="d-flex justify-content-between align-items-start">
                  <div className="me-auto">
                    <div className="fw-semibold">{notification.title}</div>
                    <div className="text-medium-emphasis small">
                      {notification.message}
                    </div>
                    <div className="text-muted small">
                      {notification.timestamp.toLocaleString()}
                    </div>
                  </div>
                  <CBadge color={getSeverityColor(notification.severity)}>
                    {notification.severity}
                  </CBadge>
                </div>
              </CListGroupItem>
            ))}
          </CListGroup>
        )}
        
        <div className="mt-3">
          <CButton color="primary" variant="outline" size="sm" className="w-100">
            Mark All as Read
          </CButton>
        </div>
      </COffcanvasBody>
    </COffcanvas>
  );
};

export default NotificationCenter;