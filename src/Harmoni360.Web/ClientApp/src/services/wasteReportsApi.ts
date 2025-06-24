import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { WasteReportDto, WasteCommentDto, WasteAuditLogDto } from '../types/wasteReports';
import { PagedList } from '../types/common';

export interface WasteReportFilters {
  classification?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  location?: string;
  reporterId?: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface UpdateWasteReportRequest {
  id: number;
  title: string;
  description: string;
  classification: number;
  location?: string;
  estimatedQuantity?: number;
  quantityUnit?: string;
  disposalMethod?: string;
  disposalDate?: string;
  disposedBy?: string;
  disposalCost?: number;
  contractorName?: string;
  manifestNumber?: string;
  treatment?: string;
  notes?: string;
}

export const wasteReportsApi = createApi({
  reducerPath: 'wasteReportsApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/wastereport',
    prepareHeaders: (headers, { getState }) => {
      // Add auth token if available
      const token = (getState() as any).auth?.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['WasteReport', 'WasteComment'],
  endpoints: (builder) => ({
    getWasteReports: builder.query<PagedList<WasteReportDto>, WasteReportFilters>({
      query: (filters) => ({
        url: '',
        params: filters,
      }),
      providesTags: (result) =>
        result
          ? [
              ...result.items.map(({ id }) => ({ type: 'WasteReport' as const, id })),
              { type: 'WasteReport', id: 'LIST' },
            ]
          : [{ type: 'WasteReport', id: 'LIST' }],
    }),

    getWasteReportById: builder.query<WasteReportDto, number>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'WasteReport', id }],
    }),

    getMyWasteReports: builder.query<PagedList<WasteReportDto>, { page?: number; pageSize?: number }>({
      query: ({ page = 1, pageSize = 20 }) => ({
        url: '/my-reports',
        params: { page, pageSize },
      }),
      providesTags: [{ type: 'WasteReport', id: 'MY_REPORTS' }],
    }),

    createWasteReport: builder.mutation<WasteReportDto, FormData>({
      query: (formData) => ({
        url: '',
        method: 'POST',
        body: formData,
      }),
      invalidatesTags: [
        { type: 'WasteReport', id: 'LIST' },
        { type: 'WasteReport', id: 'MY_REPORTS' },
      ],
    }),

    updateWasteReport: builder.mutation<WasteReportDto, UpdateWasteReportRequest>({
      query: ({ id, ...patch }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: patch,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'WasteReport', id },
        { type: 'WasteReport', id: 'LIST' },
        { type: 'WasteReport', id: 'MY_REPORTS' },
      ],
    }),

    deleteWasteReport: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, id) => [
        { type: 'WasteReport', id },
        { type: 'WasteReport', id: 'LIST' },
        { type: 'WasteReport', id: 'MY_REPORTS' },
      ],
    }),

    getWasteComments: builder.query<WasteCommentDto[], number>({
      query: (reportId) => `/${reportId}/comments`,
      providesTags: (result, error, reportId) => [
        { type: 'WasteComment', id: reportId },
      ],
    }),

    addWasteComment: builder.mutation<
      WasteCommentDto,
      { reportId: number; comment: string; category?: string }
    >({
      query: ({ reportId, ...body }) => ({
        url: `/${reportId}/comments`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (result, error, { reportId }) => [
        { type: 'WasteComment', id: reportId },
        { type: 'WasteReport', id: reportId },
      ],
    }),

    deleteWasteComment: builder.mutation<void, number>({
      query: (commentId) => ({
        url: `/comments/${commentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: [{ type: 'WasteComment', id: 'LIST' }],
    }),

    uploadWasteAttachment: builder.mutation<
      any,
      { reportId: number; file: File }
    >({
      query: ({ reportId, file }) => {
        const formData = new FormData();
        formData.append('file', file);
        return {
          url: `/${reportId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { reportId }) => [
        { type: 'WasteReport', id: reportId },
      ],
    }),

    downloadWasteAttachment: builder.mutation<Blob, number>({
      query: (attachmentId) => ({
        url: `/attachments/${attachmentId}`,
        method: 'GET',
        responseHandler: (response) => response.blob(),
      }),
    }),

    deleteWasteAttachment: builder.mutation<void, number>({
      query: (attachmentId) => ({
        url: `/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: [{ type: 'WasteReport', id: 'LIST' }],
    }),

    getWasteAuditTrail: builder.query<WasteAuditLogDto[], number>({
      query: (reportId) => `/${reportId}/audit-trail`,
      providesTags: (result, error, reportId) => [
        { type: 'WasteReport', id: reportId },
      ],
    }),

    getWasteComplianceAuditTrail: builder.query<WasteAuditLogDto[], { fromDate: string; toDate: string }>({
      query: ({ fromDate, toDate }) => `/compliance/audit-trail?fromDate=${fromDate}&toDate=${toDate}`,
      providesTags: ['WasteReport'],
    }),
  }),
});

export const {
  useGetWasteReportsQuery,
  useGetWasteReportByIdQuery,
  useGetMyWasteReportsQuery,
  useCreateWasteReportMutation,
  useUpdateWasteReportMutation,
  useDeleteWasteReportMutation,
  useGetWasteCommentsQuery,
  useAddWasteCommentMutation,
  useDeleteWasteCommentMutation,
  useUploadWasteAttachmentMutation,
  useDownloadWasteAttachmentMutation,
  useDeleteWasteAttachmentMutation,
  useGetWasteAuditTrailQuery,
  useGetWasteComplianceAuditTrailQuery,
} = wasteReportsApi;