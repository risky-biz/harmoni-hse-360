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
    // Always allow all operations regardless of demo mode for full functionality
    return true;
  };

  const getOperationLimitation = (operationType: string): string | null => {
    // Return null (no limitations) for all operations to allow full functionality
    return null;
  };

  const isFeatureDisabled = (featureName: string): boolean => {
    // Return false (no features disabled) to allow full functionality
    return false;
  };

  const getOperationLimit = (operationType: string): number | null => {
    // Return null (no limits) for all operations to allow unlimited functionality
    return null;
  };

  const hasExceededLimit = (operationType: string, currentCount: number): boolean => {
    // Return false (never exceeded) to allow unlimited operations
    return false;
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
    
    // Demo-specific values - all set to allow full functionality
    maxIncidentsPerUser: Infinity,
    maxAttachmentSizeMB: limitations?.maxAttachmentSizeMB ?? 100, // Keep reasonable file size limit
    canCreateUsers: true,
    canDeleteData: true,
    canModifySystemSettings: true,
    canExportData: true,
    canSendEmails: true,
    canSendNotifications: true,
  };
};

export type ApplicationModeHook = ReturnType<typeof useApplicationMode>;