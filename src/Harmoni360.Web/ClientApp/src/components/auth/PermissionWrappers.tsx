import React from 'react';
import { CButton, CButtonProps } from '@coreui/react';
import { PermissionGuard } from './PermissionGuard';
import { ModuleType, PermissionType, RoleType } from '../../types/permissions';

/**
 * Enhanced button component that only renders when user has the required permissions
 */
interface PermissionButtonProps extends CButtonProps {
  // Module permission check
  module?: ModuleType;
  permission?: PermissionType;
  // Role-based check
  roles?: RoleType[];
  requireAllRoles?: boolean;
  // Admin checks
  requireAdmin?: boolean;
  requireSystemAdmin?: boolean;
  // Custom permission check
  permissionCheck?: () => boolean;
  // Show disabled state instead of hiding
  showDisabled?: boolean;
  // Fallback content when permission is denied
  fallback?: React.ReactNode;
}

export const PermissionButton: React.FC<PermissionButtonProps> = ({
  module,
  permission,
  roles,
  requireAllRoles = false,
  requireAdmin = false,
  requireSystemAdmin = false,
  permissionCheck,
  showDisabled = false,
  fallback = null,
  children,
  ...buttonProps
}) => {
  if (showDisabled) {
    return (
      <PermissionGuard
        module={module}
        permission={permission}
        roles={roles}
        requireAllRoles={requireAllRoles}
        requireAdmin={requireAdmin}
        requireSystemAdmin={requireSystemAdmin}
        check={permissionCheck}
        invert={true}
        fallback={
          <CButton {...buttonProps}>
            {children}
          </CButton>
        }
      >
        <CButton {...buttonProps} disabled>
          {children}
        </CButton>
      </PermissionGuard>
    );
  }

  return (
    <PermissionGuard
      module={module}
      permission={permission}
      roles={roles}
      requireAllRoles={requireAllRoles}
      requireAdmin={requireAdmin}
      requireSystemAdmin={requireSystemAdmin}
      check={permissionCheck}
      fallback={fallback}
    >
      <CButton {...buttonProps}>
        {children}
      </CButton>
    </PermissionGuard>
  );
};

/**
 * Higher-order component that wraps any component with permission checking
 */
export const withPermissionGuard = <P extends object>(
  Component: React.ComponentType<P>,
  permissionConfig: {
    module?: ModuleType;
    permission?: PermissionType;
    roles?: RoleType[];
    requireAllRoles?: boolean;
    requireAdmin?: boolean;
    requireSystemAdmin?: boolean;
    permissionCheck?: () => boolean;
    fallback?: React.ReactNode;
  }
) => {
  return React.forwardRef<any, P>((props, ref) => (
    <PermissionGuard {...permissionConfig}>
      <Component {...props} ref={ref} />
    </PermissionGuard>
  ));
};

/**
 * Quick wrapper components for common permission patterns
 */

// Admin-only content wrapper
interface AdminOnlyProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
  systemAdmin?: boolean;
}

export const AdminOnly: React.FC<AdminOnlyProps> = ({
  children,
  fallback = null,
  systemAdmin = false,
}) => (
  <PermissionGuard
    requireAdmin={!systemAdmin}
    requireSystemAdmin={systemAdmin}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// Incident management permission wrappers
export const IncidentCreateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.IncidentManagement}
    permission={PermissionType.Create}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

export const IncidentUpdateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.IncidentManagement}
    permission={PermissionType.Update}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

export const IncidentDeleteGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.IncidentManagement}
    permission={PermissionType.Delete}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// PPE management permission wrappers
export const PPECreateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.PPEManagement}
    permission={PermissionType.Create}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

export const PPEConfigureGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.PPEManagement}
    permission={PermissionType.Configure}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// Health monitoring permission wrappers
export const HealthCreateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.HealthMonitoring}
    permission={PermissionType.Create}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

export const HealthUpdateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.HealthMonitoring}
    permission={PermissionType.Update}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// Risk management permission wrappers
export const RiskCreateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.RiskManagement}
    permission={PermissionType.Create}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

export const RiskUpdateGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    module={ModuleType.RiskManagement}
    permission={PermissionType.Update}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// Export configuration permission wrapper
export const ExportGuard: React.FC<{ 
  children: React.ReactNode; 
  fallback?: React.ReactNode;
  module: ModuleType;
}> = ({
  children,
  fallback = null,
  module,
}) => (
  <PermissionGuard
    module={module}
    permission={PermissionType.Export}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// Configuration permission wrapper
export const ConfigureGuard: React.FC<{ 
  children: React.ReactNode; 
  fallback?: React.ReactNode;
  module: ModuleType;
}> = ({
  children,
  fallback = null,
  module,
}) => (
  <PermissionGuard
    module={module}
    permission={PermissionType.Configure}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

/**
 * Utility components for role-based rendering
 */

// Manager-level access (IncidentManager, RiskManager, PPEManager, HealthMonitor)
export const ManagerGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    roles={[RoleType.IncidentManager, RoleType.RiskManager, RoleType.PPEManager, RoleType.HealthMonitor]}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);

// Developer access
export const DeveloperGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback = null,
}) => (
  <PermissionGuard
    roles={[RoleType.Developer]}
    fallback={fallback}
  >
    {children}
  </PermissionGuard>
);