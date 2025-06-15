// Export all authentication and authorization components
export { default as PrivateRoute } from './PrivateRoute';
export { default as AdminRoute } from './AdminRoute';

export {
  PermissionGuard,
  RoleGuard,
  ModuleGuard,
  AdminGuard,
  withPermissions,
} from './PermissionGuard';

export {
  PermissionButton,
  withPermissionGuard,
  AdminOnly,
  IncidentCreateGuard,
  IncidentUpdateGuard,
  IncidentDeleteGuard,
  PPECreateGuard,
  PPEConfigureGuard,
  HealthCreateGuard,
  HealthUpdateGuard,
  RiskCreateGuard,
  RiskUpdateGuard,
  ExportGuard,
  ConfigureGuard,
  ManagerGuard,
  DeveloperGuard,
} from './PermissionWrappers';