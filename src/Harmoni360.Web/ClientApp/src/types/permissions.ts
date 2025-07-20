// Module-based authorization types for frontend

export enum ModuleType {
  Dashboard = 'Dashboard',
  WorkPermitManagement = 'WorkPermitManagement',
  IncidentManagement = 'IncidentManagement',
  RiskManagement = 'RiskManagement',
  InspectionManagement = 'InspectionManagement',
  AuditManagement = 'AuditManagement',
  PPEManagement = 'PPEManagement',
  TrainingManagement = 'TrainingManagement',
  LicenseManagement = 'LicenseManagement',
  WasteManagement = 'WasteManagement',
  HealthMonitoring = 'HealthMonitoring',
  PhysicalSecurity = 'PhysicalSecurity',
  InformationSecurity = 'InformationSecurity',
  PersonnelSecurity = 'PersonnelSecurity',
  SecurityIncidentManagement = 'SecurityIncidentManagement',
  ComplianceManagement = 'ComplianceManagement',
  Reporting = 'Reporting',
  UserManagement = 'UserManagement',
  ApplicationSettings = 'ApplicationSettings',
  WorkflowManagement = 'WorkflowManagement'
}

export enum PermissionType {
  Read = 'Read',
  Create = 'Create',
  Update = 'Update',
  Delete = 'Delete',
  Export = 'Export',
  Configure = 'Configure',
  Approve = 'Approve',
  Assign = 'Assign'
}

export enum RoleType {
  SuperAdmin = 'SuperAdmin',
  Developer = 'Developer',
  Admin = 'Admin',
  IncidentManager = 'IncidentManager',
  RiskManager = 'RiskManager',
  PPEManager = 'PPEManager',
  HealthMonitor = 'HealthMonitor',
  SecurityManager = 'SecurityManager',
  SecurityOfficer = 'SecurityOfficer',
  ComplianceOfficer = 'ComplianceOfficer',
  Reporter = 'Reporter',
  Viewer = 'Viewer',
  WorkflowManager = 'WorkflowManager'
}

export interface ModulePermission {
  module: ModuleType;
  permission: PermissionType;
}

export interface UserPermissions {
  roles: RoleType[];
  permissions: string[]; // Format: "ModuleType.PermissionType"
  hasRole: (role: RoleType) => boolean;
  hasPermission: (module: ModuleType, permission: PermissionType) => boolean;
  hasModuleAccess: (module: ModuleType) => boolean;
  canPerformAction: (permission: PermissionType) => boolean;
  isAdmin: () => boolean;
  isSystemAdmin: () => boolean;
}

// Helper functions for permission strings
export const createPermissionString = (module: ModuleType, permission: PermissionType): string => {
  return `${module}.${permission}`;
};

export const parsePermissionString = (permissionString: string): ModulePermission | null => {
  const parts = permissionString.split('.');
  if (parts.length !== 2) return null;
  
  const [module, permission] = parts;
  if (!Object.values(ModuleType).includes(module as ModuleType) || 
      !Object.values(PermissionType).includes(permission as PermissionType)) {
    return null;
  }
  
  return {
    module: module as ModuleType,
    permission: permission as PermissionType
  };
};

// Role hierarchy constants
export const ADMIN_ROLES: RoleType[] = [
  RoleType.SuperAdmin,
  RoleType.Developer,
  RoleType.Admin
];

export const SYSTEM_ADMIN_ROLES: RoleType[] = [
  RoleType.SuperAdmin,
  RoleType.Developer
];

export const MANAGER_ROLES: RoleType[] = [
  RoleType.IncidentManager,
  RoleType.RiskManager,
  RoleType.PPEManager,
  RoleType.HealthMonitor,
  RoleType.SecurityManager,
  RoleType.WorkflowManager
];

export const READ_ONLY_ROLES: RoleType[] = [
  RoleType.Reporter,
  RoleType.Viewer
];

// Module access mapping (corresponds to backend ModulePermissionMap)
export const MODULE_ACCESS_MAP: Record<RoleType, ModuleType[]> = {
  [RoleType.SuperAdmin]: [
    ModuleType.Dashboard,
    ModuleType.WorkPermitManagement,
    ModuleType.IncidentManagement,
    ModuleType.RiskManagement,
    ModuleType.InspectionManagement,
    ModuleType.AuditManagement,
    ModuleType.PPEManagement,
    ModuleType.TrainingManagement,
    ModuleType.LicenseManagement,
    ModuleType.WasteManagement,
    ModuleType.HealthMonitoring,
    ModuleType.SecurityIncidentManagement,
    ModuleType.Reporting,
    ModuleType.UserManagement,
    ModuleType.ApplicationSettings,
    ModuleType.WorkflowManagement
  ],
  [RoleType.Developer]: [
    ModuleType.Dashboard,
    ModuleType.WorkPermitManagement,
    ModuleType.IncidentManagement,
    ModuleType.RiskManagement,
    ModuleType.InspectionManagement,
    ModuleType.AuditManagement,
    ModuleType.PPEManagement,
    ModuleType.TrainingManagement,
    ModuleType.LicenseManagement,
    ModuleType.WasteManagement,
    ModuleType.HealthMonitoring,
    ModuleType.SecurityIncidentManagement,
    ModuleType.Reporting,
    ModuleType.UserManagement,
    ModuleType.ApplicationSettings,
    ModuleType.WorkflowManagement
  ],
  [RoleType.Admin]: [
    ModuleType.Dashboard,
    ModuleType.WorkPermitManagement,
    ModuleType.IncidentManagement,
    ModuleType.RiskManagement,
    ModuleType.InspectionManagement,
    ModuleType.AuditManagement,
    ModuleType.PPEManagement,
    ModuleType.TrainingManagement,
    ModuleType.LicenseManagement,
    ModuleType.WasteManagement,
    ModuleType.HealthMonitoring,
    ModuleType.SecurityIncidentManagement,
    ModuleType.Reporting,
    ModuleType.UserManagement
    // ApplicationSettings excluded
  ],
  [RoleType.IncidentManager]: [
    ModuleType.Dashboard,
    ModuleType.IncidentManagement,
    ModuleType.Reporting
  ],
  [RoleType.RiskManager]: [
    ModuleType.Dashboard,
    ModuleType.RiskManagement,
    ModuleType.Reporting
  ],
  [RoleType.PPEManager]: [
    ModuleType.Dashboard,
    ModuleType.PPEManagement,
    ModuleType.Reporting
  ],
  [RoleType.HealthMonitor]: [
    ModuleType.Dashboard,
    ModuleType.HealthMonitoring,
    ModuleType.Reporting
  ],
  [RoleType.SecurityManager]: [
    ModuleType.Dashboard,
    ModuleType.SecurityIncidentManagement,
    ModuleType.Reporting
  ],
  [RoleType.SecurityOfficer]: [
    ModuleType.Dashboard,
    ModuleType.SecurityIncidentManagement,
    ModuleType.Reporting
  ],
  [RoleType.ComplianceOfficer]: [
    ModuleType.Dashboard,
    ModuleType.WorkPermitManagement,
    ModuleType.IncidentManagement,
    ModuleType.RiskManagement,
    ModuleType.InspectionManagement,
    ModuleType.AuditManagement,
    ModuleType.PPEManagement,
    ModuleType.TrainingManagement,
    ModuleType.LicenseManagement,
    ModuleType.WasteManagement,
    ModuleType.HealthMonitoring,
    ModuleType.SecurityIncidentManagement,
    ModuleType.Reporting
  ],
  [RoleType.Reporter]: [
    ModuleType.Dashboard,
    ModuleType.WorkPermitManagement,
    ModuleType.IncidentManagement,
    ModuleType.RiskManagement,
    ModuleType.InspectionManagement,
    ModuleType.AuditManagement,
    ModuleType.PPEManagement,
    ModuleType.TrainingManagement,
    ModuleType.LicenseManagement,
    ModuleType.WasteManagement,
    ModuleType.HealthMonitoring,
    ModuleType.Reporting
  ],
  [RoleType.Viewer]: [
    ModuleType.Dashboard
  ],
  [RoleType.WorkflowManager]: [
    ModuleType.Dashboard,
    ModuleType.WorkflowManagement,
    ModuleType.Reporting
  ]
};