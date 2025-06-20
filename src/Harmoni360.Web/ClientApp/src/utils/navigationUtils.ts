import { ModuleType, PermissionType, UserPermissions } from '../types/permissions';
import { ModuleState } from '../contexts/ModuleStateContext';

export interface NavigationItem {
  component: 'CNavTitle' | 'CNavGroup' | 'CNavItem';
  name: string;
  to?: string;
  icon?: React.ReactNode;
  badge?: any;
  items?: NavigationItem[];      // Existing - for CNavGroup children
  submodules?: NavigationItem[]; // New - for CNavTitle children (hierarchical structure)
  
  // Permission requirements for this navigation item
  module?: ModuleType;
  permission?: PermissionType;
  requireAnyPermission?: boolean; // If true, any permission in module is sufficient
  adminOnly?: boolean;
  systemAdminOnly?: boolean;
  roles?: string[];
  
  // New hierarchical properties
  parentModule?: ModuleType;
  depth?: number;
  collapsible?: boolean;
  className?: string;
}

/**
 * Type guards for navigation components
 */
export const isNavTitle = (item: NavigationItem): boolean => 
  item.component === 'CNavTitle';

export const isNavGroup = (item: NavigationItem): boolean => 
  item.component === 'CNavGroup';

export const isNavItem = (item: NavigationItem): boolean => 
  item.component === 'CNavItem';

export const hasSubmodules = (item: NavigationItem): boolean => 
  Boolean(item.submodules && item.submodules.length > 0);

export const hasItems = (item: NavigationItem): boolean => 
  Boolean(item.items && item.items.length > 0);

/**
 * Utility to flatten hierarchical structure if needed for backward compatibility
 */
export const flattenNavigation = (items: NavigationItem[]): NavigationItem[] => {
  const flattened: NavigationItem[] = [];
  
  const flatten = (navItems: NavigationItem[], depth: number = 0) => {
    navItems.forEach(item => {
      // Add the current item with depth information
      flattened.push({
        ...item,
        depth,
        className: `${item.className || ''} depth-${depth}`.trim()
      });
      
      // Recursively flatten submodules (CNavTitle children)
      if (item.submodules && item.submodules.length > 0) {
        flatten(item.submodules, depth + 1);
      }
      
      // Recursively flatten items (CNavGroup children)
      if (item.items && item.items.length > 0) {
        flatten(item.items, depth + 1);
      }
    });
  };
  
  flatten(items);
  return flattened;
};

/**
 * Utility to get all navigation items of a specific type
 */
export const getNavigationItemsByType = (
  items: NavigationItem[], 
  componentType: 'CNavTitle' | 'CNavGroup' | 'CNavItem'
): NavigationItem[] => {
  const result: NavigationItem[] = [];
  
  const search = (navItems: NavigationItem[]) => {
    navItems.forEach(item => {
      if (item.component === componentType) {
        result.push(item);
      }
      
      if (item.submodules) {
        search(item.submodules);
      }
      
      if (item.items) {
        search(item.items);
      }
    });
  };
  
  search(items);
  return result;
};

/**
 * Utility to find a navigation item by path
 */
export const findNavigationItemByPath = (
  items: NavigationItem[], 
  path: string
): NavigationItem | null => {
  const search = (navItems: NavigationItem[]): NavigationItem | null => {
    for (const item of navItems) {
      if (item.to === path) {
        return item;
      }
      
      if (item.submodules) {
        const found = search(item.submodules);
        if (found) return found;
      }
      
      if (item.items) {
        const found = search(item.items);
        if (found) return found;
      }
    }
    return null;
  };
  
  return search(items);
};

/**
 * Apply module status to navigation items (for future module configuration)
 */
/**
 * Get the string key for a module, handling both string and numeric enum values
 */
const getModuleKey = (module: ModuleType): string => {
  // For string enums, module should already be a string
  if (typeof module === 'string') {
    return module;
  }
  
  // For numeric values (from backend), find the corresponding enum key
  const numericValue = Number(module);
  if (!isNaN(numericValue)) {
    // Map numeric values to enum keys based on backend ModuleType enum
    const numericToStringMap: Record<number, string> = {
      1: 'Dashboard',
      2: 'IncidentManagement', 
      3: 'RiskManagement',
      4: 'PPEManagement',
      5: 'InspectionManagement',
      6: 'AuditManagement',
      7: 'TrainingManagement',
      8: 'LicenseManagement',
      9: 'WasteManagement',
      10: 'HealthMonitoring',
      11: 'WorkPermitManagement',
      12: 'PhysicalSecurity',
      13: 'InformationSecurity',
      14: 'PersonnelSecurity',
      15: 'SecurityIncidentManagement',
      16: 'ComplianceManagement',
      17: 'Reporting',
      18: 'UserManagement',
      19: 'ApplicationSettings'
    };
    
    return numericToStringMap[numericValue] || String(module);
  }
  
  // Fallback to string conversion
  return String(module);
};

export const applyModuleStatus = (
  items: NavigationItem[],
  moduleStatusMap?: Record<string, { enabled: boolean; status?: 'disabled' | 'maintenance' | 'coming-soon' }>,
  isSuperAdmin: boolean = false
): NavigationItem[] => {
  return items.map(item => {
    if (isNavTitle(item) && item.module && moduleStatusMap) {
      const moduleKey = getModuleKey(item.module);
      const moduleStatus = moduleStatusMap[moduleKey];
      
      if (moduleStatus) {
        // For non-SuperAdmin users, hide disabled modules completely
        if (!moduleStatus.enabled && !isSuperAdmin) {
          return { ...item, className: `${item.className || ''} module-hidden` };
        }
        
        // For SuperAdmin users, show disabled modules with indicators
        if (!moduleStatus.enabled && isSuperAdmin) {
          return { 
            ...item, 
            className: `${item.className || ''} module-disabled`,
            submodules: item.submodules ? applyModuleStatus(item.submodules, moduleStatusMap, isSuperAdmin) : undefined
          };
        }
        
        // Apply status indicators for maintenance/coming-soon
        if (moduleStatus.status) {
          return { 
            ...item, 
            className: `${item.className || ''} module-${moduleStatus.status}`,
            submodules: item.submodules ? applyModuleStatus(item.submodules, moduleStatusMap, isSuperAdmin) : undefined
          };
        }
      }
    }
    
    // Recursively apply to submodules and items
    const processedItem = { ...item };
    if (item.submodules) {
      processedItem.submodules = applyModuleStatus(item.submodules, moduleStatusMap, isSuperAdmin);
    }
    if (item.items) {
      processedItem.items = applyModuleStatus(item.items, moduleStatusMap, isSuperAdmin);
    }
    
    return processedItem;
  });
};

/**
 * Get CSS selector for hiding/showing specific modules
 */
export const getModuleSelector = (moduleType: ModuleType): string => {
  return `[data-module="${moduleType}"]`;
};

/**
 * Generate CSS class names based on module state
 */
export const getModuleClassName = (moduleState: ModuleState, existingClassName?: string): string => {
  const classNames: string[] = [];
  
  if (existingClassName) {
    classNames.push(existingClassName);
  }
  
  if (!moduleState.isVisible) {
    classNames.push('module-hidden');
  }
  
  if (moduleState.status) {
    classNames.push(`module-${moduleState.status}`);
  }
  
  return classNames.join(' ').trim();
};

/**
 * React-based utility functions for module management
 * These functions return state-based operations instead of direct DOM manipulation
 */
export const createModuleManager = (
  hideModule: (moduleType: ModuleType) => void,
  showModule: (moduleType: ModuleType) => void,
  toggleModule: (moduleType: ModuleType) => void,
  setModuleStatus: (moduleType: ModuleType, status: 'disabled' | 'maintenance' | 'coming-soon' | null) => void
) => ({
  /**
   * Hide a specific module using React state
   */
  hideModule,

  /**
   * Show a specific module using React state
   */
  showModule,

  /**
   * Toggle module visibility using React state
   */
  toggleModule,

  /**
   * Set module status using React state
   */
  setModuleStatus
});

/**
 * @deprecated Use createModuleManager with React context instead
 * Legacy DOM-based module management - kept for backward compatibility
 */
export const LegacyModuleManager = {
  /**
   * Hide a specific module by adding the module-hidden class
   */
  hideModule: (moduleType: ModuleType) => {
    const selector = getModuleSelector(moduleType);
    const moduleElement = document.querySelector(selector);
    if (moduleElement) {
      moduleElement.classList.add('module-hidden');
    }
  },

  /**
   * Show a specific module by removing the module-hidden class
   */
  showModule: (moduleType: ModuleType) => {
    const selector = getModuleSelector(moduleType);
    const moduleElement = document.querySelector(selector);
    if (moduleElement) {
      moduleElement.classList.remove('module-hidden');
    }
  },

  /**
   * Toggle module visibility
   */
  toggleModule: (moduleType: ModuleType) => {
    const selector = getModuleSelector(moduleType);
    const moduleElement = document.querySelector(selector);
    if (moduleElement) {
      moduleElement.classList.toggle('module-hidden');
    }
  },

  /**
   * Set module status (disabled, maintenance, coming-soon)
   */
  setModuleStatus: (moduleType: ModuleType, status: 'disabled' | 'maintenance' | 'coming-soon' | null) => {
    const selector = getModuleSelector(moduleType);
    const moduleElement = document.querySelector(selector);
    if (moduleElement) {
      // Remove all status classes
      moduleElement.classList.remove('module-disabled', 'module-maintenance', 'module-coming-soon');
      
      // Add new status class if specified
      if (status) {
        moduleElement.classList.add(`module-${status}`);
      }
    }
  }
};

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
 * Filter a single navigation item and its children (supports hierarchical structure)
 */
const filterNavigationItem = (
  item: NavigationItem,
  permissions: UserPermissions
): NavigationItem | null => {
  // Check if user has access to this navigation item
  if (!hasNavigationAccess(item, permissions)) {
    return null;
  }

  let filteredItem = { ...item };

  // Filter submodules (new hierarchical structure for CNavTitle)
  if (item.submodules && item.submodules.length > 0) {
    const filteredSubmodules = item.submodules
      .map((submodule) => filterNavigationItem(submodule, permissions))
      .filter((submodule): submodule is NavigationItem => submodule !== null);

    // If no submodules are accessible and it's a CNavTitle, hide the parent
    if (filteredSubmodules.length === 0 && item.component === 'CNavTitle') {
      return null;
    }

    filteredItem.submodules = filteredSubmodules;
  }

  // Filter items (existing logic for CNavGroup children)
  if (item.items && item.items.length > 0) {
    const filteredItems = item.items
      .map((childItem) => filterNavigationItem(childItem, permissions))
      .filter((childItem): childItem is NavigationItem => childItem !== null);

    // If no children are accessible, hide the parent (unless it has its own route)
    if (filteredItems.length === 0 && !item.to) {
      return null;
    }

    filteredItem.items = filteredItems;
  }

  return filteredItem;
};

/**
 * Check if user has access to a navigation item
 */
export const hasNavigationAccess = (
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
 * Hierarchical navigation configuration with correct structure
 * Module Names (CNavTitle) → Functional Areas (CNavGroup) → Actions (CNavItem)
 */
export const createNavigationConfig = (): NavigationItem[] => [
  // Dashboard (standalone)
  {
    component: 'CNavItem',
    name: 'Dashboard',
    to: '/dashboard',
    icon: null, // Will be set in the component
    module: ModuleType.Dashboard,
    permission: PermissionType.Read,
  },
  
  // 1. Work Permit Management Module
  {
    component: 'CNavTitle',
    name: 'Work Permit Management', // ✅ Module name as title
    module: ModuleType.WorkPermitManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Work Permits', // ✅ Functional area as group
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
    ],
  },

  // 2. Risk Management Module
  {
    component: 'CNavTitle',
    name: 'Risk Management', // ✅ Module name as title
    module: ModuleType.RiskManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Hazard & Risk', // ✅ Functional area as group
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
    ],
  },

  // 3. Inspection Management Module
  {
    component: 'CNavTitle',
    name: 'Inspection Management', // ✅ Module name as title
    module: ModuleType.InspectionManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Inspections', // ✅ Functional area as group
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
    ],
  },

  // 4. Audit Management Module
  {
    component: 'CNavTitle',
    name: 'Audit Management', // ✅ Module name as title
    module: ModuleType.AuditManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Audits', // ✅ Functional area as group
        to: '#audits',
        icon: null,
        module: ModuleType.AuditManagement,
        requireAnyPermission: true,
        items: [
          {
            component: 'CNavItem',
            name: 'Audit Dashboard',
            to: '/audits/dashboard',
            module: ModuleType.AuditManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Create Audit',
            to: '/audits/create',
            module: ModuleType.AuditManagement,
            permission: PermissionType.Create,
          },
          {
            component: 'CNavItem',
            name: 'View Audits',
            to: '/audits',
            module: ModuleType.AuditManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'My Audits',
            to: '/audits/my-audits',
            module: ModuleType.AuditManagement,
            permission: PermissionType.Read,
          },
        ],
      },
    ],
  },
  
  // 5. Incident Management Module
  {
    component: 'CNavTitle',
    name: 'Incident Management', // ✅ Module name as title
    module: ModuleType.IncidentManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Incidents', // ✅ Functional area as group
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
    ],
  },

  // 6. PPE Management Module
  {
    component: 'CNavTitle',
    name: 'PPE Management', // ✅ Module name as title
    module: ModuleType.PPEManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'PPE', // ✅ Functional area as group
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
    ],
  },

  // 7. Training Management Module
  {
    component: 'CNavTitle',
    name: 'Training Management', // ✅ Module name as title
    module: ModuleType.TrainingManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Training', // ✅ Functional area as group
        to: '#training',
        icon: null,
        module: ModuleType.TrainingManagement,
        requireAnyPermission: true,
        items: [
          {
            component: 'CNavItem',
            name: 'Training Dashboard',
            to: '/trainings/dashboard',
            module: ModuleType.TrainingManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Create Training',
            to: '/trainings/create',
            module: ModuleType.TrainingManagement,
            permission: PermissionType.Create,
          },
          {
            component: 'CNavItem',
            name: 'View Trainings',
            to: '/trainings',
            module: ModuleType.TrainingManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'My Trainings',
            to: '/trainings/my-trainings',
            module: ModuleType.TrainingManagement,
            permission: PermissionType.Read,
          },
        ],
      },
    ],
  },

  // 8. License Management Module
  {
    component: 'CNavTitle',
    name: 'License Management', // ✅ Module name as title
    module: ModuleType.LicenseManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Licenses', // ✅ Functional area as group
        to: '#licenses',
        icon: null,
        module: ModuleType.LicenseManagement,
        requireAnyPermission: true,
        items: [
          {
            component: 'CNavItem',
            name: 'License Dashboard',
            to: '/licenses/dashboard',
            module: ModuleType.LicenseManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Create License',
            to: '/licenses/create',
            module: ModuleType.LicenseManagement,
            permission: PermissionType.Create,
          },
          {
            component: 'CNavItem',
            name: 'View Licenses',
            to: '/licenses',
            module: ModuleType.LicenseManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'My Licenses',
            to: '/licenses/my-licenses',
            module: ModuleType.LicenseManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Expiring Licenses',
            to: '/licenses/expiring',
            module: ModuleType.LicenseManagement,
            permission: PermissionType.Read,
          },
        ],
      },
    ],
  },

  // 9. Environment Management Module
  {
    component: 'CNavTitle',
    name: 'Environment Management', // ✅ Module name as title
    module: ModuleType.WasteManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Environment', // ✅ Functional area as group
        to: '#waste',
        icon: null,
        module: ModuleType.WasteManagement,
        requireAnyPermission: true,
        items: [
          {
            component: 'CNavItem',
            name: 'Waste Dashboard',
            to: '/waste-management/dashboard',
            icon: null,
            module: ModuleType.WasteManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Waste Reports',
            to: '/waste-management',
            icon: null,
            module: ModuleType.WasteManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Create Report',
            to: '/waste-management/create',
            icon: null,
            module: ModuleType.WasteManagement,
            permission: PermissionType.Create,
          },
          {
            component: 'CNavItem',
            name: 'My Reports',
            to: '/waste-management/my-reports',
            icon: null,
            module: ModuleType.WasteManagement,
            permission: PermissionType.Read,
          },
          {
            component: 'CNavItem',
            name: 'Disposal Providers',
            to: '/waste-management/providers',
            icon: null,
            module: ModuleType.WasteManagement,
            permission: PermissionType.Read,
          },
        ],
      },
    ],
  },

  // 10. HSSE Dashboard Module
  {
    component: 'CNavTitle',
    name: 'HSSE Dashboard', // ✅ Module name as title
    adminOnly: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'HSSE Statistics', // ✅ Functional area as group
        to: '#hsse-stats',
        icon: null,
        adminOnly: true,
        items: [
          {
            component: 'CNavItem',
            name: 'HSSE Dashboard',
            to: '/hsse/dashboard',
            icon: null,
            adminOnly: true,
          },
        ],
      },
    ],
  },

  // 11. Security Management Module
  {
    component: 'CNavTitle',
    name: 'Security Management', // ✅ Module name as title
    module: ModuleType.SecurityIncidentManagement,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Security', // ✅ Functional area as group
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
    ],
  },

  // 12. Health Management Module
  {
    component: 'CNavTitle',
    name: 'Health Management', // ✅ Module name as title
    module: ModuleType.HealthMonitoring,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Health Records', // ✅ Functional area as group
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
    ],
  },

  // 13. Administration Module
  {
    component: 'CNavTitle',
    name: 'Administration', // ✅ Module name as title
    adminOnly: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'System Management', // ✅ Functional area as group
        to: '#admin',
        icon: null,
        adminOnly: true,
        items: [
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
        ],
      },
    ],
  },

  // 14. Reporting Module
  {
    component: 'CNavTitle',
    name: 'Reporting', // ✅ Module name as title
    module: ModuleType.Reporting,
    requireAnyPermission: true,
    submodules: [
      {
        component: 'CNavGroup',
        name: 'Reports', // ✅ Functional area as group
        to: '#reports',
        icon: null,
        module: ModuleType.Reporting,
        requireAnyPermission: true,
        items: [
          {
            component: 'CNavItem',
            name: 'Reports',
            to: '/reports',
            icon: null,
            module: ModuleType.Reporting,
            permission: PermissionType.Read,
          },
        ],
      },
    ],
  },
];