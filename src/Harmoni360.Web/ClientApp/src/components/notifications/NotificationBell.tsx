import React, { useState, useEffect } from 'react';
import {
  CButton,
  CBadge,
  CTooltip,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBell } from '@fortawesome/free-solid-svg-icons';
import { useSignalR } from '../../hooks/useSignalR';
import NotificationCenter from './NotificationCenter';

interface NotificationBellProps {
  className?: string;
}

const NotificationBell: React.FC<NotificationBellProps> = ({ className = '' }) => {
  const [showNotifications, setShowNotifications] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const [hasNewNotification, setHasNewNotification] = useState(false);
  const { connection, isConnected } = useSignalR();

  // Load initial unread count
  useEffect(() => {
    loadUnreadCount();
  }, []);

  // Set up SignalR listeners for real-time updates
  useEffect(() => {
    if (!connection) return;

    const handleNewNotification = () => {
      setUnreadCount(prev => prev + 1);
      setHasNewNotification(true);
      
      // Reset the animation after 3 seconds
      setTimeout(() => setHasNewNotification(false), 3000);
    };

    const handleNotificationRead = () => {
      setUnreadCount(prev => Math.max(0, prev - 1));
    };

    const handleAllNotificationsRead = () => {
      setUnreadCount(0);
    };

    // Register SignalR event handlers
    connection.on('NewNotification', handleNewNotification);
    connection.on('HazardCreated', handleNewNotification);
    connection.on('HighRiskAssessmentCreated', handleNewNotification);
    connection.on('MitigationActionOverdue', handleNewNotification);
    connection.on('NotificationRead', handleNotificationRead);
    connection.on('AllNotificationsRead', handleAllNotificationsRead);

    // Health-specific notification handlers
    connection.on('VaccinationExpiryAlert', handleNewNotification);
    connection.on('HealthExaminationDue', handleNewNotification);
    connection.on('EmergencyHealthIncident', handleNewNotification);
    connection.on('CriticalMedicalConditionAlert', handleNewNotification);
    connection.on('HealthComplianceAlert', handleNewNotification);

    return () => {
      connection.off('NewNotification');
      connection.off('HazardCreated');
      connection.off('HighRiskAssessmentCreated');
      connection.off('MitigationActionOverdue');
      connection.off('NotificationRead');
      connection.off('AllNotificationsRead');
      
      // Clean up health notification handlers
      connection.off('VaccinationExpiryAlert');
      connection.off('HealthExaminationDue');
      connection.off('EmergencyHealthIncident');
      connection.off('CriticalMedicalConditionAlert');
      connection.off('HealthComplianceAlert');
    };
  }, [connection]);

  const loadUnreadCount = async () => {
    try {
      const response = await fetch('/api/notifications/unread-count', {
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setUnreadCount(data.count || 0);
      } else if (response.status === 401) {
        // Authentication required - set to 0 for unauthenticated users
        setUnreadCount(0);
      } else {
        // API error - provide guidance for development mode
        if (import.meta.env.DEV) {
          console.warn('NotificationBell: API error. Please ensure the backend is running and database is seeded.');
          setUnreadCount(0); // Show 0 when API has errors
        } else {
          setUnreadCount(0);
        }
      }
    } catch (error) {
      // Network error or JSON parsing error
      if (import.meta.env.DEV) {
        console.warn('NotificationBell: API unavailable. Please ensure the backend is running and database is seeded.');
        setUnreadCount(0); // Show 0 when API is unavailable
      } else {
        console.error('Failed to load unread count:', error);
        setUnreadCount(0);
      }
    }
  };

  const handleBellClick = () => {
    setShowNotifications(true);
    setHasNewNotification(false);
  };

  return (
    <>
      <CTooltip
        content={`${unreadCount} unread notifications${!isConnected ? ' (Disconnected)' : ''}`}
        placement="bottom"
      >
        <CButton
          color="ghost"
          className={`position-relative ${className} ${hasNewNotification ? 'notification-pulse' : ''}`}
          onClick={handleBellClick}
        >
          <FontAwesomeIcon 
            icon={faBell} 
            className={`${!isConnected ? 'text-muted' : 'text-dark'}`}
          />
          {unreadCount > 0 && (
            <CBadge
              color="danger"
              position="top-end"
              shape="rounded-pill"
              className="position-absolute translate-middle"
              style={{ 
                fontSize: '0.7em',
                minWidth: '1.2em',
                height: '1.2em',
                lineHeight: '1.2em',
                top: '0.3em',
                right: '0.3em',
              }}
            >
              {unreadCount > 99 ? '99+' : unreadCount}
            </CBadge>
          )}
        </CButton>
      </CTooltip>

      <NotificationCenter
        isOpen={showNotifications}
        onClose={() => setShowNotifications(false)}
      />

      {/* CSS for pulse animation */}
      <style>
        {`
          .notification-pulse {
            animation: pulse 1.5s infinite;
          }
          
          @keyframes pulse {
            0% {
              transform: scale(1);
            }
            50% {
              transform: scale(1.1);
            }
            100% {
              transform: scale(1);
            }
          }
          
          .notification-pulse .fa-bell {
            color: #ffc107 !important;
          }
        `}
      </style>
    </>
  );
};

export default NotificationBell;