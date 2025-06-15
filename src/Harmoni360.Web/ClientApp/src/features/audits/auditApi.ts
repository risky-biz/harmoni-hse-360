import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  AuditDto,
  AuditSummaryDto,
  CreateAuditRequest,
  UpdateAuditRequest,
  GetAuditsParams,
  GetAuditsResponse,
  AuditDashboardDto,
  AuditStatisticsDto,
  AuditAttachmentDto,
  AuditFindingDto,
  AuditItemDto,
  AuditCommentDto
} from '../../types/audit';

export const auditApi = createApi({
  reducerPath: 'auditApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/audits',
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
    'Audit',
    'AuditDetail',
    'AuditDashboard',
    'AuditStatistics',
    'AuditFinding',
    'AuditItem',
    'AuditAttachment',
    'AuditComment',
    'MyAudits',
    'PendingAudits',
    'OverdueAudits'
  ],
  endpoints: (builder) => ({
    // Get audits with filtering and pagination
    getAudits: builder.query<GetAuditsResponse, GetAuditsParams>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.search) searchParams.append('search', params.search);
        if (params.status) searchParams.append('status', params.status);
        if (params.type) searchParams.append('type', params.type);
        if (params.category) searchParams.append('category', params.category);
        if (params.priority) searchParams.append('priority', params.priority);
        if (params.riskLevel) searchParams.append('riskLevel', params.riskLevel);
        if (params.auditorId) searchParams.append('auditorId', params.auditorId.toString());
        if (params.departmentId) searchParams.append('departmentId', params.departmentId.toString());
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.sortBy) searchParams.append('sortBy', params.sortBy);
        if (params.sortDescending !== undefined) searchParams.append('sortDescending', params.sortDescending.toString());

        return `?${searchParams.toString()}`;
      },
      providesTags: ['Audit'],
    }),

    // Get audit by ID
    getAuditById: builder.query<AuditDto, number>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'AuditDetail', id }],
    }),

    // Create new audit
    createAudit: builder.mutation<AuditDto, CreateAuditRequest>({
      query: (audit) => ({
        url: '',
        method: 'POST',
        body: audit,
      }),
      invalidatesTags: ['Audit', 'AuditDashboard'],
    }),

    // Update audit
    updateAudit: builder.mutation<AuditDto, { id: number; audit: UpdateAuditRequest }>({
      query: ({ id, audit }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: audit,
      }),
      invalidatesTags: (result, error, { id }) => [
        'Audit',
        { type: 'AuditDetail', id },
        'AuditDashboard',
      ],
    }),

    // Delete audit
    deleteAudit: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Audit', 'AuditDashboard'],
    }),

    // Start audit
    startAudit: builder.mutation<AuditDto, number>({
      query: (id) => ({
        url: `/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, id) => [
        'Audit',
        { type: 'AuditDetail', id },
        'AuditDashboard',
      ],
    }),

    // Complete audit
    completeAudit: builder.mutation<AuditDto, { id: number; summary?: string; recommendations?: string }>({
      query: ({ id, summary, recommendations }) => ({
        url: `/${id}/complete`,
        method: 'POST',
        body: { id, summary, recommendations },
      }),
      invalidatesTags: (result, error, { id }) => [
        'Audit',
        { type: 'AuditDetail', id },
        'AuditDashboard',
      ],
    }),

    // Cancel audit
    cancelAudit: builder.mutation<AuditDto, { id: number; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/${id}/cancel`,
        method: 'POST',
        body: { id, reason },
      }),
      invalidatesTags: (result, error, { id }) => [
        'Audit',
        { type: 'AuditDetail', id },
        'AuditDashboard',
      ],
    }),

    // Archive audit
    archiveAudit: builder.mutation<AuditDto, { id: number; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/${id}/archive`,
        method: 'POST',
        body: { id, reason },
      }),
      invalidatesTags: (result, error, { id }) => [
        'Audit',
        { type: 'AuditDetail', id },
        'AuditDashboard',
      ],
    }),

    // Upload attachment
    uploadAttachment: builder.mutation<AuditAttachmentDto, { auditId: number; file: File; description?: string; category?: string }>({
      query: ({ auditId, file, description, category }) => {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('auditId', auditId.toString());
        if (description) formData.append('description', description);
        if (category) formData.append('category', category);

        return {
          url: `/${auditId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { auditId }) => [
        { type: 'AuditDetail', id: auditId },
      ],
    }),

    // Delete attachment
    deleteAttachment: builder.mutation<void, { auditId: number; attachmentId: number }>({
      query: ({ auditId, attachmentId }) => ({
        url: `/${auditId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { auditId }) => [
        { type: 'AuditDetail', id: auditId },
      ],
    }),

    // Get audit dashboard
    getAuditDashboard: builder.query<AuditDashboardDto, { startDate?: string; endDate?: string; departmentId?: number }>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.departmentId) searchParams.append('departmentId', params.departmentId.toString());

        return `dashboard?${searchParams.toString()}`;
      },
      providesTags: ['AuditDashboard'],
    }),

    // Get my audits
    getMyAudits: builder.query<GetAuditsResponse, GetAuditsParams>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.search) searchParams.append('search', params.search);
        if (params.status) searchParams.append('status', params.status);
        if (params.type) searchParams.append('type', params.type);
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.sortBy) searchParams.append('sortBy', params.sortBy);
        if (params.sortDescending !== undefined) searchParams.append('sortDescending', params.sortDescending.toString());

        return `my-audits?${searchParams.toString()}`;
      },
      providesTags: ['MyAudits'],
    }),

    // Get pending audits
    getPendingAudits: builder.query<GetAuditsResponse, GetAuditsParams>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.search) searchParams.append('search', params.search);

        return `pending?${searchParams.toString()}`;
      },
      providesTags: ['PendingAudits'],
    }),

    // Get overdue audits
    getOverdueAudits: builder.query<GetAuditsResponse, GetAuditsParams>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.page) searchParams.append('page', params.page.toString());
        if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
        if (params.search) searchParams.append('search', params.search);

        return `overdue?${searchParams.toString()}`;
      },
      providesTags: ['OverdueAudits'],
    }),

    // Get audit statistics
    getAuditStatistics: builder.query<AuditStatisticsDto, { startDate?: string; endDate?: string; departmentId?: number }>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        if (params.departmentId) searchParams.append('departmentId', params.departmentId.toString());

        return `statistics?${searchParams.toString()}`;
      },
      providesTags: ['AuditStatistics'],
    }),

    // Export audit report
    exportAuditReport: builder.query<Blob, { id: number; format?: string }>({
      query: ({ id, format = 'pdf' }) => ({
        url: `/${id}/export?format=${format}`,
        responseHandler: (response) => response.blob(),
      }),
    }),
  }),
});

export const {
  useGetAuditsQuery,
  useGetAuditByIdQuery,
  useCreateAuditMutation,
  useUpdateAuditMutation,
  useDeleteAuditMutation,
  useStartAuditMutation,
  useCompleteAuditMutation,
  useCancelAuditMutation,
  useArchiveAuditMutation,
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useGetAuditDashboardQuery,
  useGetMyAuditsQuery,
  useGetPendingAuditsQuery,
  useGetOverdueAuditsQuery,
  useGetAuditStatisticsQuery,
  useLazyExportAuditReportQuery,
} = auditApi;