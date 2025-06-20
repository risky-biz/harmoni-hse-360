import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// Types for Module Configuration API
export interface ModuleConfigurationDto {
  id: number;
  moduleType: ModuleType;
  moduleTypeName: string;
  displayName: string;
  description?: string;
  isEnabled: boolean;
  settings?: string;
  displayOrder: number;
  parentModuleType?: ModuleType;
  canBeDisabled: boolean;
  disableWarnings: string[];
  dependencies: ModuleDependencyDto[];
  subModules: ModuleConfigurationDto[];
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  createdAt?: string;
  createdBy?: string;
}

export interface ModuleDependencyDto {
  id: number;
  moduleType: ModuleType;
  dependsOnModuleType: ModuleType;
  isRequired: boolean;
  dependencyType: DependencyType;
  description?: string;
  module: ModuleConfigurationDto;
  dependsOnModule: ModuleConfigurationDto;
}

export interface ModuleConfigurationDashboardDto {
  totalModules: number;
  enabledModules: number;
  disabledModules: number;
  criticalModules: number;
  modulesWithDependencies: number;
  moduleStatusSummary: ModuleStatusSummaryDto[];
  recentActivity: ModuleConfigurationAuditLogDto[];
  warnings: ModuleWarningDto[];
}

export interface ModuleStatusSummaryDto {
  moduleName: string;
  isEnabled: boolean;
  canBeDisabled: boolean;
  dependentModulesCount: number;
  dependenciesCount: number;
}

export interface ModuleConfigurationAuditLogDto {
  id: number;
  moduleType: ModuleType;
  moduleTypeName: string;
  action: string;
  oldValue?: string;
  newValue?: string;
  userId: string;
  userName: string;
  userEmail: string;
  timestamp: string;
  ipAddress?: string;
  userAgent?: string;
  context?: string;
}

export interface ModuleWarningDto {
  moduleName: string;
  warningType: string;
  message: string;
  severity: string;
}

export interface CanDisableModuleResponse {
  canDisable: boolean;
  warnings: string[];
}

export interface EnableModuleRequest {
  context?: string;
}

export interface DisableModuleRequest {
  context?: string;
  forceDisable?: boolean;
}

export interface UpdateModuleSettingsRequest {
  settings?: string;
  context?: string;
}

// Enums
export enum ModuleType {
  IncidentManagement = 1,
  HazardManagement = 2,
  InspectionManagement = 3,
  TrainingManagement = 4,
  PPEManagement = 5,
  LicenseManagement = 6,
  AuditManagement = 7,
  WorkPermitManagement = 8,
  WasteManagement = 9,
  SecurityManagement = 10,
  HealthManagement = 11,
  ApplicationSettings = 12,
  UserManagement = 13
}

export enum DependencyType {
  Hard = 1,
  Soft = 2
}

export const moduleConfigurationApi = createApi({
  reducerPath: 'moduleConfigurationApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/module-configuration',
    prepareHeaders: (headers, { getState }) => {
      // Get token from auth state
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      headers.set('content-type', 'application/json');
      return headers;
    },
  }),
  tagTypes: ['ModuleConfiguration', 'ModuleDashboard', 'ModuleAudit'],
  endpoints: (builder) => ({
    // Get all module configurations
    getModuleConfigurations: builder.query<ModuleConfigurationDto[], {
      isEnabled?: boolean;
      includeDependencies?: boolean;
      includeSubModules?: boolean;
      includeAuditProperties?: boolean;
    }>({
      query: (params = {}) => ({
        url: '',
        params: {
          isEnabled: params.isEnabled,
          includeDependencies: params.includeDependencies ?? true,
          includeSubModules: params.includeSubModules ?? true,
          includeAuditProperties: params.includeAuditProperties ?? false
        }
      }),
      providesTags: (result) => [
        'ModuleConfiguration',
        ...(result || []).map(({ id }) => ({ type: 'ModuleConfiguration' as const, id }))
      ]
    }),

    // Get enabled module configurations only
    getEnabledModuleConfigurations: builder.query<ModuleConfigurationDto[], void>({
      query: () => 'enabled',
      providesTags: (result) => [
        'ModuleConfiguration',
        ...(result || []).map(({ id }) => ({ type: 'ModuleConfiguration' as const, id }))
      ]
    }),

    // Get module configuration by module type
    getModuleConfigurationByType: builder.query<ModuleConfigurationDto, {
      moduleType: ModuleType;
      includeDependencies?: boolean;
      includeSubModules?: boolean;
      includeAuditProperties?: boolean;
    }>({
      query: ({ moduleType, ...params }) => ({
        url: `/${moduleType}`,
        params: {
          includeDependencies: params.includeDependencies ?? true,
          includeSubModules: params.includeSubModules ?? true,
          includeAuditProperties: params.includeAuditProperties ?? true
        }
      }),
      providesTags: (result, error, { moduleType }) => [
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Get module configuration dashboard
    getModuleConfigurationDashboard: builder.query<ModuleConfigurationDashboardDto, { recentActivityCount?: number }>({
      query: (params = {}) => ({
        url: 'dashboard',
        params: {
          recentActivityCount: params.recentActivityCount ?? 10
        }
      }),
      providesTags: ['ModuleDashboard']
    }),

    // Enable a module
    enableModule: builder.mutation<{ message: string }, { moduleType: ModuleType; request?: EnableModuleRequest }>({
      query: ({ moduleType, request = {} }) => ({
        url: `/${moduleType}/enable`,
        method: 'POST',
        body: request
      }),
      invalidatesTags: (result, error, { moduleType }) => [
        'ModuleConfiguration',
        'ModuleDashboard',
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Disable a module
    disableModule: builder.mutation<{ message: string }, { moduleType: ModuleType; request?: DisableModuleRequest }>({
      query: ({ moduleType, request = {} }) => ({
        url: `/${moduleType}/disable`,
        method: 'POST',
        body: request
      }),
      invalidatesTags: (result, error, { moduleType }) => [
        'ModuleConfiguration',
        'ModuleDashboard',
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Check if a module can be disabled
    canDisableModule: builder.query<CanDisableModuleResponse, ModuleType>({
      query: (moduleType) => `/${moduleType}/can-disable`,
      providesTags: (result, error, moduleType) => [
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Update module settings
    updateModuleSettings: builder.mutation<{ message: string }, { 
      moduleType: ModuleType; 
      request: UpdateModuleSettingsRequest 
    }>({
      query: ({ moduleType, request }) => ({
        url: `/${moduleType}/settings`,
        method: 'PUT',
        body: request
      }),
      invalidatesTags: (result, error, { moduleType }) => [
        'ModuleConfiguration',
        'ModuleDashboard',
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Get module settings
    getModuleSettings: builder.query<{ settings?: string }, ModuleType>({
      query: (moduleType) => `/${moduleType}/settings`,
      providesTags: (result, error, moduleType) => [
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Get module dependencies
    getModuleDependencies: builder.query<ModuleDependencyDto[], ModuleType>({
      query: (moduleType) => `/${moduleType}/dependencies`,
      providesTags: (result, error, moduleType) => [
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Get modules that depend on this module
    getDependentModules: builder.query<ModuleDependencyDto[], ModuleType>({
      query: (moduleType) => `/${moduleType}/dependents`,
      providesTags: (result, error, moduleType) => [
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Get module audit trail
    getModuleAuditTrail: builder.query<ModuleConfigurationAuditLogDto[], ModuleType>({
      query: (moduleType) => `/${moduleType}/audit-trail`,
      providesTags: (result, error, moduleType) => [
        'ModuleAudit',
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    }),

    // Get recent module activity
    getRecentModuleActivity: builder.query<ModuleConfigurationAuditLogDto[], { count?: number }>({
      query: (params = {}) => ({
        url: 'recent-activity',
        params: {
          count: params.count ?? 10
        }
      }),
      providesTags: ['ModuleAudit']
    }),

    // Get module hierarchy
    getModuleHierarchy: builder.query<ModuleConfigurationDto[], void>({
      query: () => 'hierarchy',
      providesTags: ['ModuleConfiguration']
    }),

    // Validate module dependencies
    validateModuleDependencies: builder.query<{ isValid: boolean; moduleType: ModuleType }, ModuleType>({
      query: (moduleType) => `/${moduleType}/validate-dependencies`,
      providesTags: (result, error, moduleType) => [
        { type: 'ModuleConfiguration', id: moduleType }
      ]
    })
  })
});

// Export hooks for usage in functional components
export const {
  useGetModuleConfigurationsQuery,
  useGetEnabledModuleConfigurationsQuery,
  useGetModuleConfigurationByTypeQuery,
  useGetModuleConfigurationDashboardQuery,
  useEnableModuleMutation,
  useDisableModuleMutation,
  useCanDisableModuleQuery,
  useUpdateModuleSettingsMutation,
  useGetModuleSettingsQuery,
  useGetModuleDependenciesQuery,
  useGetDependentModulesQuery,
  useGetModuleAuditTrailQuery,
  useGetRecentModuleActivityQuery,
  useGetModuleHierarchyQuery,
  useValidateModuleDependenciesQuery
} = moduleConfigurationApi;

// Helper function to get module display name
export const getModuleDisplayName = (moduleType: ModuleType): string => {
  const moduleNames: Record<ModuleType, string> = {
    [ModuleType.IncidentManagement]: 'Incident Management',
    [ModuleType.HazardManagement]: 'Hazard Management',
    [ModuleType.InspectionManagement]: 'Inspection Management',
    [ModuleType.TrainingManagement]: 'Training Management',
    [ModuleType.PPEManagement]: 'PPE Management',
    [ModuleType.LicenseManagement]: 'License Management',
    [ModuleType.AuditManagement]: 'Audit Management',
    [ModuleType.WorkPermitManagement]: 'Work Permit Management',
    [ModuleType.WasteManagement]: 'Waste Management',
    [ModuleType.SecurityManagement]: 'Security Management',
    [ModuleType.HealthManagement]: 'Health Management',
    [ModuleType.ApplicationSettings]: 'Application Settings',
    [ModuleType.UserManagement]: 'User Management'
  };
  
  return moduleNames[moduleType] || `Module ${moduleType}`;
};

// Helper function to get module icon
export const getModuleIcon = (moduleType: ModuleType): string => {
  const moduleIcons: Record<ModuleType, string> = {
    [ModuleType.IncidentManagement]: 'warning',
    [ModuleType.HazardManagement]: 'alert-triangle',
    [ModuleType.InspectionManagement]: 'clipboard-check',
    [ModuleType.TrainingManagement]: 'graduation-cap',
    [ModuleType.PPEManagement]: 'shield',
    [ModuleType.LicenseManagement]: 'file-text',
    [ModuleType.AuditManagement]: 'search',
    [ModuleType.WorkPermitManagement]: 'file-check',
    [ModuleType.WasteManagement]: 'trash-2',
    [ModuleType.SecurityManagement]: 'lock',
    [ModuleType.HealthManagement]: 'heart',
    [ModuleType.ApplicationSettings]: 'settings',
    [ModuleType.UserManagement]: 'users'
  };
  
  return moduleIcons[moduleType] || 'box';
};