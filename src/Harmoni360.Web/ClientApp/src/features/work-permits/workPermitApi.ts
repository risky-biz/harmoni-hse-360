import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  WorkPermitDto,
  CreateWorkPermitRequest,
  UpdateWorkPermitRequest,
  GetWorkPermitsParams,
  GetWorkPermitsResponse,
  WorkPermitDashboardDto,
  WorkPermitStatisticsDto,
  WorkPermitAttachmentDto,
  WorkPermitHazardDto,
  WorkPermitPrecautionDto,
  WorkPermitApprovalDto
} from '../../types/workPermit';

export const workPermitApi = createApi({
  reducerPath: 'workPermitApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/work-permits',
    prepareHeaders: (headers, { getState, endpoint }) => {
      // Get token from auth state
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }

      // Don't set content-type for FormData uploads (RTK Query will set it automatically)
      if (endpoint?.includes('upload') || endpoint?.includes('attachment')) {
        // RTK Query will handle FormData content-type automatically
      } else {
        headers.set('content-type', 'application/json');
      }

      return headers;
    },
  }),
  tagTypes: [
    'WorkPermit',
    'WorkPermitDetail',
    'WorkPermitDashboard',
    'WorkPermitStatistics',
    'WorkPermitApproval',
    'WorkPermitHazard',
    'WorkPermitPrecaution',
    'WorkPermitAttachment',
    'MyWorkPermits',
    'PendingApprovals',
    'OverduePermits'
  ],
  endpoints: (builder) => ({
    // Basic CRUD Operations
    getWorkPermits: builder.query<GetWorkPermitsResponse, GetWorkPermitsParams>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.search) searchParams.append('search', params.search);
        if (params.status) searchParams.append('status', params.status);
        if (params.type) searchParams.append('type', params.type);
        if (params.priority) searchParams.append('priority', params.priority);
        if (params.riskLevel) searchParams.append('riskLevel', params.riskLevel);
        if (params.department) searchParams.append('department', params.department);
        if (params.location) searchParams.append('location', params.location);
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.sortBy) searchParams.append('sortBy', params.sortBy);
        if (params.sortDescending !== undefined) searchParams.append('sortDescending', params.sortDescending.toString());

        return `?${searchParams.toString()}`;
      },
      providesTags: ['WorkPermit'],
    }),

    getWorkPermitById: builder.query<WorkPermitDto, string>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'WorkPermitDetail', id }],
    }),

    createWorkPermit: builder.mutation<WorkPermitDto, any>({
      query: (command) => ({
        url: '',
        method: 'POST',
        body: command,
      }),
      invalidatesTags: ['WorkPermit', 'WorkPermitDashboard', 'MyWorkPermits'],
    }),

    updateWorkPermit: builder.mutation<WorkPermitDto, { id: string; workPermit: UpdateWorkPermitRequest }>({
      query: ({ id, workPermit }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: workPermit,
      }),
      invalidatesTags: (result, error, { id }) => [
        'WorkPermit',
        'WorkPermitDashboard',
        'MyWorkPermits',
        { type: 'WorkPermitDetail', id }
      ],
    }),

    deleteWorkPermit: builder.mutation<void, string>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['WorkPermit', 'WorkPermitDashboard', 'MyWorkPermits'],
    }),

    // Workflow Operations
    submitWorkPermit: builder.mutation<WorkPermitDto, string>({
      query: (id) => ({
        url: `/${id}/submit`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, id) => [
        'WorkPermit',
        'WorkPermitDashboard',
        'MyWorkPermits',
        'PendingApprovals',
        { type: 'WorkPermitDetail', id }
      ],
    }),

    approveWorkPermit: builder.mutation<WorkPermitDto, { id: string; comments?: string; k3CertificateNumber?: string; authorityLevel?: string }>({
      query: ({ id, ...body }) => ({
        url: `/${id}/approve`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (result, error, { id }) => [
        'WorkPermit',
        'WorkPermitDashboard',
        'MyWorkPermits',
        'PendingApprovals',
        { type: 'WorkPermitDetail', id }
      ],
    }),

    rejectWorkPermit: builder.mutation<WorkPermitDto, { id: string; rejectionReason: string }>({
      query: ({ id, rejectionReason }) => ({
        url: `/${id}/reject`,
        method: 'POST',
        body: { rejectionReason },
      }),
      invalidatesTags: (result, error, { id }) => [
        'WorkPermit',
        'WorkPermitDashboard',
        'MyWorkPermits',
        'PendingApprovals',
        { type: 'WorkPermitDetail', id }
      ],
    }),

    startWork: builder.mutation<WorkPermitDto, string>({
      query: (id) => ({
        url: `/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, id) => [
        'WorkPermit',
        'WorkPermitDashboard',
        'MyWorkPermits',
        { type: 'WorkPermitDetail', id }
      ],
    }),

    completeWork: builder.mutation<WorkPermitDto, string | { id: string; completionNotes?: string; isCompletedSafely?: boolean; lessonsLearned?: string }>({
      query: (arg) => {
        const id = typeof arg === 'string' ? arg : arg.id;
        const body = typeof arg === 'string' ? {} : { completionNotes: arg.completionNotes, isCompletedSafely: arg.isCompletedSafely, lessonsLearned: arg.lessonsLearned };
        
        return {
          url: `/${id}/complete`,
          method: 'POST',
          body,
        };
      },
      invalidatesTags: (result, error, arg) => {
        const id = typeof arg === 'string' ? arg : arg.id;
        return [
          'WorkPermit',
          'WorkPermitDashboard',
          'MyWorkPermits',
          'WorkPermitStatistics',
          { type: 'WorkPermitDetail', id }
        ];
      },
    }),

    cancelWorkPermit: builder.mutation<WorkPermitDto, string | { id: string; cancellationReason?: string }>({
      query: (arg) => {
        const id = typeof arg === 'string' ? arg : arg.id;
        const body = typeof arg === 'string' ? {} : { cancellationReason: arg.cancellationReason };
        
        return {
          url: `/${id}/cancel`,
          method: 'POST',
          body,
        };
      },
      invalidatesTags: (result, error, arg) => {
        const id = typeof arg === 'string' ? arg : arg.id;
        return [
          'WorkPermit',
          'WorkPermitDashboard',
          'MyWorkPermits',
          { type: 'WorkPermitDetail', id }
        ];
      },
    }),

    // Component Management
    addHazard: builder.mutation<WorkPermitHazardDto, { workPermitId: string; hazard: Partial<WorkPermitHazardDto> }>({
      query: ({ workPermitId, hazard }) => ({
        url: `/${workPermitId}/hazards`,
        method: 'POST',
        body: hazard,
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    updateHazard: builder.mutation<WorkPermitHazardDto, { workPermitId: string; hazardId: string; hazard: Partial<WorkPermitHazardDto> }>({
      query: ({ workPermitId, hazardId, hazard }) => ({
        url: `/${workPermitId}/hazards/${hazardId}`,
        method: 'PUT',
        body: hazard,
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    removeHazard: builder.mutation<void, { workPermitId: string; hazardId: string }>({
      query: ({ workPermitId, hazardId }) => ({
        url: `/${workPermitId}/hazards/${hazardId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    addPrecaution: builder.mutation<WorkPermitPrecautionDto, { workPermitId: string; precaution: Partial<WorkPermitPrecautionDto> }>({
      query: ({ workPermitId, precaution }) => ({
        url: `/${workPermitId}/precautions`,
        method: 'POST',
        body: precaution,
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    updatePrecaution: builder.mutation<WorkPermitPrecautionDto, { workPermitId: string; precautionId: string; precaution: Partial<WorkPermitPrecautionDto> }>({
      query: ({ workPermitId, precautionId, precaution }) => ({
        url: `/${workPermitId}/precautions/${precautionId}`,
        method: 'PUT',
        body: precaution,
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    completePrecaution: builder.mutation<WorkPermitPrecautionDto, { workPermitId: string; precautionId: string; completionNotes?: string }>({
      query: ({ workPermitId, precautionId, completionNotes }) => ({
        url: `/${workPermitId}/precautions/${precautionId}/complete`,
        method: 'POST',
        body: { completionNotes },
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    uploadAttachment: builder.mutation<WorkPermitAttachmentDto, { workPermitId: string; file: File; attachmentType: string; description?: string }>({
      query: ({ workPermitId, file, attachmentType, description }) => {
        const formData = new FormData();
        formData.append('workPermitId', workPermitId);
        formData.append('file', file);
        formData.append('attachmentType', attachmentType);
        if (description) formData.append('description', description);

        return {
          url: `/${workPermitId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    deleteAttachment: builder.mutation<void, { workPermitId: string; attachmentId: string }>({
      query: ({ workPermitId, attachmentId }) => ({
        url: `/${workPermitId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { workPermitId }) => [
        { type: 'WorkPermitDetail', id: workPermitId }
      ],
    }),

    // Analytics and Reports
    getDashboard: builder.query<WorkPermitDashboardDto, void>({
      query: () => '/dashboard',
      providesTags: ['WorkPermitDashboard'],
    }),

    getMyWorkPermits: builder.query<GetWorkPermitsResponse, { status?: string; type?: string; role?: string; searchTerm?: string; filterType?: string; pageNumber?: number; pageSize?: number }>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.pageNumber) searchParams.append('page', params.pageNumber.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.status) searchParams.append('status', params.status);
        if (params.type) searchParams.append('type', params.type);
        if (params.role) searchParams.append('role', params.role);
        if (params.searchTerm) searchParams.append('search', params.searchTerm);
        if (params.filterType) searchParams.append('filterType', params.filterType);

        return `/my-permits?${searchParams.toString()}`;
      },
      providesTags: ['MyWorkPermits'],
    }),

    getPendingApprovals: builder.query<GetWorkPermitsResponse, { type?: string; priority?: string; page?: number; pageSize?: number }>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.type) searchParams.append('type', params.type);
        if (params.priority) searchParams.append('priority', params.priority);

        return `/pending-approval?${searchParams.toString()}`;
      },
      providesTags: ['PendingApprovals'],
    }),

    getOverduePermits: builder.query<GetWorkPermitsResponse, { type?: string; page?: number; pageSize?: number }>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.type) searchParams.append('type', params.type);

        return `/overdue?${searchParams.toString()}`;
      },
      providesTags: ['OverduePermits'],
    }),

    getStatistics: builder.query<WorkPermitStatisticsDto, { startDate?: string; endDate?: string }>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);

        return `/statistics?${searchParams.toString()}`;
      },
      providesTags: ['WorkPermitStatistics'],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useGetWorkPermitsQuery,
  useGetWorkPermitByIdQuery,
  useCreateWorkPermitMutation,
  useUpdateWorkPermitMutation,
  useDeleteWorkPermitMutation,
  useSubmitWorkPermitMutation,
  useApproveWorkPermitMutation,
  useRejectWorkPermitMutation,
  useStartWorkMutation,
  useCompleteWorkMutation,
  useCancelWorkPermitMutation,
  useAddHazardMutation,
  useUpdateHazardMutation,
  useRemoveHazardMutation,
  useAddPrecautionMutation,
  useUpdatePrecautionMutation,
  useCompletePrecautionMutation,
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useGetDashboardQuery,
  useGetMyWorkPermitsQuery,
  useGetPendingApprovalsQuery,
  useGetOverduePermitsQuery,
  useGetStatisticsQuery,
} = workPermitApi;

// Expose the full API for named usage
export { workPermitApi as useWorkPermitApi };