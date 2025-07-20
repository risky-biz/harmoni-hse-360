import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { RoleType } from '../../types/permissions';
import UnauthorizedAccess from '../common/UnauthorizedAccess';

interface ElsaStudioGuardProps {
  children: React.ReactNode;
}

/**
 * Guard component specifically for Elsa Studio access
 * Only allows SuperAdmin and Developer roles
 */
const ElsaStudioGuard: React.FC<ElsaStudioGuardProps> = ({ children }) => {
  const { user, isAuthenticated, isLoading } = useAuth();

  // Show loading while checking authentication
  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check if user has SuperAdmin or Developer role
  const hasRequiredRole = user?.roles.includes(RoleType.SuperAdmin) || 
                         user?.roles.includes(RoleType.Developer);

  // Show unauthorized access page if user doesn't have required role
  if (!hasRequiredRole) {
    return (
      <UnauthorizedAccess
        title="Elsa Studio Access Restricted"
        message="Access to Elsa Studio is restricted to System Administrators and Developers only."
        requiredRole="SuperAdmin or Developer"
        requiredModule="Workflow Management"
        showContactSupport={true}
        showBackButton={true}
        showHomeButton={true}
      />
    );
  }

  // User has required role, render children
  return <>{children}</>;
};

export default ElsaStudioGuard;