import { ModuleType, PermissionType } from '../types/permissions';

export interface RouteProtectionRule {
  module: ModuleType;
  permission: PermissionType;
  allowBypass?: boolean; // For certain routes that might have special handling
}

/**
 * Dynamic route protection mapping based on URL patterns
 * This allows us to automatically determine required permissions without manually wrapping each route
 */
export const ROUTE_PROTECTION_MAP: Record<string, RouteProtectionRule> = {
  // Incident Management
  '/incidents': { module: ModuleType.IncidentManagement, permission: PermissionType.Read },
  '/incidents/dashboard': { module: ModuleType.IncidentManagement, permission: PermissionType.Read },
  '/incidents/create': { module: ModuleType.IncidentManagement, permission: PermissionType.Create },
  '/incidents/quick-report': { module: ModuleType.IncidentManagement, permission: PermissionType.Create },
  '/incidents/qr-scanner': { module: ModuleType.IncidentManagement, permission: PermissionType.Create },
  '/incidents/my-reports': { module: ModuleType.IncidentManagement, permission: PermissionType.Read },

  // Risk/Hazard Management
  '/hazards': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/hazards/dashboard': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/hazards/create': { module: ModuleType.RiskManagement, permission: PermissionType.Create },
  '/hazards/mobile-report': { module: ModuleType.RiskManagement, permission: PermissionType.Create },
  '/hazards/my-hazards': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/hazards/assessments': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/hazards/analytics': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/hazards/mapping': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/risk-assessments': { module: ModuleType.RiskManagement, permission: PermissionType.Read },
  '/risk-assessments/create': { module: ModuleType.RiskManagement, permission: PermissionType.Create },

  // Inspection Management
  '/inspections': { module: ModuleType.InspectionManagement, permission: PermissionType.Read },
  '/inspections/dashboard': { module: ModuleType.InspectionManagement, permission: PermissionType.Read },
  '/inspections/create': { module: ModuleType.InspectionManagement, permission: PermissionType.Create },
  '/inspections/my-inspections': { module: ModuleType.InspectionManagement, permission: PermissionType.Read },

  // PPE Management
  '/ppe': { module: ModuleType.PPEManagement, permission: PermissionType.Read },
  '/ppe/dashboard': { module: ModuleType.PPEManagement, permission: PermissionType.Read },
  '/ppe/management': { module: ModuleType.PPEManagement, permission: PermissionType.Read },
  '/ppe/create': { module: ModuleType.PPEManagement, permission: PermissionType.Create },

  // Work Permit Management
  '/work-permits': { module: ModuleType.WorkPermitManagement, permission: PermissionType.Read },
  '/work-permits/dashboard': { module: ModuleType.WorkPermitManagement, permission: PermissionType.Read },
  '/work-permits/create': { module: ModuleType.WorkPermitManagement, permission: PermissionType.Create },
  '/work-permits/my-permits': { module: ModuleType.WorkPermitManagement, permission: PermissionType.Read },

  // License Management
  '/licenses': { module: ModuleType.LicenseManagement, permission: PermissionType.Read },
  '/licenses/dashboard': { module: ModuleType.LicenseManagement, permission: PermissionType.Read },
  '/licenses/create': { module: ModuleType.LicenseManagement, permission: PermissionType.Create },
  '/licenses/my-licenses': { module: ModuleType.LicenseManagement, permission: PermissionType.Read },
  '/licenses/expiring': { module: ModuleType.LicenseManagement, permission: PermissionType.Read },

  // Audit Management
  '/audits': { module: ModuleType.AuditManagement, permission: PermissionType.Read },
  '/audits/dashboard': { module: ModuleType.AuditManagement, permission: PermissionType.Read },
  '/audits/create': { module: ModuleType.AuditManagement, permission: PermissionType.Create },
  '/audits/my-audits': { module: ModuleType.AuditManagement, permission: PermissionType.Read },

  // Health Management
  '/health': { module: ModuleType.HealthMonitoring, permission: PermissionType.Read },
  '/health/dashboard': { module: ModuleType.HealthMonitoring, permission: PermissionType.Read },
  '/health/create': { module: ModuleType.HealthMonitoring, permission: PermissionType.Create },
  '/health/vaccinations': { module: ModuleType.HealthMonitoring, permission: PermissionType.Read },
  '/health/compliance': { module: ModuleType.HealthMonitoring, permission: PermissionType.Read },

  // Security Management
  '/security': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read },
  '/security/incidents': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read },
  '/security/dashboard': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read },
  '/security/incidents/create': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Create },
  '/security/threat-assessment': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read },
  '/security/controls': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read },
  '/security/analytics': { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read },

  // Training Management
  '/trainings': { module: ModuleType.TrainingManagement, permission: PermissionType.Read },
  '/trainings/dashboard': { module: ModuleType.TrainingManagement, permission: PermissionType.Read },
  '/trainings/create': { module: ModuleType.TrainingManagement, permission: PermissionType.Create },
  '/trainings/my-trainings': { module: ModuleType.TrainingManagement, permission: PermissionType.Read },

  // Workflow Management
  '/workflows': { module: ModuleType.WorkflowManagement, permission: PermissionType.Read },

  // Waste Management
  '/waste': { module: ModuleType.WasteManagement, permission: PermissionType.Read },
  '/waste/reports': { module: ModuleType.WasteManagement, permission: PermissionType.Read },
  '/waste/dashboard': { module: ModuleType.WasteManagement, permission: PermissionType.Read },
  '/waste/reports/create': { module: ModuleType.WasteManagement, permission: PermissionType.Create },
  '/waste/reports/my-reports': { module: ModuleType.WasteManagement, permission: PermissionType.Read },
  '/waste/providers': { module: ModuleType.WasteManagement, permission: PermissionType.Read },

  // User Management (Admin only)
  '/admin/users': { module: ModuleType.UserManagement, permission: PermissionType.Read },

  // Settings (Application Settings module)
  '/settings': { module: ModuleType.ApplicationSettings, permission: PermissionType.Read },
  '/settings/modules': { module: ModuleType.ApplicationSettings, permission: PermissionType.Configure },
  '/settings/modules/dashboard': { module: ModuleType.ApplicationSettings, permission: PermissionType.Read },
  '/settings/work-permits': { module: ModuleType.ApplicationSettings, permission: PermissionType.Configure },
  '/settings/incidents': { module: ModuleType.ApplicationSettings, permission: PermissionType.Configure },
  '/settings/risks': { module: ModuleType.ApplicationSettings, permission: PermissionType.Configure },
  '/settings/system': { module: ModuleType.ApplicationSettings, permission: PermissionType.Configure },
};

/**
 * Patterns for dynamic routes (with parameters like :id)
 * These use regex patterns to match dynamic segments
 */
export const DYNAMIC_ROUTE_PATTERNS: Array<{
  pattern: RegExp;
  rule: RouteProtectionRule;
}> = [
  // Incident Management dynamic routes
  { pattern: /^\/incidents\/[^/]+$/, rule: { module: ModuleType.IncidentManagement, permission: PermissionType.Read } },
  { pattern: /^\/incidents\/[^/]+\/edit$/, rule: { module: ModuleType.IncidentManagement, permission: PermissionType.Update } },

  // Hazard Management dynamic routes
  { pattern: /^\/hazards\/[^/]+$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Read } },
  { pattern: /^\/hazards\/[^/]+\/edit$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Update } },
  { pattern: /^\/hazards\/[^/]+\/mitigation-actions$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Update } },

  // Risk Assessment dynamic routes
  { pattern: /^\/risk-assessments\/[^/]+$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Read } },
  { pattern: /^\/risk-assessments\/[^/]+\/edit$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Update } },
  { pattern: /^\/risk-assessments\/[^/]+\/reassess$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Create } },
  { pattern: /^\/risk-assessments\/create\/[^/]+$/, rule: { module: ModuleType.RiskManagement, permission: PermissionType.Create } },

  // Inspection Management dynamic routes
  { pattern: /^\/inspections\/[^/]+$/, rule: { module: ModuleType.InspectionManagement, permission: PermissionType.Read } },
  { pattern: /^\/inspections\/[^/]+\/edit$/, rule: { module: ModuleType.InspectionManagement, permission: PermissionType.Update } },

  // PPE Management dynamic routes
  { pattern: /^\/ppe\/[^/]+$/, rule: { module: ModuleType.PPEManagement, permission: PermissionType.Read } },
  { pattern: /^\/ppe\/[^/]+\/edit$/, rule: { module: ModuleType.PPEManagement, permission: PermissionType.Update } },

  // Work Permit Management dynamic routes
  { pattern: /^\/work-permits\/[^/]+$/, rule: { module: ModuleType.WorkPermitManagement, permission: PermissionType.Read } },
  { pattern: /^\/work-permits\/[^/]+\/edit$/, rule: { module: ModuleType.WorkPermitManagement, permission: PermissionType.Update } },
  { pattern: /^\/work-permits\/[^/]+\/approve$/, rule: { module: ModuleType.WorkPermitManagement, permission: PermissionType.Approve } },

  // License Management dynamic routes
  { pattern: /^\/licenses\/[^/]+$/, rule: { module: ModuleType.LicenseManagement, permission: PermissionType.Read } },
  { pattern: /^\/licenses\/[^/]+\/edit$/, rule: { module: ModuleType.LicenseManagement, permission: PermissionType.Update } },

  // Audit Management dynamic routes
  { pattern: /^\/audits\/[^/]+$/, rule: { module: ModuleType.AuditManagement, permission: PermissionType.Read } },
  { pattern: /^\/audits\/[^/]+\/edit$/, rule: { module: ModuleType.AuditManagement, permission: PermissionType.Update } },

  // Health Management dynamic routes
  { pattern: /^\/health\/detail\/[^/]+$/, rule: { module: ModuleType.HealthMonitoring, permission: PermissionType.Read } },
  { pattern: /^\/health\/edit\/[^/]+$/, rule: { module: ModuleType.HealthMonitoring, permission: PermissionType.Update } },

  // Security Management dynamic routes
  { pattern: /^\/security\/incidents\/[^/]+$/, rule: { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Read } },
  { pattern: /^\/security\/incidents\/[^/]+\/edit$/, rule: { module: ModuleType.SecurityIncidentManagement, permission: PermissionType.Update } },

  // Training Management dynamic routes
  { pattern: /^\/trainings\/[^/]+$/, rule: { module: ModuleType.TrainingManagement, permission: PermissionType.Read } },
  { pattern: /^\/trainings\/[^/]+\/edit$/, rule: { module: ModuleType.TrainingManagement, permission: PermissionType.Update } },
  { pattern: /^\/trainings\/[^/]+\/enroll$/, rule: { module: ModuleType.TrainingManagement, permission: PermissionType.Update } },

  // Waste Management dynamic routes
  { pattern: /^\/waste\/reports\/[^/]+$/, rule: { module: ModuleType.WasteManagement, permission: PermissionType.Read } },
  { pattern: /^\/waste\/reports\/edit\/[^/]+$/, rule: { module: ModuleType.WasteManagement, permission: PermissionType.Update } },
];

/**
 * Routes that should bypass module protection (always accessible to authenticated users)
 */
export const BYPASS_ROUTES = [
  '/dashboard',
  '/hsse/dashboard',
  '/profile',
  '/unauthorized',
  '/report/qr/',
  '/report/anonymous',
  '/report/quick',
];

/**
 * Resolves the protection rule for a given route path
 * @param pathname - The current route pathname
 * @returns RouteProtectionRule or null if no protection needed
 */
export function resolveRouteProtection(pathname: string): RouteProtectionRule | null {
  // Check if route should bypass protection
  if (BYPASS_ROUTES.some(bypass => pathname === bypass || pathname.startsWith(bypass))) {
    return null;
  }

  // Check exact path matches first
  const exactMatch = ROUTE_PROTECTION_MAP[pathname];
  if (exactMatch) {
    return exactMatch;
  }

  // Check dynamic route patterns
  for (const { pattern, rule } of DYNAMIC_ROUTE_PATTERNS) {
    if (pattern.test(pathname)) {
      return rule;
    }
  }

  // No protection rule found
  return null;
}

/**
 * Helper function to check if a route is protected
 * @param pathname - The route pathname to check
 * @returns boolean indicating if the route requires protection
 */
export function isProtectedRoute(pathname: string): boolean {
  return resolveRouteProtection(pathname) !== null;
}