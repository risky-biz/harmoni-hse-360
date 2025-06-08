import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  CContainer,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CButton,
  CAlert,
} from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faExclamationTriangle,
  faArrowLeft,
  faHome,
  faEnvelope,
} from '@fortawesome/free-solid-svg-icons';
import { useAuth } from '../../hooks/useAuth';
import { usePermissions } from '../../hooks/usePermissions';

interface UnauthorizedAccessProps {
  title?: string;
  message?: string;
  requiredPermission?: string;
  requiredModule?: string;
  requiredRole?: string;
  showContactSupport?: boolean;
  showBackButton?: boolean;
  showHomeButton?: boolean;
}

const UnauthorizedAccess: React.FC<UnauthorizedAccessProps> = ({
  title = 'Access Denied',
  message = 'You do not have permission to access this resource.',
  requiredPermission,
  requiredModule,
  requiredRole,
  showContactSupport = true,
  showBackButton = true,
  showHomeButton = true,
}) => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const permissions = usePermissions();

  const handleGoBack = () => {
    navigate(-1);
  };

  const handleGoHome = () => {
    navigate('/dashboard');
  };

  const handleContactSupport = () => {
    const subject = encodeURIComponent('Access Permission Request');
    const body = encodeURIComponent(
      `Dear IT Support,\n\nI am requesting access to a resource in Harmoni360.\n\n` +
      `User: ${user?.name} (${user?.email})\n` +
      `Current Roles: ${user?.roles.join(', ') || 'None'}\n` +
      `Required Permission: ${requiredPermission || 'Not specified'}\n` +
      `Required Module: ${requiredModule || 'Not specified'}\n` +
      `Required Role: ${requiredRole || 'Not specified'}\n` +
      `Requested Resource: ${window.location.pathname}\n\n` +
      `Please review and grant the necessary permissions if appropriate.\n\n` +
      `Thank you.`
    );
    window.location.href = `mailto:support@harmoni360.com?subject=${subject}&body=${body}`;
  };

  return (
    <CContainer className="d-flex align-items-center min-vh-100">
      <CRow className="justify-content-center w-100">
        <CCol md={8} lg={6} xl={5}>
          <CCard className="mx-4">
            <CCardBody className="p-4 text-center">
              <div className="mb-4">
                <FontAwesomeIcon
                  icon={faExclamationTriangle}
                  size="4x"
                  className="text-warning"
                />
              </div>
              
              <h1 className="h2 mb-3">{title}</h1>
              
              <p className="text-medium-emphasis mb-4">
                {message}
              </p>

              {/* Permission Details */}
              {(requiredPermission || requiredModule || requiredRole) && (
                <CAlert color="info" className="text-start mb-4">
                  <h6 className="alert-heading">Required Access:</h6>
                  {requiredModule && (
                    <div><strong>Module:</strong> {requiredModule}</div>
                  )}
                  {requiredPermission && (
                    <div><strong>Permission:</strong> {requiredPermission}</div>
                  )}
                  {requiredRole && (
                    <div><strong>Role:</strong> {requiredRole}</div>
                  )}
                </CAlert>
              )}

              {/* Current User Info */}
              {user && (
                <CAlert color="secondary" className="text-start mb-4">
                  <h6 className="alert-heading">Your Current Access:</h6>
                  <div><strong>User:</strong> {user.name}</div>
                  <div><strong>Roles:</strong> {user.roles.join(', ') || 'None assigned'}</div>
                  <div><strong>Department:</strong> {user.department || 'Not specified'}</div>
                </CAlert>
              )}

              {/* Action Buttons */}
              <div className="d-flex flex-column flex-sm-row gap-2 justify-content-center">
                {showBackButton && (
                  <CButton
                    color="secondary"
                    variant="outline"
                    onClick={handleGoBack}
                    className="me-sm-2"
                  >
                    <FontAwesomeIcon icon={faArrowLeft} className="me-2" />
                    Go Back
                  </CButton>
                )}
                
                {showHomeButton && (
                  <CButton
                    color="primary"
                    onClick={handleGoHome}
                    className="me-sm-2"
                  >
                    <FontAwesomeIcon icon={faHome} className="me-2" />
                    Dashboard
                  </CButton>
                )}
                
                {showContactSupport && (
                  <CButton
                    color="info"
                    variant="outline"
                    onClick={handleContactSupport}
                  >
                    <FontAwesomeIcon icon={faEnvelope} className="me-2" />
                    Request Access
                  </CButton>
                )}
              </div>

              {/* Additional Help Text */}
              <div className="mt-4 text-medium-emphasis small">
                <p className="mb-1">
                  If you believe you should have access to this resource, please contact your supervisor or IT support.
                </p>
                <p className="mb-0">
                  Access permissions are managed based on your role and department assignments.
                </p>
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </CContainer>
  );
};

export default UnauthorizedAccess;