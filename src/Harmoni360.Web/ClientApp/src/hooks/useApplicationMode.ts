import { useMemo } from 'react';
import { 
  useGetApplicationModeInfoQuery, 
  useGetDemoLimitationsQuery,
  useLazyCheckOperationQuery,
  ApplicationModeInfo,
  DemoLimitations 
} from '../api/applicationModeApi';

export const useApplicationMode = () => {
  const { 
    data: modeInfo, 
    isLoading: isModeInfoLoading, 
    error: modeInfoError 
  } = useGetApplicationModeInfoQuery();
  
  const { 
    data: limitations, 
    isLoading: isLimitationsLoading, 
    error: limitationsError 
  } = useGetDemoLimitationsQuery();

  const [checkOperation] = useLazyCheckOperationQuery();

  const combinedData = useMemo(() => {
    if (!modeInfo || !limitations) {
      return null;
    }

    return {
      ...modeInfo,
      limitations,
    };
  }, [modeInfo, limitations]);

  const isLoading = isModeInfoLoading || isLimitationsLoading;
  const error = modeInfoError || limitationsError;

  // Helper functions
  const isDemoMode = combinedData?.isDemoMode ?? false;
  const isProductionMode = combinedData?.environment === 'Production' && !isDemoMode;
  const environment = combinedData?.environment ?? 'Development';

  const canPerformOperation = async (operationType: string): Promise<boolean> => {
    if (!isDemoMode) return true; // Allow all operations in non-demo mode
    
    try {
      const result = await checkOperation(operationType).unwrap();
      return result.isAllowed;
    } catch (error) {
      console.warn(`Failed to check operation ${operationType}:`, error);
      return false; // Fail closed for demo mode
    }
  };

  const getOperationLimitation = (operationType: string): string | null => {
    if (!isDemoMode || !limitations) return null;

    switch (operationType) {
      case 'CreateUser':
        return limitations.canCreateUsers ? null : 'User creation is disabled in demo mode';
      case 'DeleteData':
        return limitations.canDeleteData ? null : 'Data deletion is disabled in demo mode';
      case 'ModifySystemSettings':
        return limitations.canModifySystemSettings ? null : 'System settings modification is disabled in demo mode';
      case 'SendEmail':
        return limitations.canSendEmails ? null : 'Email sending is disabled in demo mode';
      case 'ExportData':
        return limitations.canExportData ? null : 'Data export is disabled in demo mode';
      default:
        return null;
    }
  };

  const isFeatureDisabled = (featureName: string): boolean => {
    if (!isDemoMode || !limitations) return false;
    return limitations.disabledFeatures.includes(featureName);
  };

  const getOperationLimit = (operationType: string): number | null => {
    if (!isDemoMode || !limitations) return null;
    return limitations.operationLimits[operationType] ?? null;
  };

  const hasExceededLimit = (operationType: string, currentCount: number): boolean => {
    const limit = getOperationLimit(operationType);
    return limit !== null && currentCount >= limit;
  };

  return {
    // Data
    modeInfo: combinedData,
    limitations,
    
    // Loading states
    isLoading,
    error,
    
    // Computed values
    isDemoMode,
    isProductionMode,
    environment,
    
    // Helper functions
    canPerformOperation,
    getOperationLimitation,
    isFeatureDisabled,
    getOperationLimit,
    hasExceededLimit,
    
    // Demo-specific values
    maxIncidentsPerUser: limitations?.maxIncidentsPerUser ?? Infinity,
    maxAttachmentSizeMB: limitations?.maxAttachmentSizeMB ?? 100,
    canCreateUsers: limitations?.canCreateUsers ?? true,
    canDeleteData: limitations?.canDeleteData ?? true,
    canModifySystemSettings: limitations?.canModifySystemSettings ?? true,
    canExportData: limitations?.canExportData ?? true,
    canSendEmails: limitations?.canSendEmails ?? true,
    canSendNotifications: limitations?.canSendNotifications ?? true,
  };
};

export type ApplicationModeHook = ReturnType<typeof useApplicationMode>;