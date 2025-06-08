import { useAppSelector } from '../store/hooks';
import { selectAuth } from '../features/auth/authSlice';

export const useAuth = () => {
  const auth = useAppSelector(selectAuth);

  const hasRole = (role: string): boolean => {
    return auth.user?.roles.includes(role) || false;
  };

  const hasPermission = (permission: string): boolean => {
    return auth.user?.permissions.includes(permission) || false;
  };

  const hasAnyRole = (roles: string[]): boolean => {
    return roles.some((role) => hasRole(role));
  };

  const hasAnyPermission = (permissions: string[]): boolean => {
    return permissions.some((permission) => hasPermission(permission));
  };

  return {
    user: auth.user,
    isAuthenticated: auth.isAuthenticated,
    isLoading: auth.isLoading,
    error: auth.error,
    hasRole,
    hasPermission,
    hasAnyRole,
    hasAnyPermission,
  };
};
