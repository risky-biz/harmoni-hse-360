import { ModuleType, PermissionType, UserPermissions } from '../types/permissions';

interface NavigationItem {
  component: any;
  name: string;
  to?: string;
  icon?: React.ReactNode;
  badge?: any;
  items?: NavigationItem[];
  // Permission requirements for this navigation item
  module?: ModuleType;
  permission?: PermissionType;
  requireAnyPermission?: boolean; // If true, any permission in module is sufficient
  adminOnly?: boolean;
  systemAdminOnly?: boolean;
  roles?: string[];
}

/**
 * Filter navigation items based on user permissions
 */
export const filterNavigationByPermissions = (
  navigation: NavigationItem[],
  permissions: UserPermissions
): NavigationItem[] => {
  return navigation
    .map((item) => filterNavigationItem(item, permissions))
    .filter((item): item is NavigationItem => item !== null);
};

/**
 * Filter a single navigation item and its children
 */
const filterNavigationItem = (
  item: NavigationItem,
  permissions: UserPermissions
): NavigationItem | null => {
  // Check if user has access to this navigation item
  if (!hasNavigationAccess(item, permissions)) {
    return null;
  }

  // If the item has children, filter them recursively
  if (item.items && item.items.length > 0) {
    const filteredItems = item.items
      .map((childItem) => filterNavigationItem(childItem, permissions))
      .filter((childItem): childItem is NavigationItem => childItem !== null);

    // If no children are accessible, hide the parent (unless it has its own route)
    if (filteredItems.length === 0 && !item.to) {
      return null;
    }

    return {
      ...item,
      items: filteredItems,
    };
  }

  return item;
};

/**
 * Check if user has access to a navigation item
 */
const hasNavigationAccess = (
  item: NavigationItem,
  permissions: UserPermissions
): boolean => {
  // System admin check
  if (item.systemAdminOnly) {
    return permissions.isSystemAdmin();
  }

  // Admin check - if item requires admin AND has module permissions, check both
  if (item.adminOnly) {
    const isAdmin = permissions.isAdmin();
    
    // If it's adminOnly but also has module requirements, check both
    if (item.module) {
      if (item.permission) {
        return isAdmin && permissions.hasPermission(item.module, item.permission);
      } else if (item.requireAnyPermission) {
        return isAdmin && permissions.hasModuleAccess(item.module);
      }
      // If adminOnly and has module but no specific permissions, just check admin
      return isAdmin;
    }
    
    // Pure adminOnly check
    return isAdmin;
  }

  // Module-based check
  if (item.module) {
    if (item.permission) {
      // Specific permission required
      return permissions.hasPermission(item.module, item.permission);
    } else if (item.requireAnyPermission) {
      // Any permission in the module is sufficient
      return permissions.hasModuleAccess(item.module);
    }
  }

  // Role-based check (legacy)
  if (item.roles && item.roles.length > 0) {
    return item.roles.some((role) => permissions.roles.includes(role as any));
  }

  // Default to allowing access if no restrictions specified
  return true;
};

/**
 * Enhanced navigation configuration with permission requirements
 * Following HSSE workflow hierarchy as per advisor feedback
 */
export const createNavigationConfig = (): NavigationItem[] => [
  {
    component: 'CNavItem',
    name: 'Dashboard',
    to: '/dashboard',
    icon: null, // Will be set in the component
    module: ModuleType.Dashboard,
    permission: PermissionType.Read,
  },
  
  // 1. Work Permit Management
  {
    component: 'CNavTitle',
    name: 'Work Permit Management',
    module: ModuleType.WorkPermitManagement,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'Work Permits',
    to: '#work-permits',
    icon: null,
    module: ModuleType.WorkPermitManagement,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'Work Permit Dashboard',
        to: '/work-permits/dashboard',
        module: ModuleType.WorkPermitManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Submit Work Permit',
        to: '/work-permits/create',
        module: ModuleType.WorkPermitManagement,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'View Work Permits',
        to: '/work-permits',
        module: ModuleType.WorkPermitManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'My Work Permits',
        to: '/work-permits/my-permits',
        module: ModuleType.WorkPermitManagement,
        permission: PermissionType.Read,
      },
    ],
  },

  // 2. Risk Management Section (moved up from previous position)
  {
    component: 'CNavTitle',
    name: 'Risk Management',
    module: ModuleType.RiskManagement,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'Hazard & Risk',
    to: '#hazards',
    icon: null,
    module: ModuleType.RiskManagement,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'Risk Dashboard',
        to: '/hazards/dashboard',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Report Hazard',
        to: '/hazards/create',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'Hazard Register',
        to: '/hazards',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Risk Assessments',
        to: '/hazards/assessments',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'My Hazards',
        to: '/hazards/my-hazards',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Location Mapping',
        to: '/hazards/mapping',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Risk Analytics',
        to: '/hazards/analytics',
        module: ModuleType.RiskManagement,
        permission: PermissionType.Read,
      },
    ],
  },

  // 3. Inspection Management
  {
    component: 'CNavTitle',
    name: 'Inspection Management',
    module: ModuleType.InspectionManagement,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'Inspections',
    to: '#inspections',
    icon: null,
    module: ModuleType.InspectionManagement,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'Inspection Dashboard',
        to: '/inspections/dashboard',
        module: ModuleType.InspectionManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Create Inspection',
        to: '/inspections/create',
        module: ModuleType.InspectionManagement,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'View Inspections',
        to: '/inspections',
        module: ModuleType.InspectionManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'My Inspections',
        to: '/inspections/my-inspections',
        module: ModuleType.InspectionManagement,
        permission: PermissionType.Read,
      },
    ],
  },

  // 4. Audit Management (Placeholder)
  {
    component: 'CNavTitle',
    name: 'Audit Management',
    adminOnly: true, // Temporary until module is implemented
  },
  {
    component: 'CNavItem',
    name: 'Audits (Coming Soon)',
    to: '#',
    icon: null,
    adminOnly: true,
  },
  
  // 5. Incident Management Section (moved down from top)
  {
    component: 'CNavTitle',
    name: 'Incident Management',
    module: ModuleType.IncidentManagement,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'Incidents',
    to: '#incidents',
    icon: null,
    module: ModuleType.IncidentManagement,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'Incident Dashboard',
        to: '/incidents/dashboard',
        module: ModuleType.IncidentManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Report Incident',
        to: '/incidents/create',
        module: ModuleType.IncidentManagement,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'View Incidents',
        to: '/incidents',
        module: ModuleType.IncidentManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'My Reports',
        to: '/incidents/my-reports',
        module: ModuleType.IncidentManagement,
        permission: PermissionType.Read,
      },
    ],
  },

  // 6. PPE Management Section
  {
    component: 'CNavTitle',
    name: 'PPE Management',
    module: ModuleType.PPEManagement,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'PPE',
    to: '#ppe',
    icon: null,
    module: ModuleType.PPEManagement,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'PPE Dashboard',
        to: '/ppe/dashboard',
        module: ModuleType.PPEManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'PPE Inventory',
        to: '/ppe',
        module: ModuleType.PPEManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Add PPE Item',
        to: '/ppe/create',
        module: ModuleType.PPEManagement,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'PPE Operations',
        to: '/ppe/management',
        module: ModuleType.PPEManagement,
        permission: PermissionType.Configure,
      },
    ],
  },

  // 7. Training Management (Placeholder)
  {
    component: 'CNavTitle',
    name: 'Training Management',
    adminOnly: true, // Temporary until module is implemented
  },
  {
    component: 'CNavItem',
    name: 'Training (Coming Soon)',
    to: '#',
    icon: null,
    adminOnly: true,
  },

  // 8. License Management (Placeholder)
  {
    component: 'CNavTitle',
    name: 'License Management',
    adminOnly: true, // Temporary until module is implemented
  },
  {
    component: 'CNavItem',
    name: 'Licenses (Coming Soon)',
    to: '#',
    icon: null,
    adminOnly: true,
  },

  // 9. Waste Management (Placeholder)
  {
    component: 'CNavTitle',
    name: 'Waste Management',
    adminOnly: true, // Temporary until module is implemented
  },
  {
    component: 'CNavItem',
    name: 'Waste Management (Coming Soon)',
    to: '#',
    icon: null,
    adminOnly: true,
  },

  // 10. HSE Statistics/Dashboard (Placeholder)
  {
    component: 'CNavTitle',
    name: 'HSE Statistics',
    adminOnly: true, // Temporary until module is implemented
  },
  {
    component: 'CNavItem',
    name: 'HSE Dashboard (Coming Soon)',
    to: '#',
    icon: null,
    adminOnly: true,
  },

  // 11. Security Management Section
  {
    component: 'CNavTitle',
    name: 'Security Management',
    module: ModuleType.SecurityIncidentManagement,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'Security',
    to: '#security',
    icon: null,
    module: ModuleType.SecurityIncidentManagement,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'Security Dashboard',
        to: '/security/dashboard',
        module: ModuleType.SecurityIncidentManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Report Security Incident',
        to: '/security/incidents/create',
        module: ModuleType.SecurityIncidentManagement,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'Security Incidents',
        to: '/security/incidents',
        module: ModuleType.SecurityIncidentManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Threat Assessment',
        to: '/security/threat-assessment',
        module: ModuleType.SecurityIncidentManagement,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Security Controls',
        to: '/security/controls',
        module: ModuleType.SecurityIncidentManagement,
        permission: PermissionType.Configure,
      },
    ],
  },

  // 12. Health Management Section (moved to last position)
  {
    component: 'CNavTitle',
    name: 'Health Management',
    module: ModuleType.HealthMonitoring,
    requireAnyPermission: true,
  },
  {
    component: 'CNavGroup',
    name: 'Health Records',
    to: '#health',
    icon: null,
    module: ModuleType.HealthMonitoring,
    requireAnyPermission: true,
    items: [
      {
        component: 'CNavItem',
        name: 'Health Dashboard',
        to: '/health/dashboard',
        module: ModuleType.HealthMonitoring,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Health Records',
        to: '/health',
        module: ModuleType.HealthMonitoring,
        permission: PermissionType.Read,
      },
      {
        component: 'CNavItem',
        name: 'Create Health Record',
        to: '/health/create',
        module: ModuleType.HealthMonitoring,
        permission: PermissionType.Create,
      },
      {
        component: 'CNavItem',
        name: 'Vaccination Management',
        to: '/health/vaccinations',
        module: ModuleType.HealthMonitoring,
        permission: PermissionType.Update,
      },
      {
        component: 'CNavItem',
        name: 'Health Compliance',
        to: '/health/compliance',
        module: ModuleType.HealthMonitoring,
        permission: PermissionType.Read,
      },
    ],
  },

  // Administration Section (Admin only)
  {
    component: 'CNavTitle',
    name: 'Administration',
    adminOnly: true,
  },
  {
    component: 'CNavItem',
    name: 'User Management',
    to: '/admin/users',
    icon: null,
    module: ModuleType.UserManagement,
    permission: PermissionType.Read,
  },
  {
    component: 'CNavItem',
    name: 'System Settings',
    to: '/admin/settings',
    icon: null,
    module: ModuleType.ApplicationSettings,
    permission: PermissionType.Configure,
  },

  // Reporting Section
  {
    component: 'CNavTitle',
    name: 'Reporting',
    module: ModuleType.Reporting,
    requireAnyPermission: true,
  },
  {
    component: 'CNavItem',
    name: 'Reports',
    to: '/reports',
    icon: null,
    module: ModuleType.Reporting,
    permission: PermissionType.Read,
  },
];