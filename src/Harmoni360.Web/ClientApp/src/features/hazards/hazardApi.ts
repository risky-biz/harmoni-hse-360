import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  HazardDto,
  HazardDetailDto,
  CreateHazardRequest,
  UpdateHazardRequest,
  GetHazardsParams,
  GetHazardsResponse,
  CreateRiskAssessmentRequest,
  RiskAssessmentDto,
  CreateMitigationActionRequest,
  HazardMitigationActionDto,
  HazardDashboardDto,
  GetHazardDashboardParams,
  HazardAttachmentDto,
  UserDto
} from '../../types/hazard';

// Re-export for convenience
export type { 
  HazardAttachmentDto,
  HazardMitigationActionDto 
} from '../../types/hazard';

export const hazardApi = createApi({
  reducerPath: 'hazardApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/hazard',
    prepareHeaders: (headers, { getState, endpoint }) => {
      // Get token from auth state
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }

      // Don't set content-type for FormData uploads (RTK Query will set it automatically)
      if (endpoint?.includes('createHazard') || endpoint?.includes('updateHazard') || endpoint?.includes('upload')) {
        // RTK Query will handle FormData content-type automatically
      } else {
        headers.set('content-type', 'application/json');
      }

      return headers;
    },
  }),
  tagTypes: [
    'Hazard',
    'HazardDetail',
    'HazardStatistics',
    'HazardDashboard',
    'HazardAttachment',
    'RiskAssessment',
    'MitigationAction',
    'HazardLocation',
    'HazardAuditTrail'
  ],
  endpoints: (builder) => ({
    // Get hazards list with filtering and pagination
    getHazards: builder.query<GetHazardsResponse, GetHazardsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();

        // Add all parameters to search params
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: (result) => [
        'Hazard',
        'HazardStatistics',
        ...(result?.hazards.map(({ id }) => ({
          type: 'Hazard' as const,
          id,
        })) ?? []),
      ],
    }),

    // Get hazard by ID with detailed information
    getHazard: builder.query<HazardDetailDto, { 
      id: number; 
      includeAttachments?: boolean;
      includeRiskAssessments?: boolean;
      includeMitigationActions?: boolean;
      includeReassessments?: boolean;
    }>({
      query: ({ id, includeAttachments = true, includeRiskAssessments = true, includeMitigationActions = true, includeReassessments = true }) => {
        const searchParams = new URLSearchParams();
        searchParams.append('includeAttachments', includeAttachments.toString());
        searchParams.append('includeRiskAssessments', includeRiskAssessments.toString());
        searchParams.append('includeMitigationActions', includeMitigationActions.toString());
        searchParams.append('includeReassessments', includeReassessments.toString());

        return {
          url: `/${id}?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: (_, __, { id }) => [
        { type: 'Hazard' as const, id },
        { type: 'HazardDetail' as const, id }
      ],
    }),

    // Create new hazard
    createHazard: builder.mutation<HazardDto, CreateHazardRequest>({
      query: (hazard) => {
        const formData = new FormData();
        
        // Add all fields to FormData
        formData.append('title', hazard.title);
        formData.append('description', hazard.description);
        formData.append('category', hazard.category);
        formData.append('type', hazard.type);
        formData.append('location', hazard.location);
        formData.append('severity', hazard.severity);
        formData.append('reporterId', hazard.reporterId.toString());
        formData.append('reporterDepartment', hazard.reporterDepartment);
        
        if (hazard.latitude !== undefined) {
          formData.append('latitude', hazard.latitude.toString());
        }
        if (hazard.longitude !== undefined) {
          formData.append('longitude', hazard.longitude.toString());
        }
        if (hazard.expectedResolutionDate) {
          formData.append('expectedResolutionDate', hazard.expectedResolutionDate);
        }

        // Add file attachments
        if (hazard.attachments?.length) {
          hazard.attachments.forEach(file => {
            formData.append('attachments', file);
          });
        }

        return {
          url: '',
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: ['Hazard', 'HazardStatistics', 'HazardDashboard'],
    }),

    // Update existing hazard
    updateHazard: builder.mutation<HazardDto, UpdateHazardRequest>({
      query: ({ id, ...hazard }) => {
        const formData = new FormData();
        
        // Add all fields to FormData
        Object.entries(hazard).forEach(([key, value]) => {
          if (value !== undefined && value !== null && key !== 'newAttachments' && key !== 'attachmentsToRemove') {
            if (Array.isArray(value)) {
              value.forEach(item => formData.append(key, item.toString()));
            } else {
              formData.append(key, value.toString());
            }
          }
        });

        // Add new file attachments
        if (hazard.newAttachments?.length) {
          hazard.newAttachments.forEach(file => {
            formData.append('newAttachments', file);
          });
        }

        // Add attachments to remove
        if (hazard.attachmentsToRemove?.length) {
          hazard.attachmentsToRemove.forEach(attachmentId => {
            formData.append('attachmentsToRemove', attachmentId.toString());
          });
        }

        return {
          url: `/${id}`,
          method: 'PUT',
          body: formData,
        };
      },
      invalidatesTags: (_, __, { id }) => [
        { type: 'Hazard' as const, id },
        { type: 'HazardDetail' as const, id },
        'Hazard',
        'HazardStatistics',
        'HazardDashboard'
      ],
    }),

    // Create risk assessment
    createRiskAssessment: builder.mutation<RiskAssessmentDto, CreateRiskAssessmentRequest>({
      query: ({ hazardId, ...assessment }) => ({
        url: `/${hazardId}/risk-assessment`,
        method: 'POST',
        body: assessment,
      }),
      invalidatesTags: (_, __, { hazardId }) => [
        { type: 'Hazard' as const, id: hazardId },
        { type: 'HazardDetail' as const, id: hazardId },
        'Hazard',
        'HazardStatistics',
        'HazardDashboard'
      ],
    }),

    // Create mitigation action
    createMitigationAction: builder.mutation<HazardMitigationActionDto, CreateMitigationActionRequest>({
      query: ({ hazardId, ...action }) => ({
        url: `/${hazardId}/mitigation-action`,
        method: 'POST',
        body: action,
      }),
      invalidatesTags: (_, __, { hazardId }) => [
        { type: 'Hazard' as const, id: hazardId },
        { type: 'HazardDetail' as const, id: hazardId },
        'Hazard',
        'HazardStatistics',
        'HazardDashboard'
      ],
    }),

    // Get dashboard data
    getHazardDashboard: builder.query<HazardDashboardDto, GetHazardDashboardParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/dashboard${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['HazardDashboard', 'HazardStatistics'],
    }),

    // Get hazard attachments
    getHazardAttachments: builder.query<HazardAttachmentDto[], number>({
      query: (hazardId) => ({
        url: `/${hazardId}/attachments`,
        method: 'GET',
      }),
      providesTags: (_, __, hazardId) => [
        { type: 'HazardAttachment' as const, id: hazardId },
      ],
    }),

    // Get hazard locations for mapping
    getHazardLocations: builder.query<any[], { department?: string }>({
      query: ({ department } = {}) => {
        const searchParams = new URLSearchParams();
        if (department) {
          searchParams.append('department', department);
        }

        return {
          url: `/locations${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['HazardLocation'],
    }),

    // Get nearby hazards
    getNearbyHazards: builder.query<GetHazardsResponse, {
      latitude: number;
      longitude: number;
      radiusKm?: number;
    }>({
      query: ({ latitude, longitude, radiusKm = 1.0 }) => {
        const searchParams = new URLSearchParams();
        searchParams.append('latitude', latitude.toString());
        searchParams.append('longitude', longitude.toString());
        searchParams.append('radiusKm', radiusKm.toString());

        return {
          url: `/nearby?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['Hazard'],
    }),

    // Get my hazards
    getMyHazards: builder.query<GetHazardsResponse, GetHazardsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/my-hazards${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['Hazard'],
    }),

    // Get unassessed hazards
    getUnassessedHazards: builder.query<GetHazardsResponse, GetHazardsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/unassessed${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['Hazard'],
    }),

    // Get overdue hazards
    getOverdueHazards: builder.query<GetHazardsResponse, GetHazardsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/overdue${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['Hazard'],
    }),

    // Get high-risk hazards
    getHighRiskHazards: builder.query<GetHazardsResponse, GetHazardsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/high-risk${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['Hazard'],
    }),

    // Get available users for assignment
    getAvailableUsers: builder.query<UserDto[], string | undefined>({
      query: (searchTerm) => ({
        url: `/available-users${searchTerm ? `?searchTerm=${searchTerm}` : ''}`,
        method: 'GET',
      }),
    }),

    // Upload hazard attachments
    uploadHazardAttachments: builder.mutation<void, { hazardId: number; files: File[] }>({
      query: ({ hazardId, files }) => {
        const formData = new FormData();
        files.forEach(file => {
          formData.append('newAttachments', file);
        });

        return {
          url: `/${hazardId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (_, __, { hazardId }) => [
        { type: 'Hazard' as const, id: hazardId },
        { type: 'HazardDetail' as const, id: hazardId },
        { type: 'HazardAttachment' as const, id: hazardId },
      ],
    }),

    // Delete hazard attachment
    deleteHazardAttachment: builder.mutation<void, { hazardId: number; attachmentId: number }>({
      query: ({ hazardId, attachmentId }) => ({
        url: `/${hazardId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, { hazardId }) => [
        { type: 'Hazard' as const, id: hazardId },
        { type: 'HazardDetail' as const, id: hazardId },
        { type: 'HazardAttachment' as const, id: hazardId },
      ],
    }),

    // Get hazard audit trail
    getHazardAuditTrail: builder.query<HazardAuditLogDto[], number>({
      query: (hazardId) => ({
        url: `/${hazardId}/audit-trail`,
        method: 'GET',
      }),
      providesTags: (_, __, hazardId) => [
        { type: 'HazardAuditTrail' as const, id: hazardId },
      ],
    }),
  }),
});

// Export audit log type
export interface HazardAuditLogDto {
  id: number;
  hazardId: number;
  action: string;
  fieldName: string;
  oldValue: string;
  newValue: string;
  changeDescription: string;
  changedBy: string;
  changedAt: string;
}

// Export hooks for usage in functional components
export const {
  useGetHazardsQuery,
  useGetHazardQuery,
  useCreateHazardMutation,
  useUpdateHazardMutation,
  useCreateRiskAssessmentMutation,
  useCreateMitigationActionMutation,
  useGetHazardDashboardQuery,
  useGetHazardAttachmentsQuery,
  useGetHazardLocationsQuery,
  useGetNearbyHazardsQuery,
  useGetMyHazardsQuery,
  useGetUnassessedHazardsQuery,
  useGetOverdueHazardsQuery,
  useGetHighRiskHazardsQuery,
  useGetAvailableUsersQuery,
  useUploadHazardAttachmentsMutation,
  useDeleteHazardAttachmentMutation,
  useGetHazardAuditTrailQuery,
} = hazardApi;