import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface ApplicationModeInfo {
  isDemoMode: boolean;
  environment: 'Development' | 'Demo' | 'Staging' | 'Production';
  environmentDisplayName: string;
  bannerMessage: string;
  bannerColor: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info';
  showDemoIndicator: boolean;
  limitations: DemoLimitations;
}

export interface DemoLimitations {
  canCreateUsers: boolean;
  canDeleteData: boolean;
  canModifySystemSettings: boolean;
  canExportData: boolean;
  canSendEmails: boolean;
  canSendNotifications: boolean;
  maxIncidentsPerUser: number;
  maxAttachmentSizeMB: number;
  disabledFeatures: string[];
  operationLimits: Record<string, number>;
}

export interface OperationCheckResult {
  operationType: string;
  isAllowed: boolean;
  reason: string;
}

export interface EnvironmentStatus {
  environment: string;
  isDemoMode: boolean;
  isProductionMode: boolean;
  version: string;
  timestamp: string;
}

export const applicationModeApi = createApi({
  reducerPath: 'applicationModeApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/applicationmode',
    prepareHeaders: (headers, { getState }) => {
      // Add auth token if available
      const token = (getState() as any).auth?.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
    responseHandler: async (response) => {
      // Check if response is HTML (likely 404 page) instead of JSON
      const contentType = response.headers.get('content-type');
      if (contentType && contentType.includes('text/html')) {
        throw new Error('API endpoint not found - received HTML instead of JSON');
      }
      
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
      
      try {
        return await response.json();
      } catch (error) {
        throw new Error('Invalid JSON response from server');
      }
    },
  }),
  tagTypes: ['ApplicationMode'],
  endpoints: (builder) => ({
    getApplicationModeInfo: builder.query<ApplicationModeInfo, void>({
      query: () => 'info',
      providesTags: ['ApplicationMode'],
      transformErrorResponse: (response: any) => {
        // Provide helpful guidance for development mode
        if (import.meta.env.DEV) {
          console.warn('ApplicationModeAPI: Backend API unavailable. Please ensure the backend is running and database is seeded.');
        }
        return response;
      },
    }),
    getDemoLimitations: builder.query<DemoLimitations, void>({
      query: () => 'limitations',
      providesTags: ['ApplicationMode'],
    }),
    checkOperation: builder.query<OperationCheckResult, string>({
      query: (operationType) => `check-operation/${operationType}`,
    }),
    getEnvironmentStatus: builder.query<EnvironmentStatus, void>({
      query: () => 'status',
    }),
  }),
});

export const {
  useGetApplicationModeInfoQuery,
  useGetDemoLimitationsQuery,
  useCheckOperationQuery,
  useLazyCheckOperationQuery,
  useGetEnvironmentStatusQuery,
} = applicationModeApi;