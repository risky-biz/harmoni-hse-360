import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { CompanyConfiguration, useGetCompanyConfigurationQuery } from '../services/companyConfigurationService';

interface CompanyConfigurationContextType {
  companyConfig: CompanyConfiguration | null;
  isLoading: boolean;
  error: any;
  refreshConfig: () => void;
  
  // Helper methods for commonly used values
  getCompanyName: () => string;
  getWebsiteUrl: () => string;
  getEmergencyContact: () => string;
  getDefaultMapCenter: () => { lat: number; lng: number } | null;
  getPrimaryColor: () => string;
  getSecondaryColor: () => string;
}

const CompanyConfigurationContext = createContext<CompanyConfigurationContextType | undefined>(undefined);

interface CompanyConfigurationProviderProps {
  children: ReactNode;
}

// Default fallback values
const DEFAULT_VALUES = {
  companyName: 'Harmoni360',
  websiteUrl: '#',
  emergencyContact: 'Please contact your local emergency services',
  primaryColor: '#1976d2',
  secondaryColor: '#424242',
  defaultMapCenter: { lat: -6.1751, lng: 106.8650 }, // Jakarta, Indonesia
};

export const CompanyConfigurationProvider: React.FC<CompanyConfigurationProviderProps> = ({ children }) => {
  const { data: companyConfig, isLoading, error, refetch } = useGetCompanyConfigurationQuery();
  const [cachedConfig, setCachedConfig] = useState<CompanyConfiguration | null>(null);

  // Cache the configuration to avoid repeated API calls
  useEffect(() => {
    if (companyConfig) {
      setCachedConfig(companyConfig);
      // Store in localStorage for offline access
      localStorage.setItem('companyConfig', JSON.stringify(companyConfig));
    } else if (!isLoading && !companyConfig) {
      // Try to load from localStorage if API call fails
      const stored = localStorage.getItem('companyConfig');
      if (stored) {
        try {
          setCachedConfig(JSON.parse(stored));
        } catch (e) {
          console.warn('Failed to parse stored company configuration');
        }
      }
    }
  }, [companyConfig, isLoading]);

  const getCompanyName = (): string => {
    return cachedConfig?.companyName || DEFAULT_VALUES.companyName;
  };

  const getWebsiteUrl = (): string => {
    return cachedConfig?.websiteUrl || DEFAULT_VALUES.websiteUrl;
  };

  const getEmergencyContact = (): string => {
    if (cachedConfig?.emergencyContactNumber) {
      return `Please contact ${cachedConfig.companyName || 'your organization'} immediately at ${cachedConfig.emergencyContactNumber}.`;
    }
    return DEFAULT_VALUES.emergencyContact;
  };

  const getDefaultMapCenter = (): { lat: number; lng: number } | null => {
    if (cachedConfig?.hasGeographicCoordinates && 
        cachedConfig.defaultLatitude && 
        cachedConfig.defaultLongitude) {
      return {
        lat: cachedConfig.defaultLatitude,
        lng: cachedConfig.defaultLongitude,
      };
    }
    return DEFAULT_VALUES.defaultMapCenter;
  };

  const getPrimaryColor = (): string => {
    return cachedConfig?.primaryColor || DEFAULT_VALUES.primaryColor;
  };

  const getSecondaryColor = (): string => {
    return cachedConfig?.secondaryColor || DEFAULT_VALUES.secondaryColor;
  };

  const refreshConfig = () => {
    refetch();
  };

  const contextValue: CompanyConfigurationContextType = {
    companyConfig: cachedConfig,
    isLoading,
    error,
    refreshConfig,
    getCompanyName,
    getWebsiteUrl,
    getEmergencyContact,
    getDefaultMapCenter,
    getPrimaryColor,
    getSecondaryColor,
  };

  return (
    <CompanyConfigurationContext.Provider value={contextValue}>
      {children}
    </CompanyConfigurationContext.Provider>
  );
};

export const useCompanyConfiguration = (): CompanyConfigurationContextType => {
  const context = useContext(CompanyConfigurationContext);
  if (context === undefined) {
    throw new Error('useCompanyConfiguration must be used within a CompanyConfigurationProvider');
  }
  return context;
};

// Hook for getting just the company name (most common use case)
export const useCompanyName = (): string => {
  const { getCompanyName } = useCompanyConfiguration();
  return getCompanyName();
};

// Hook for getting just the website URL
export const useWebsiteUrl = (): string => {
  const { getWebsiteUrl } = useCompanyConfiguration();
  return getWebsiteUrl();
};

// Hook for getting emergency contact info
export const useEmergencyContact = (): string => {
  const { getEmergencyContact } = useCompanyConfiguration();
  return getEmergencyContact();
};

// Hook for getting map center coordinates
export const useDefaultMapCenter = (): { lat: number; lng: number } => {
  const { getDefaultMapCenter } = useCompanyConfiguration();
  return getDefaultMapCenter() || DEFAULT_VALUES.defaultMapCenter;
};