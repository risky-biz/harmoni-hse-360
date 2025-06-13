import React, { useEffect, useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { CSpinner } from '@coreui/react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { selectAuth, initializeAuth } from '../../features/auth/authSlice';
import { usePermissions } from '../../hooks/usePermissions';
import { ModuleType, PermissionType, RoleType } from '../../types/permissions';

interface PrivateRouteProps {
  children: React.ReactNode;
  // Legacy role-based protection (deprecated, use module-based instead)
  requiredRoles?: string[];
  // Module-based protection
  module?: ModuleType;
  permission?: PermissionType;
  // Role-based protection using enums
  roles?: RoleType[];
  requireAllRoles?: boolean;
  // Admin protection
  requireAdmin?: boolean;
  requireSystemAdmin?: boolean;
  // Custom permission check
  permissionCheck?: () => boolean;
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({
  children,
  requiredRoles = [],
  module,
  permission,
  roles,
  requireAllRoles = false,
  requireAdmin = false,
  requireSystemAdmin = false,
  permissionCheck,
}) => {
  const dispatch = useAppDispatch();
  const auth = useAppSelector(selectAuth);
  const permissions = usePermissions();
  const location = useLocation();
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    // Initialize auth state from localStorage on component mount
    dispatch(initializeAuth());
    setIsInitialized(true);
  }, [dispatch]);

  // Show loading spinner while initializing authentication
  if (!isInitialized) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <CSpinner color="primary" />
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!auth.isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Check permissions if user is authenticated
  if (auth.user) {
    let hasAccess = true;

    // Custom permission check has highest priority
    if (permissionCheck) {
      hasAccess = permissionCheck();
    }
    // System admin check
    else if (requireSystemAdmin) {
      hasAccess = permissions.isSystemAdmin();
    }
    // Admin check
    else if (requireAdmin) {
      hasAccess = permissions.isAdmin();
    }
    // Module permission check
    else if (module && permission) {
      hasAccess = permissions.hasPermission(module, permission);
    }
    // Module access check (any permission)
    else if (module && !permission) {
      hasAccess = permissions.hasModuleAccess(module);
    }
    // Role-based check using enums
    else if (roles && roles.length > 0) {
      if (requireAllRoles) {
        hasAccess = roles.every(role => permissions.hasRole(role));
      } else {
        hasAccess = roles.some(role => permissions.hasRole(role));
      }
    }
    // Legacy role-based check (deprecated)
    else if (requiredRoles.length > 0) {
      hasAccess = requiredRoles.some((role) =>
        auth.user?.roles.includes(role)
      );
    }

    if (!hasAccess) {
      return (
        <div className="d-flex justify-content-center align-items-center min-vh-100">
          <div className="text-center">
            <h3>Access Denied</h3>
            <p>You don't have permission to access this page.</p>
            <p className="text-muted">
              {module && permission && `Required: ${permission} permission in ${module} module`}
              {module && !permission && `Required: Access to ${module} module`}
              {roles && `Required role: ${roles.join(' or ')}`}
              {requireAdmin && 'Required: Administrative privileges'}
              {requireSystemAdmin && 'Required: System administrative privileges'}
            </p>
          </div>
        </div>
      );
    }
  }

  return <>{children}</>;
};

export default PrivateRoute;
