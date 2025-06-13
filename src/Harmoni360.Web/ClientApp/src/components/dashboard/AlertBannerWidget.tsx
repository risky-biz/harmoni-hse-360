import React from 'react';
import { CAlert, CCard, CCardBody, CButton } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faExclamationTriangle, 
  faInfoCircle, 
  faCheckCircle, 
  faExternalLinkAlt
} from '@fortawesome/free-solid-svg-icons';

interface AlertBannerWidgetProps {
  title: string;
  message: string;
  color?: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  data?: any;
  onDismiss?: () => void;
  actionButton?: {
    text: string;
    onClick: () => void;
    color?: string;
  };
}

const AlertBannerWidget: React.FC<AlertBannerWidgetProps> = ({
  title,
  message,
  color = 'info',
  data,
  onDismiss,
  actionButton,
}) => {
  const getIcon = () => {
    switch (color) {
      case 'danger':
        return faExclamationTriangle;
      case 'warning':
        return faExclamationTriangle;
      case 'success':
        return faCheckCircle;
      case 'info':
      case 'primary':
      default:
        return faInfoCircle;
    }
  };

  const renderContent = () => {
    if (data?.type === 'overdue-incidents') {
      return (
        <div>
          <div className="d-flex justify-content-between align-items-start mb-2">
            <div>
              <h6 className="mb-1">{data.count} Overdue Incidents</h6>
              <p className="mb-2 small">{message}</p>
            </div>
            <FontAwesomeIcon icon={getIcon()} size="lg" />
          </div>
          {data.incidents && data.incidents.length > 0 && (
            <div className="mt-3">
              <small className="text-muted d-block mb-2">Most urgent:</small>
              {data.incidents.slice(0, 3).map((incident: any, index: number) => (
                <div key={index} className="d-flex justify-content-between align-items-center py-1">
                  <span className="small">{incident.title}</span>
                  <span className="badge bg-danger small">{incident.daysOverdue}d overdue</span>
                </div>
              ))}
            </div>
          )}
        </div>
      );
    }

    if (data?.type === 'safety-tip') {
      return (
        <div className="d-flex align-items-start">
          <FontAwesomeIcon icon={getIcon()} className="me-3 mt-1" />
          <div className="flex-grow-1">
            <h6 className="mb-1">Safety Tip of the Day</h6>
            <p className="mb-2">{message}</p>
            {data.source && (
              <small className="text-muted">Source: {data.source}</small>
            )}
          </div>
        </div>
      );
    }

    // Default content
    return (
      <div className="d-flex align-items-start">
        <FontAwesomeIcon icon={getIcon()} className="me-3 mt-1" />
        <div className="flex-grow-1">
          <h6 className="mb-1">{title}</h6>
          <p className="mb-0">{message}</p>
        </div>
      </div>
    );
  };

  return (
    <CCard className="h-100">
      <CCardBody className="p-0">
        <CAlert 
          color={color} 
          className="mb-0 border-0 rounded-0"
          dismissible={!!onDismiss}
          onClose={onDismiss}
        >
          {renderContent()}
          
          {actionButton && (
            <div className="mt-3 pt-2 border-top">
              <CButton
                size="sm"
                color={actionButton.color || color}
                variant="outline"
                onClick={actionButton.onClick}
                className="d-flex align-items-center"
              >
                <span>{actionButton.text}</span>
                <FontAwesomeIcon icon={faExternalLinkAlt} size="sm" className="ms-1" />
              </CButton>
            </div>
          )}
        </CAlert>
      </CCardBody>
    </CCard>
  );
};

export default AlertBannerWidget;