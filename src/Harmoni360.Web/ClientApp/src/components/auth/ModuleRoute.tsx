import React from 'react';
import { Navigate } from 'react-router-dom';
import { usePermissions } from '../../hooks/usePermissions';
import { useGetEnabledModuleConfigurationsQuery } from '../../services/moduleConfigurationApi';
import { ModuleType, PermissionType, RoleType } from '../../types/permissions';
import { CSpinner } from '@coreui/react';

interface ModuleRouteProps {
  children: React.ReactNode;
  module: ModuleType;
  permission?: PermissionType;
  requireAnyPermission?: boolean;
}

/**
 * Route protection component that checks both module status and user permissions
 * Redirects to dashboard if:
 * 1. Module is disabled (except for SuperAdmin/Developer)
 * 2. User doesn't have required permissions
 */
const ModuleRoute: React.FC<ModuleRouteProps> = ({
  children,
  module,
  permission,
  requireAnyPermission = false
}) => {
  const permissions = usePermissions();
  const { data: enabledModules = [], isLoading } = useGetEnabledModuleConfigurationsQuery();

  // Show loading spinner while checking module status
  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <CSpinner color="primary" />
      </div>
    );
  }

  // Check if user has SuperAdmin or Developer role (they bypass all restrictions)
  const hasHighestPriority = permissions.roles.includes(RoleType.SuperAdmin) || 
                             permissions.roles.includes(RoleType.Developer);

  // If not SuperAdmin/Developer, check if module is enabled
  if (!hasHighestPriority) {
    // Create mapping from frontend ModuleType to backend numeric values
    const moduleMapping: Record<string, number> = {
      'Dashboard': 1,
      'IncidentManagement': 2,
      'RiskManagement': 3,
      'PPEManagement': 4,
      'HealthMonitoring': 5,
      'PhysicalSecurity': 6,
      'InformationSecurity': 7,
      'PersonnelSecurity': 8,
      'SecurityIncidentManagement': 9,
      'ComplianceManagement': 10,
      'Reporting': 11,
      'UserManagement': 12,
      'WorkPermitManagement': 14,
      'InspectionManagement': 15,
      'AuditManagement': 16,
      'TrainingManagement': 17,
      'LicenseManagement': 18,
      'WasteManagement': 19,
      'ApplicationSettings': 20
    };

    const backendModuleType = moduleMapping[module];
    const isModuleEnabled = enabledModules.some(m => m.moduleType === backendModuleType);

    // If module is disabled, redirect to dashboard
    if (!isModuleEnabled) {
      console.warn(`Access denied: Module ${module} is disabled`);
      return <Navigate to="/dashboard" replace />;
    }
  }

  // Check if user has access to this module at all
  if (!permissions.hasModuleAccess(module)) {
    console.warn(`Access denied: User does not have access to module ${module}`);
    return <Navigate to="/dashboard" replace />;
  }

  // If specific permission is required, check for it
  if (permission && !requireAnyPermission) {
    if (!permissions.hasPermission(module, permission)) {
      console.warn(`Access denied: User does not have ${permission} permission for module ${module}`);
      return <Navigate to="/dashboard" replace />;
    }
  }

  // If requireAnyPermission is true, check if user has any permission for this module
  if (requireAnyPermission && permission) {
    const hasAnyPermission = [
      PermissionType.Read,
      PermissionType.Create,
      PermissionType.Update,
      PermissionType.Delete,
      PermissionType.Export,
      PermissionType.Configure,
      PermissionType.Approve,
      PermissionType.Assign
    ].some(perm => permissions.hasPermission(module, perm));

    if (!hasAnyPermission) {
      console.warn(`Access denied: User does not have any permissions for module ${module}`);
      return <Navigate to="/dashboard" replace />;
    }
  }

  // All checks passed, render the protected component
  return <>{children}</>;
};

export default ModuleRoute;