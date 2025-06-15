import React from 'react';
import { CAlert, CButton } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faExclamationTriangle, faRefresh, faExternalLinkAlt } from '@fortawesome/free-solid-svg-icons';

interface ApiUnavailableMessageProps {
  title?: string;
  message?: string;
  showRefresh?: boolean;
  onRefresh?: () => void;
  className?: string;
}

const ApiUnavailableMessage: React.FC<ApiUnavailableMessageProps> = ({
  title = 'API Unavailable',
  message = 'Unable to connect to the backend API.',
  showRefresh = true,
  onRefresh,
  className = ''
}) => {
  const isDevelopment = import.meta.env.DEV;

  const handleRefresh = () => {
    if (onRefresh) {
      onRefresh();
    } else {
      window.location.reload();
    }
  };

  return (
    <CAlert color="warning" className={`d-flex align-items-center ${className}`}>
      <FontAwesomeIcon icon={faExclamationTriangle} className="flex-shrink-0 me-3" size="lg" />
      <div className="flex-grow-1">
        <h5 className="mb-2">{title}</h5>
        <p className="mb-2">{message}</p>
        
        {isDevelopment && (
          <div className="small text-muted mb-3">
            <strong>Development Mode:</strong>
            <ul className="mb-0 mt-1">
              <li>Ensure the .NET backend API is running</li>
              <li>Check if the database is properly seeded</li>
              <li>Verify your connection to the backend server</li>
            </ul>
          </div>
        )}
        
        <div className="d-flex gap-2">
          {showRefresh && (
            <CButton
              color="warning"
              variant="outline"
              size="sm"
              onClick={handleRefresh}
            >
              <FontAwesomeIcon icon={faRefresh} className="me-1" />
              Retry
            </CButton>
          )}
          
          {isDevelopment && (
            <CButton
              color="info"
              variant="outline"
              size="sm"
              onClick={() => window.open('/DEVELOPMENT_SETUP.md', '_blank')}
            >
              <FontAwesomeIcon icon={faExternalLinkAlt} className="me-1" />
              Setup Guide
            </CButton>
          )}
        </div>
      </div>
    </CAlert>
  );
};

export default ApiUnavailableMessage;