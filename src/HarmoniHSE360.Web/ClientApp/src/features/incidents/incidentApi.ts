import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// Types
export interface IncidentDto {
  id: number;
  title: string;
  description: string;
  severity: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  status: 'Reported' | 'UnderInvestigation' | 'AwaitingAction' | 'Resolved' | 'Closed';
  incidentDate: string;
  location: string;
  reporterId: number;
  reporterName: string;
  investigatorId?: number;
  investigatorName?: string;
  createdAt: string;
  lastModifiedAt?: string;
  latitude?: number;
  longitude?: number;
}

export interface CreateIncidentRequest {
  title: string;
  description: string;
  severity: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  incidentDate: string;
  location: string;
  latitude?: number;
  longitude?: number;
}

export interface UpdateIncidentRequest {
  title?: string;
  description?: string;
  severity?: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  status?: 'Reported' | 'UnderInvestigation' | 'AwaitingAction' | 'Resolved' | 'Closed';
  location?: string;
}

export interface IncidentListParams {
  page?: number;
  pageSize?: number;
  status?: string;
  severity?: string;
  search?: string;
}

export interface IncidentStatistics {
  totalIncidents: number;
  openIncidents: number;
  closedIncidents: number;
  criticalIncidents: number;
  incidentsByMonth: Array<{
    month: string;
    count: number;
  }>;
}

// API slice
export const incidentApi = createApi({
  reducerPath: 'incidentApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/incident',
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
  tagTypes: ['Incident', 'IncidentStatistics'],
  endpoints: (builder) => ({
    // Create incident
    createIncident: builder.mutation<IncidentDto, CreateIncidentRequest>({
      query: (incident) => ({
        url: '',
        method: 'POST',
        body: incident,
      }),
      invalidatesTags: ['Incident', 'IncidentStatistics'],
    }),

    // Get incidents list
    getIncidents: builder.query<IncidentDto[], IncidentListParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.status) searchParams.append('status', params.status);
        if (params.severity) searchParams.append('severity', params.severity);
        if (params.search) searchParams.append('search', params.search);

        return {
          url: `?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['Incident'],
    }),

    // Get incident by ID
    getIncident: builder.query<IncidentDto, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'GET',
      }),
      providesTags: (_, __, id) => [{ type: 'Incident' as const, id }],
    }),

    // Update incident
    updateIncident: builder.mutation<IncidentDto, { id: number; incident: UpdateIncidentRequest }>({
      query: ({ id, incident }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: incident,
      }),
      invalidatesTags: (_, __, { id }) => [
        { type: 'Incident' as const, id },
        'Incident',
        'IncidentStatistics',
      ],
    }),

    // Get my incidents
    getMyIncidents: builder.query<IncidentDto[], IncidentListParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.status) searchParams.append('status', params.status);
        if (params.severity) searchParams.append('severity', params.severity);

        return {
          url: `/my-reports?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['Incident'],
    }),

    // Get incident statistics
    getIncidentStatistics: builder.query<IncidentStatistics, void>({
      query: () => ({
        url: '/statistics',
        method: 'GET',
      }),
      providesTags: ['IncidentStatistics'],
    }),

    // Upload incident attachments
    uploadIncidentAttachments: builder.mutation<
      { attachmentIds: number[] },
      { incidentId: number; files: File[] }
    >({
      query: ({ incidentId, files }) => {
        const formData = new FormData();
        files.forEach((file, index) => {
          formData.append(`files[${index}]`, file);
        });

        return {
          url: `/${incidentId}/attachments`,
          method: 'POST',
          body: formData,
          formData: true,
        };
      },
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
      ],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useCreateIncidentMutation,
  useGetIncidentsQuery,
  useGetIncidentQuery,
  useUpdateIncidentMutation,
  useGetMyIncidentsQuery,
  useGetIncidentStatisticsQuery,
  useUploadIncidentAttachmentsMutation,
} = incidentApi;