import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../store/hooks';
import { logout } from '../features/auth/authSlice';

interface UnauthorizedHandlerOptions {
  redirectToLogin?: boolean;
  showNotification?: boolean;
  logoutUser?: boolean;
}

/**
 * Custom hook to handle unauthorized access scenarios
 * Provides centralized handling for 401/403 responses and permission violations
 */
export const useUnauthorizedHandler = (options: UnauthorizedHandlerOptions = {}) => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  
  const {
    redirectToLogin = true,
    showNotification = true,
    logoutUser = false,
  } = options;

  /**
   * Handle 401 Unauthorized responses (authentication issues)
   */
  const handle401 = useCallback((error?: any) => {
    console.warn('401 Unauthorized access detected:', error);
    
    if (showNotification) {
      // Could integrate with notification system here
      console.info('User session has expired. Please log in again.');
    }
    
    if (logoutUser) {
      dispatch(logout());
    }
    
    if (redirectToLogin) {
      navigate('/login', { 
        state: { 
          from: window.location.pathname,
          message: 'Your session has expired. Please log in again.'
        }
      });
    }
  }, [navigate, dispatch, showNotification, logoutUser, redirectToLogin]);

  /**
   * Handle 403 Forbidden responses (authorization issues)
   */
  const handle403 = useCallback((error?: any, requiredPermission?: string) => {
    console.warn('403 Forbidden access detected:', error);
    
    if (showNotification) {
      // Could integrate with notification system here
      console.info('You do not have permission to access this resource.');
    }
    
    // Navigate to unauthorized page with context
    navigate('/unauthorized', {
      state: {
        from: window.location.pathname,
        requiredPermission,
        error: error?.message,
      }
    });
  }, [navigate, showNotification]);

  /**
   * Generic unauthorized handler that determines the appropriate response
   */
  const handleUnauthorized = useCallback((
    error: any, 
    context?: {
      requiredPermission?: string;
      requiredModule?: string;
      requiredRole?: string;
    }
  ) => {
    const status = error?.status || error?.response?.status;
    
    switch (status) {
      case 401:
        handle401(error);
        break;
      case 403:
        handle403(error, context?.requiredPermission);
        break;
      default:
        // Handle other authorization-related errors
        console.warn('Unknown authorization error:', error);
        if (showNotification) {
          console.info('Access denied. Please check your permissions.');
        }
        break;
    }
  }, [handle401, handle403, showNotification]);

  /**
   * Check if an error is authorization-related
   */
  const isAuthorizationError = useCallback((error: any): boolean => {
    const status = error?.status || error?.response?.status;
    return status === 401 || status === 403;
  }, []);

  /**
   * Handle permission check failures for UI components
   */
  const handlePermissionDenied = useCallback((context: {
    action: string;
    requiredPermission?: string;
    requiredModule?: string;
    requiredRole?: string;
  }) => {
    console.warn('Permission denied for action:', context);
    
    if (showNotification) {
      console.info(`You do not have permission to ${context.action}.`);
    }
    
    // Optionally navigate to unauthorized page with detailed context
    // navigate('/unauthorized', { state: context });
  }, [showNotification]);

  /**
   * Wrapper for API calls that automatically handles unauthorized responses
   */
  const withUnauthorizedHandling = useCallback(<T>(
    apiCall: () => Promise<T>,
    context?: {
      requiredPermission?: string;
      requiredModule?: string;
      requiredRole?: string;
    }
  ): Promise<T> => {
    return apiCall().catch((error) => {
      if (isAuthorizationError(error)) {
        handleUnauthorized(error, context);
        throw error; // Re-throw to allow calling code to handle
      }
      throw error; // Re-throw non-authorization errors
    });
  }, [isAuthorizationError, handleUnauthorized]);

  return {
    handle401,
    handle403,
    handleUnauthorized,
    handlePermissionDenied,
    isAuthorizationError,
    withUnauthorizedHandling,
  };
};

/**
 * Higher-order function to wrap API functions with unauthorized handling
 */
export const withUnauthorizedHandler = <T extends (...args: any[]) => Promise<any>>(
  apiFunction: T,
  context?: {
    requiredPermission?: string;
    requiredModule?: string;
    requiredRole?: string;
  }
): T => {
  return ((...args: Parameters<T>) => {
    const { withUnauthorizedHandling } = useUnauthorizedHandler();
    return withUnauthorizedHandling(() => apiFunction(...args), context);
  }) as T;
};