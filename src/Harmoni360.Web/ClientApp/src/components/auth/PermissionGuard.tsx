import React from 'react';
import { usePermissions } from '../../hooks/usePermissions';
import { ModuleType, PermissionType, RoleType } from '../../types/permissions';

interface PermissionGuardProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  // Module permission check
  module?: ModuleType;
  permission?: PermissionType;
  // Role-based check
  roles?: RoleType[];
  requireAllRoles?: boolean; // If true, user must have ALL specified roles
  // Admin checks
  requireAdmin?: boolean;
  requireSystemAdmin?: boolean;
  // Custom permission check
  check?: () => boolean;
  // Invert the check (show when user DOESN'T have permission)
  invert?: boolean;
}

/**
 * Permission-based component that conditionally renders children based on user permissions
 * 
 * @example
 * // Show content only if user has read access to Health module
 * <PermissionGuard module={ModuleType.HealthMonitoring} permission={PermissionType.Read}>
 *   <HealthDashboard />
 * </PermissionGuard>
 * 
 * @example
 * // Show content only for admins
 * <PermissionGuard requireAdmin>
 *   <AdminPanel />
 * </PermissionGuard>
 * 
 * @example
 * // Show content for specific roles
 * <PermissionGuard roles={[RoleType.IncidentManager, RoleType.Admin]}>
 *   <IncidentManagement />
 * </PermissionGuard>
 */
export const PermissionGuard: React.FC<PermissionGuardProps> = ({
  children,
  fallback = null,
  module,
  permission,
  roles,
  requireAllRoles = false,
  requireAdmin = false,
  requireSystemAdmin = false,
  check,
  invert = false
}) => {
  const permissions = usePermissions();

  const hasAccess = React.useMemo(() => {
    // Custom check has highest priority
    if (check) {
      return check();
    }

    // System admin check
    if (requireSystemAdmin) {
      return permissions.isSystemAdmin();
    }

    // Admin check
    if (requireAdmin) {
      return permissions.isAdmin();
    }

    // Module permission check
    if (module && permission) {
      return permissions.hasPermission(module, permission);
    }

    // Module access check (any permission)
    if (module && !permission) {
      return permissions.hasModuleAccess(module);
    }

    // Role-based check
    if (roles && roles.length > 0) {
      if (requireAllRoles) {
        return roles.every(role => permissions.hasRole(role));
      } else {
        return roles.some(role => permissions.hasRole(role));
      }
    }

    // Default to true if no checks specified
    return true;
  }, [permissions, module, permission, roles, requireAllRoles, requireAdmin, requireSystemAdmin, check]);

  const shouldRender = invert ? !hasAccess : hasAccess;

  return shouldRender ? <>{children}</> : <>{fallback}</>;
};

/**
 * Higher-order component for permission-based rendering
 */
export const withPermissions = <P extends object>(
  Component: React.ComponentType<P>,
  permissionProps: Omit<PermissionGuardProps, 'children' | 'fallback'>
) => {
  return React.forwardRef<any, P>((props, ref) => (
    <PermissionGuard {...permissionProps}>
      <Component {...(props as any)} ref={ref} />
    </PermissionGuard>
  ));
};

/**
 * Component for role-specific content
 */
interface RoleGuardProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  roles: RoleType[];
  requireAll?: boolean;
}

export const RoleGuard: React.FC<RoleGuardProps> = ({
  children,
  fallback = null,
  roles,
  requireAll = false
}) => {
  return (
    <PermissionGuard 
      roles={roles} 
      requireAllRoles={requireAll} 
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  );
};

/**
 * Component for module-specific content
 */
interface ModuleGuardProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  module: ModuleType;
  permission?: PermissionType;
}

export const ModuleGuard: React.FC<ModuleGuardProps> = ({
  children,
  fallback = null,
  module,
  permission
}) => {
  return (
    <PermissionGuard 
      module={module} 
      permission={permission} 
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  );
};

/**
 * Component for admin-only content
 */
interface AdminGuardProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  systemAdmin?: boolean;
}

export const AdminGuard: React.FC<AdminGuardProps> = ({
  children,
  fallback = null,
  systemAdmin = false
}) => {
  return (
    <PermissionGuard 
      requireAdmin={!systemAdmin}
      requireSystemAdmin={systemAdmin}
      fallback={fallback}
    >
      {children}
    </PermissionGuard>
  );
};