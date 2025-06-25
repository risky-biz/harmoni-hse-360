import React from 'react';
import { useLocation, Navigate } from 'react-router-dom';
import { usePermissions } from '../../hooks/usePermissions';
import { useGetEnabledModuleConfigurationsQuery } from '../../services/moduleConfigurationApi';
import { RoleType } from '../../types/permissions';
import { resolveRouteProtection } from '../../utils/routeProtection';
import { CSpinner } from '@coreui/react';

interface DynamicRouteGuardProps {
  children: React.ReactNode;
}

/**
 * Dynamic route protection component that automatically resolves permissions
 * based on the current route and protects accordingly
 */
const DynamicRouteGuard: React.FC<DynamicRouteGuardProps> = ({ children }) => {
  const location = useLocation();
  const permissions = usePermissions();
  const { data: enabledModules = [], isLoading } = useGetEnabledModuleConfigurationsQuery();

  // Show loading spinner while checking module status
  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
        <CSpinner color="primary" />
      </div>
    );
  }

  // Resolve protection rule for current route
  const protectionRule = resolveRouteProtection(location.pathname);

  // If no protection rule, allow access (unprotected route)
  if (!protectionRule) {
    return <>{children}</>;
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

    const backendModuleType = moduleMapping[protectionRule.module];
    const isModuleEnabled = enabledModules.some(m => m.moduleType === backendModuleType);

    // If module is disabled, redirect to dashboard
    if (!isModuleEnabled) {
      console.warn(`Access denied: Module ${protectionRule.module} is disabled for route ${location.pathname}`);
      return <Navigate to="/dashboard" replace />;
    }
  }

  // Check if user has access to this module at all
  if (!permissions.hasModuleAccess(protectionRule.module)) {
    console.warn(`Access denied: User does not have access to module ${protectionRule.module} for route ${location.pathname}`);
    return <Navigate to="/dashboard" replace />;
  }

  // Check specific permission
  if (!permissions.hasPermission(protectionRule.module, protectionRule.permission)) {
    console.warn(`Access denied: User does not have ${protectionRule.permission} permission for module ${protectionRule.module} on route ${location.pathname}`);
    return <Navigate to="/dashboard" replace />;
  }

  // All checks passed, render the protected component
  return <>{children}</>;
};

export default DynamicRouteGuard;