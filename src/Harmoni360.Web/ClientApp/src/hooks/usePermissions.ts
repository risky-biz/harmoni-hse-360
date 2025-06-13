import { useMemo } from 'react';
import { useSelector } from 'react-redux';
import { selectUser } from '../features/auth/authSlice';
import {
  ModuleType,
  PermissionType,
  RoleType,
  UserPermissions,
  createPermissionString,
  parsePermissionString,
  ADMIN_ROLES,
  SYSTEM_ADMIN_ROLES,
  MODULE_ACCESS_MAP
} from '../types/permissions';

/**
 * Custom hook for checking user permissions and roles
 * Provides a comprehensive interface for authorization checks in React components
 */
export const usePermissions = (): UserPermissions => {
  const user = useSelector(selectUser);

  return useMemo(() => {
    if (!user) {
      // Return empty permissions for unauthenticated users
      return {
        roles: [],
        permissions: [],
        hasRole: () => false,
        hasPermission: () => false,
        hasModuleAccess: () => false,
        canPerformAction: () => false,
        isAdmin: () => false,
        isSystemAdmin: () => false
      };
    }

    const userRoles = user.roles.map(role => role as RoleType);
    const userPermissions = user.permissions || [];

    const permissions: UserPermissions = {
      roles: userRoles,
      permissions: userPermissions,

      /**
       * Check if user has a specific role
       */
      hasRole: (role: RoleType): boolean => {
        return userRoles.includes(role);
      },

      /**
       * Check if user has a specific permission for a module
       */
      hasPermission: (module: ModuleType, permission: PermissionType): boolean => {
        const permissionString = createPermissionString(module, permission);
        return userPermissions.includes(permissionString);
      },

      /**
       * Check if user has any access to a module
       */
      hasModuleAccess: (module: ModuleType): boolean => {
        // Check if user has any permission for this module
        return userPermissions.some(perm => {
          const parsed = parsePermissionString(perm);
          return parsed?.module === module;
        });
      },

      /**
       * Check if user can perform a specific action across any module they have access to
       */
      canPerformAction: (permission: PermissionType): boolean => {
        return userPermissions.some(perm => {
          const parsed = parsePermissionString(perm);
          return parsed?.permission === permission;
        });
      },

      /**
       * Check if user has administrative privileges
       */
      isAdmin: (): boolean => {
        return userRoles.some(role => ADMIN_ROLES.includes(role));
      },

      /**
       * Check if user has system administrative privileges
       */
      isSystemAdmin: (): boolean => {
        return userRoles.some(role => SYSTEM_ADMIN_ROLES.includes(role));
      }
    };

    return permissions;
  }, [user]);
};

/**
 * Hook to check if user has access to any of the specified modules
 */
export const useHasAnyModuleAccess = (modules: ModuleType[]): boolean => {
  const permissions = usePermissions();
  
  return useMemo(() => {
    return modules.some(module => permissions.hasModuleAccess(module));
  }, [permissions, modules]);
};

/**
 * Hook to check if user has all specified permissions
 */
export const useHasAllPermissions = (requiredPermissions: Array<{ module: ModuleType; permission: PermissionType }>): boolean => {
  const permissions = usePermissions();
  
  return useMemo(() => {
    return requiredPermissions.every(({ module, permission }) => 
      permissions.hasPermission(module, permission)
    );
  }, [permissions, requiredPermissions]);
};

/**
 * Hook to get all modules the user has access to
 */
export const useAccessibleModules = (): ModuleType[] => {
  const permissions = usePermissions();
  
  return useMemo(() => {
    if (!permissions.roles.length) return [];
    
    // Get all modules from role-based access map
    const accessibleModules = new Set<ModuleType>();
    
    permissions.roles.forEach(role => {
      const roleModules = MODULE_ACCESS_MAP[role] || [];
      roleModules.forEach(module => accessibleModules.add(module));
    });
    
    return Array.from(accessibleModules);
  }, [permissions.roles]);
};

/**
 * Hook to get user's permissions for a specific module
 */
export const useModulePermissions = (module: ModuleType): PermissionType[] => {
  const permissions = usePermissions();
  
  return useMemo(() => {
    return permissions.permissions
      .map(parsePermissionString)
      .filter(parsed => parsed?.module === module)
      .map(parsed => parsed!.permission);
  }, [permissions.permissions, module]);
};

/**
 * Hook for role-based conditional rendering
 */
export const useRoleAccess = () => {
  const permissions = usePermissions();
  
  return useMemo(() => ({
    isSuperAdmin: permissions.hasRole(RoleType.SuperAdmin),
    isDeveloper: permissions.hasRole(RoleType.Developer),
    isAdmin: permissions.hasRole(RoleType.Admin),
    isIncidentManager: permissions.hasRole(RoleType.IncidentManager),
    isRiskManager: permissions.hasRole(RoleType.RiskManager),
    isPPEManager: permissions.hasRole(RoleType.PPEManager),
    isHealthMonitor: permissions.hasRole(RoleType.HealthMonitor),
    isReporter: permissions.hasRole(RoleType.Reporter),
    isViewer: permissions.hasRole(RoleType.Viewer),
    isAnyAdmin: permissions.isAdmin(),
    isSystemAdmin: permissions.isSystemAdmin()
  }), [permissions]);
};