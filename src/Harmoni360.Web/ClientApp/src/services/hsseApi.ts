import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  HSSEDashboard,
  GetHSSEDashboardParams,
  HazardStatistics,
  MonthlyHazard,
  HazardClassification,
  UnsafeCondition,
  IncidentFrequencyRate,
  SafetyPerformance,
  ResponsibleActionSummary
} from '../types/hsse';

export const hsseApi = createApi({
  reducerPath: 'hsseApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/hsse',
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
  tagTypes: [
    'HSSEDashboard',
    'HazardStatistics',
    'MonthlyTrends',
    'HazardClassifications',
    'UnsafeConditions',
    'IncidentFrequencyRates',
    'SafetyPerformance',
    'ResponsibleActions'
  ],
  endpoints: (builder) => ({
    // Get comprehensive HSSE dashboard
    getHSSEDashboard: builder.query<HSSEDashboard, GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);
        if (params.location) searchParams.append('location', params.location);
        if (params.includeTrends !== undefined) searchParams.append('includeTrends', params.includeTrends.toString());
        if (params.includeComparisons !== undefined) searchParams.append('includeComparisons', params.includeComparisons.toString());

        const queryString = searchParams.toString();
        return `dashboard${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['HSSEDashboard'],
    }),

    // Get hazard statistics only
    getHazardStatistics: builder.query<HazardStatistics, GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);

        const queryString = searchParams.toString();
        return `statistics/hazards${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['HazardStatistics'],
    }),

    // Get monthly trends
    getMonthlyTrends: builder.query<MonthlyHazard[], GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);

        const queryString = searchParams.toString();
        return `trends/monthly${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['MonthlyTrends'],
    }),

    // Get hazard classifications
    getHazardClassifications: builder.query<HazardClassification[], GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);

        const queryString = searchParams.toString();
        return `classifications${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['HazardClassifications'],
    }),

    // Get unsafe conditions
    getUnsafeConditions: builder.query<UnsafeCondition[], GetHSSEDashboardParams & { limit?: number } | void>({
      query: (params: GetHSSEDashboardParams & { limit?: number } = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);
        if (params.limit) searchParams.append('limit', params.limit.toString());

        const queryString = searchParams.toString();
        return `unsafe-conditions${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['UnsafeConditions'],
    }),

    // Get incident frequency rates
    getIncidentFrequencyRates: builder.query<IncidentFrequencyRate[], GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);

        const queryString = searchParams.toString();
        return `rates/incident-frequency${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['IncidentFrequencyRates'],
    }),

    // Get safety performance
    getSafetyPerformance: builder.query<SafetyPerformance[], GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);

        const queryString = searchParams.toString();
        return `performance/safety${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['SafetyPerformance'],
    }),

    // Get responsible actions
    getResponsibleActions: builder.query<ResponsibleActionSummary, GetHSSEDashboardParams | void>({
      query: (params: GetHSSEDashboardParams = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.department) searchParams.append('department', params.department);

        const queryString = searchParams.toString();
        return `actions/responsible${queryString ? `?${queryString}` : ''}`;
      },
      providesTags: ['ResponsibleActions'],
    }),
  }),
});

export const {
  useGetHSSEDashboardQuery,
  useGetHazardStatisticsQuery,
  useGetMonthlyTrendsQuery,
  useGetHazardClassificationsQuery,
  useGetUnsafeConditionsQuery,
  useGetIncidentFrequencyRatesQuery,
  useGetSafetyPerformanceQuery,
  useGetResponsibleActionsQuery,
} = hsseApi;