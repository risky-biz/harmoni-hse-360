import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { CAlert, CButton, CCard, CCardBody, CCardHeader } from '@coreui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faShieldAlt, faHome } from '@fortawesome/free-solid-svg-icons';
import { useAuth } from '../../hooks/useAuth';

interface AdminRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
  fallbackPath?: string;
}

const AdminRoute: React.FC<AdminRouteProps> = ({
  children,
  requiredRoles = ['SuperAdmin', 'Developer'],
  fallbackPath = '/dashboard',
}) => {
  const { user, isAuthenticated } = useAuth();
  const location = useLocation();

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Check if user has any of the required roles
  const hasRequiredRole = user?.roles?.some((role) =>
    requiredRoles.includes(role)
  );

  // Check if user has system configuration permissions as fallback
  const hasSystemPermissions = user?.permissions?.some(
    (permission) =>
      permission === 'system.configure' || 
      permission === 'system.admin' ||
      permission === 'settings.manage'
  );

  if (!hasRequiredRole && !hasSystemPermissions) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="container-fluid">
          <div className="row justify-content-center">
            <div className="col-md-6 col-lg-5">
              <CCard className="shadow-lg border-0">
                <CCardHeader className="text-center bg-danger text-white">
                  <FontAwesomeIcon icon={faShieldAlt} size="2x" className="mb-2" />
                  <h4 className="mb-0">Access Restricted</h4>
                </CCardHeader>
                <CCardBody className="text-center p-4">
                  <CAlert color="danger" className="border-0">
                    <strong>Insufficient Permissions</strong>
                  </CAlert>
                  
                  <p className="text-muted mb-3">
                    You don't have the required permissions to access application settings. 
                    This area is restricted to system administrators only.
                  </p>
                  
                  <div className="mb-3">
                    <small className="text-muted">
                      <strong>Required Roles:</strong> {requiredRoles.join(' or ')}
                    </small>
                    <br />
                    <small className="text-muted">
                      <strong>Your Roles:</strong> {user?.roles?.join(', ') || 'None'}
                    </small>
                  </div>
                  
                  <div className="d-grid gap-2">
                    <CButton 
                      color="primary" 
                      onClick={() => window.history.back()}
                      className="mb-2"
                    >
                      <FontAwesomeIcon icon={faHome} className="me-2" />
                      Go Back
                    </CButton>
                    
                    <CButton 
                      color="outline-secondary" 
                      onClick={() => window.location.href = fallbackPath}
                      size="sm"
                    >
                      Return to Dashboard
                    </CButton>
                  </div>
                  
                  <hr className="my-3" />
                  
                  <small className="text-muted">
                    If you believe you should have access to this area, please contact your system administrator.
                  </small>
                </CCardBody>
              </CCard>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return <>{children}</>;
};

export default AdminRoute;