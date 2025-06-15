import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { SecurityIncident, SecurityIncidentDetail, SecurityIncidentList, SecurityDashboard, ThreatAssessment, SecurityControl, CreateSecurityIncidentRequest, UpdateSecurityIncidentRequest, CreateThreatAssessmentRequest } from '../../types/security';
import type { PagedList } from '../../types/common';

export const securityApi = createApi({
  reducerPath: 'securityApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/security-incident',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['SecurityIncident', 'SecurityDashboard', 'ThreatAssessment'],
  endpoints: (builder) => ({
    getSecurityIncidents: builder.query<PagedList<SecurityIncidentList>, {
      page?: number;
      pageSize?: number;
      type?: string;
      category?: string;
      severity?: string;
      status?: string;
      startDate?: string;
      endDate?: string;
      location?: string;
      searchTerm?: string;
      onlyMyIncidents?: boolean;
      onlyOpenIncidents?: boolean;
      onlyOverdueIncidents?: boolean;
      sortBy?: string;
      sortDescending?: boolean;
    }>({
      query: (params) => ({
        url: '',
        params,
      }),
      providesTags: (result) =>
        result
          ? [
              ...result.items.map(({ id }) => ({ type: 'SecurityIncident' as const, id })),
              { type: 'SecurityIncident', id: 'LIST' },
            ]
          : [{ type: 'SecurityIncident', id: 'LIST' }],
    }),

    getSecurityIncident: builder.query<SecurityIncidentDetail, number>({
      query: (id) => `${id}`,
      providesTags: (result, error, id) => [{ type: 'SecurityIncident', id }],
    }),

    createSecurityIncident: builder.mutation<SecurityIncident, CreateSecurityIncidentRequest>({
      query: (incident) => ({
        url: '',
        method: 'POST',
        body: incident,
      }),
      invalidatesTags: [
        { type: 'SecurityIncident', id: 'LIST' },
        { type: 'SecurityDashboard', id: 'DASHBOARD' },
      ],
    }),

    updateSecurityIncident: builder.mutation<SecurityIncident, { id: number; incident: UpdateSecurityIncidentRequest }>({
      query: ({ id, incident }) => ({
        url: `${id}`,
        method: 'PUT',
        body: { ...incident, id },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'SecurityIncident', id },
        { type: 'SecurityIncident', id: 'LIST' },
        { type: 'SecurityDashboard', id: 'DASHBOARD' },
      ],
    }),

    deleteSecurityIncident: builder.mutation<void, number>({
      query: (id) => ({
        url: `${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, id) => [
        { type: 'SecurityIncident', id },
        { type: 'SecurityIncident', id: 'LIST' },
        { type: 'SecurityDashboard', id: 'DASHBOARD' },
      ],
    }),

    getSecurityDashboard: builder.query<SecurityDashboard, {
      startDate?: string;
      endDate?: string;
      includeThreatIntel?: boolean;
      includeTrends?: boolean;
      includeMetrics?: boolean;
    }>({
      query: (params) => ({
        url: 'dashboard',
        params,
      }),
      providesTags: [{ type: 'SecurityDashboard', id: 'DASHBOARD' }],
    }),

    createThreatAssessment: builder.mutation<ThreatAssessment, { id: number; assessment: CreateThreatAssessmentRequest }>({
      query: ({ id, assessment }) => ({
        url: `${id}/threat-assessment`,
        method: 'POST',
        body: { ...assessment, securityIncidentId: id },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'SecurityIncident', id },
        { type: 'ThreatAssessment', id },
        { type: 'SecurityDashboard', id: 'DASHBOARD' },
      ],
    }),

    escalateIncident: builder.mutation<{ message: string; newStatus: string }, { id: number; reason: string; escalatedById: number }>({
      query: ({ id, reason, escalatedById }) => ({
        url: `${id}/escalate`,
        method: 'POST',
        body: { reason, escalatedById },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'SecurityIncident', id },
        { type: 'SecurityIncident', id: 'LIST' },
        { type: 'SecurityDashboard', id: 'DASHBOARD' },
      ],
    }),

    assignIncident: builder.mutation<{ message: string }, { id: number; assigneeId: number; assignedById: number }>({
      query: ({ id, assigneeId, assignedById }) => ({
        url: `${id}/assign`,
        method: 'POST',
        body: { assigneeId, assignedById },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'SecurityIncident', id },
        { type: 'SecurityIncident', id: 'LIST' },
      ],
    }),

    closeIncident: builder.mutation<SecurityIncidentDetail, { id: number; rootCause: string; closedById: number }>({
      query: ({ id, rootCause, closedById }) => ({
        url: `${id}/close`,
        method: 'POST',
        body: { rootCause, closedById },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'SecurityIncident', id },
        { type: 'SecurityIncident', id: 'LIST' },
        { type: 'SecurityDashboard', id: 'DASHBOARD' },
      ],
    }),

    getRecommendedControls: builder.query<SecurityControl[], number>({
      query: (id) => `${id}/controls/recommendations`,
    }),

    getRelatedIncidents: builder.query<SecurityIncident[], number>({
      query: (id) => `${id}/related`,
    }),

    generateComplianceReport: builder.mutation<any, number>({
      query: (id) => ({
        url: `${id}/compliance-report`,
        method: 'POST',
      }),
    }),

    getIncidentAnalytics: builder.query<any, number>({
      query: (id) => `${id}/analytics`,
    }),

    linkToSafetyIncident: builder.mutation<SecurityIncident, { id: number; safetyIncidentId: number }>({
      query: ({ id, safetyIncidentId }) => ({
        url: `${id}/link-safety-incident`,
        method: 'POST',
        body: { safetyIncidentId },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'SecurityIncident', id },
      ],
    }),

    sendNotification: builder.mutation<{ message: string }, { id: number; message: string; recipientIds: number[] }>({
      query: ({ id, message, recipientIds }) => ({
        url: `${id}/notify`,
        method: 'POST',
        body: { message, recipientIds },
      }),
    }),
  }),
});

export const {
  useGetSecurityIncidentsQuery,
  useGetSecurityIncidentQuery,
  useCreateSecurityIncidentMutation,
  useUpdateSecurityIncidentMutation,
  useDeleteSecurityIncidentMutation,
  useGetSecurityDashboardQuery,
  useCreateThreatAssessmentMutation,
  useEscalateIncidentMutation,
  useAssignIncidentMutation,
  useCloseIncidentMutation,
  useGetRecommendedControlsQuery,
  useGetRelatedIncidentsQuery,
  useGenerateComplianceReportMutation,
  useGetIncidentAnalyticsQuery,
  useLinkToSafetyIncidentMutation,
  useSendNotificationMutation,
} = securityApi;