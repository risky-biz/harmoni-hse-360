import React, { useEffect, useState } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { CSpinner } from '@coreui/react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { selectAuth, initializeAuth } from '../../features/auth/authSlice';

interface PrivateRouteProps {
  children: React.ReactNode;
  requiredRoles?: string[];
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children, requiredRoles = [] }) => {
  const dispatch = useAppDispatch();
  const auth = useAppSelector(selectAuth);
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

  // Check role-based access if required roles are specified
  if (requiredRoles.length > 0 && auth.user) {
    const hasRequiredRole = requiredRoles.some(role => 
      auth.user?.roles.includes(role)
    );

    if (!hasRequiredRole) {
      return (
        <div className="d-flex justify-content-center align-items-center min-vh-100">
          <div className="text-center">
            <h3>Access Denied</h3>
            <p>You don't have permission to access this page.</p>
          </div>
        </div>
      );
    }
  }

  return <>{children}</>;
};

export default PrivateRoute;