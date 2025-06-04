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
  reporterId?: number;
  reporterName: string;
  reporterEmail?: string;
  reporterDepartment?: string;
  investigatorId?: number;
  investigatorName?: string;
  createdAt: string;
  lastModifiedAt?: string;
  latitude?: number;
  longitude?: number;
  injuryType?: string;
  medicalTreatmentProvided?: boolean;
  emergencyServicesContacted?: boolean;
  witnessNames?: string;
  immediateActionsTaken?: string;
  attachmentsCount?: number;
  involvedPersonsCount?: number;
  correctiveActionsCount?: number;
  correctiveActions?: CorrectiveActionDto[];
  createdBy?: string;
  lastModifiedBy?: string;
}

export interface CreateIncidentRequest {
  title: string;
  description: string;
  severity: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  incidentDate: string;
  location: string;
  latitude?: number;
  longitude?: number;
  witnessNames?: string;
  immediateActionsTaken?: string;
}

export interface UpdateIncidentRequest {
  title?: string;
  description?: string;
  severity?: 'Minor' | 'Moderate' | 'Serious' | 'Critical';
  status?: 'Reported' | 'UnderInvestigation' | 'AwaitingAction' | 'Resolved' | 'Closed';
  location?: string;
}

export interface IncidentListParams {
  pageNumber?: number;
  pageSize?: number;
  status?: string;
  severity?: string;
  searchTerm?: string;
}

export interface GetIncidentsResponse {
  incidents: IncidentDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
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

export interface UserDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  fullName: string;
}

export interface AddInvolvedPersonRequest {
  personId: number;
  involvementType: string;
  injuryDescription?: string;
}

export interface UpdateInvolvedPersonRequest {
  involvementType: string;
  injuryDescription?: string;
}

export interface IncidentAttachmentDto {
  id: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  uploadedBy: string;
  uploadedAt: string;
  fileUrl: string;
  fileSizeFormatted: string;
}

export interface CorrectiveActionDto {
  id: number;
  description: string;
  assignedTo: UserDto;
  assignedToDepartment: string;
  dueDate: string;
  completedDate?: string;
  status: 'Pending' | 'InProgress' | 'Completed' | 'Overdue';
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  completionNotes?: string;
}

export interface CreateCorrectiveActionRequest {
  description: string;
  assignedToDepartment: string;
  assignedToId?: number;
  dueDate: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
}

export interface UpdateCorrectiveActionRequest {
  description: string;
  assignedToDepartment: string;
  assignedToId?: number;
  dueDate: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Pending' | 'InProgress' | 'Completed' | 'Overdue';
  completionNotes?: string;
}

export interface IncidentAuditLogDto {
  id: number;
  incidentId: number;
  action: string;
  fieldName: string;
  oldValue?: string;
  newValue?: string;
  changedBy: string;
  changedAt: string;
  changeDescription?: string;
}

// API slice
export const incidentApi = createApi({
  reducerPath: 'incidentApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/incident',
    prepareHeaders: (headers, { getState, endpoint }) => {
      // Get token from auth state
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      
      // Don't set content-type for FormData uploads (RTK Query will set it automatically)
      if (endpoint !== 'uploadIncidentAttachments') {
        headers.set('content-type', 'application/json');
      }
      
      return headers;
    },
  }),
  tagTypes: ['Incident', 'IncidentStatistics', 'IncidentAttachment', 'CorrectiveAction', 'IncidentAudit'],
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
    getIncidents: builder.query<GetIncidentsResponse, IncidentListParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.pageNumber) searchParams.append('pageNumber', params.pageNumber.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.status) searchParams.append('status', params.status);
        if (params.severity) searchParams.append('severity', params.severity);
        if (params.searchTerm) searchParams.append('searchTerm', params.searchTerm);

        return {
          url: `?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: (result) => [
        'Incident',
        ...(result?.incidents.map(({ id }) => ({ type: 'Incident' as const, id })) ?? []),
      ],
    }),

    // Get incident by ID
    getIncident: builder.query<IncidentDto, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'GET',
      }),
      providesTags: (_, __, id) => [{ type: 'Incident' as const, id }],
    }),

    // Get incident detail with full information
    getIncidentDetail: builder.query<any, number>({
      query: (id) => ({
        url: `/${id}/detail`,
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
        { type: 'IncidentAudit' as const, id },
        'Incident',
        'IncidentStatistics',
      ],
    }),

    // Get my incidents
    getMyIncidents: builder.query<GetIncidentsResponse, IncidentListParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        if (params.pageNumber) searchParams.append('pageNumber', params.pageNumber.toString());
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

    // Delete incident
    deleteIncident: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      // Optimistic update: remove from cache immediately
      onQueryStarted: async (id, { dispatch, queryFulfilled }) => {
        const patches: any[] = [];
        
        try {
          // Update all getIncidents queries optimistically
          const patchResults = dispatch(
            incidentApi.util.updateQueryData('getIncidents', {} as any, (draft) => {
              if (draft && draft.incidents) {
                draft.incidents = draft.incidents.filter(incident => incident.id !== id);
                draft.totalCount = Math.max(0, draft.totalCount - 1);
              }
            })
          );
          patches.push(patchResults);

          // Also handle queries with specific parameters
          [
            { pageNumber: 1, pageSize: 10 },
            { pageNumber: 1, pageSize: 5 },
            { pageNumber: 1, pageSize: 20 },
            { pageNumber: 2, pageSize: 10 },
          ].forEach(params => {
            const patchResult = dispatch(
              incidentApi.util.updateQueryData('getIncidents', params, (draft) => {
                if (draft && draft.incidents) {
                  draft.incidents = draft.incidents.filter(incident => incident.id !== id);
                  draft.totalCount = Math.max(0, draft.totalCount - 1);
                }
              })
            );
            patches.push(patchResult);
          });

          await queryFulfilled;
        } catch {
          // Revert optimistic updates on error
          patches.forEach(patch => patch.undo());
        }
      },
      invalidatesTags: (_, error, id) => {
        if (error) return [];
        return [
          'Incident',
          'IncidentStatistics',
          { type: 'Incident' as const, id },
        ];
      },
    }),

    // Upload incident attachments
    uploadIncidentAttachments: builder.mutation<
      { attachmentIds: number[] },
      { incidentId: number; files: File[] }
    >({
      query: ({ incidentId, files }) => {
        const formData = new FormData();
        files.forEach((file) => {
          formData.append('files', file);
        });

        return {
          url: `/${incidentId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
        'IncidentAttachment',
      ],
    }),

    // Get incident attachments
    getIncidentAttachments: builder.query<IncidentAttachmentDto[], number>({
      query: (incidentId) => ({
        url: `/${incidentId}/attachments`,
        method: 'GET',
      }),
      providesTags: (_, __, incidentId) => [
        { type: 'IncidentAttachment' as const, id: incidentId },
      ],
    }),

    // Delete incident attachment
    deleteIncidentAttachment: builder.mutation<void, { incidentId: number; attachmentId: number }>({
      query: ({ incidentId, attachmentId }) => ({
        url: `/${incidentId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'IncidentAttachment' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Get available users for involved persons
    getAvailableUsers: builder.query<UserDto[], string | undefined>({
      query: (searchTerm) => ({
        url: `/available-users${searchTerm ? `?searchTerm=${searchTerm}` : ''}`,
        method: 'GET',
      }),
    }),

    // Add involved person to incident
    addInvolvedPerson: builder.mutation<void, { incidentId: number; data: AddInvolvedPersonRequest }>({
      query: ({ incidentId, data }) => ({
        url: `/${incidentId}/involved-persons`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Update involved person
    updateInvolvedPerson: builder.mutation<void, { incidentId: number; personId: number; data: UpdateInvolvedPersonRequest }>({
      query: ({ incidentId, personId, data }) => ({
        url: `/${incidentId}/involved-persons/${personId}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Remove involved person from incident
    removeInvolvedPerson: builder.mutation<void, { incidentId: number; personId: number }>({
      query: ({ incidentId, personId }) => ({
        url: `/${incidentId}/involved-persons/${personId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Get corrective actions for incident
    getCorrectiveActions: builder.query<CorrectiveActionDto[], number>({
      query: (incidentId) => ({
        url: `/${incidentId}/corrective-actions`,
        method: 'GET',
      }),
      providesTags: (_, __, incidentId) => [
        { type: 'CorrectiveAction' as const, id: incidentId },
      ],
    }),

    // Create corrective action
    createCorrectiveAction: builder.mutation<{ id: number }, { incidentId: number; data: CreateCorrectiveActionRequest }>({
      query: ({ incidentId, data }) => ({
        url: `/${incidentId}/corrective-actions`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'CorrectiveAction' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Update corrective action
    updateCorrectiveAction: builder.mutation<void, { incidentId: number; actionId: number; data: UpdateCorrectiveActionRequest }>({
      query: ({ incidentId, actionId, data }) => ({
        url: `/${incidentId}/corrective-actions/${actionId}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'CorrectiveAction' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Delete corrective action
    deleteCorrectiveAction: builder.mutation<void, { incidentId: number; actionId: number }>({
      query: ({ incidentId, actionId }) => ({
        url: `/${incidentId}/corrective-actions/${actionId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, { incidentId }) => [
        { type: 'Incident' as const, id: incidentId },
        { type: 'CorrectiveAction' as const, id: incidentId },
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),

    // Get incident audit trail
    getIncidentAuditTrail: builder.query<IncidentAuditLogDto[], number>({
      query: (incidentId) => ({
        url: `/${incidentId}/audit-trail`,
        method: 'GET',
      }),
      providesTags: (_, __, incidentId) => [
        { type: 'IncidentAudit' as const, id: incidentId },
      ],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useCreateIncidentMutation,
  useGetIncidentsQuery,
  useGetIncidentQuery,
  useGetIncidentDetailQuery,
  useUpdateIncidentMutation,
  useDeleteIncidentMutation,
  useGetMyIncidentsQuery,
  useGetIncidentStatisticsQuery,
  useUploadIncidentAttachmentsMutation,
  useGetIncidentAttachmentsQuery,
  useDeleteIncidentAttachmentMutation,
  useGetAvailableUsersQuery,
  useAddInvolvedPersonMutation,
  useUpdateInvolvedPersonMutation,
  useRemoveInvolvedPersonMutation,
  useGetCorrectiveActionsQuery,
  useCreateCorrectiveActionMutation,
  useUpdateCorrectiveActionMutation,
  useDeleteCorrectiveActionMutation,
  useGetIncidentAuditTrailQuery,
} = incidentApi;