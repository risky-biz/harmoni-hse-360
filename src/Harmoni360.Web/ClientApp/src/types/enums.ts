export enum UserStatus {
  Active = 1,
  Inactive = 2,
  Suspended = 3,
  PendingActivation = 4,
  Terminated = 5,
}

export enum ModuleType {
  Dashboard = 1,
  UserManagement = 2,
  RoleManagement = 3,
  AuditTrail = 4,
  IncidentManagement = 5,
  RiskManagement = 6,
  PPEManagement = 7,
  HealthMonitoring = 8,
  TrainingManagement = 9,
  LicenseManagement = 10,
  ApplicationSettings = 11,
  Reporting = 12,
  WorkPermitSettings = 13,
  Inspections = 14,
  Audits = 15,
  Hazards = 16,
  SecurityIncidents = 17,
  WasteReports = 18,
  ModuleConfiguration = 19,
}

export enum PermissionType {
  Create = 1,
  Read = 2,
  Update = 3,
  Delete = 4,
  Assign = 5,
  Export = 6,
  Import = 7,
  Approve = 8,
}

export enum RoleType {
  SuperAdmin = 'SuperAdmin',
  Developer = 'Developer',
  Admin = 'Admin',
  SecurityManager = 'SecurityManager',
  ComplianceOfficer = 'ComplianceOfficer',
  IncidentManager = 'IncidentManager',
  RiskManager = 'RiskManager',
  PPEManager = 'PPEManager',
  HealthMonitor = 'HealthMonitor',
  SafetyOfficer = 'SafetyOfficer',
  DepartmentHead = 'DepartmentHead',
  HSEManager = 'HSEManager',
  Reporter = 'Reporter',
  Viewer = 'Viewer',
}