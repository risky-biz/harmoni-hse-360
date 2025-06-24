import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

interface WasteReportDto {
  id: number;
  title: string;
  description: string;
  classification: number;
  classificationDisplay: string;
  status: number;
  statusDisplay: string;
  reportDate: string;
  reportedBy: string;
  location: string;
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
  createdAt: string;
  createdBy: string;
  updatedAt?: string;
  updatedBy?: string;
  comments: WasteCommentDto[];
  canEdit: boolean;
  canDispose: boolean;
  canApprove: boolean;
  canReject: boolean;
  canArchive: boolean;
  isOverdue: boolean;
  isHighRisk: boolean;
  hasComments: boolean;
  commentsCount: number;
  daysUntilDisposal: number;
  daysOverdue: number;
}

interface PagedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface WasteReportSummaryDto {
  id: number;
  title: string;
  type: string;
  status: string;
  reportDate: string;
  reportedBy: string;
  location?: string;
  estimatedQuantity?: number;
  quantityUnit?: string;
  disposalDate?: string;
  disposalCost?: number;
  commentsCount: number;
  isOverdue: boolean;
  canEdit: boolean;
  canDispose: boolean;
  createdAt: string;
}

interface WasteCommentDto {
  id: number;
  wasteReportId: number;
  comment: string;
  commentedBy: string;
  commentedAt: string;
  category?: string;
  isInternal: boolean;
}

interface WasteDashboardDto {
  totalReports: number;
  pendingReports: number;
  completedReports: number;
  totalDisposalProviders: number;
  activeDisposalProviders: number;
  categoryStats: WasteCategoryStatsDto[];
  monthlyStats: MonthlyWasteStatsDto[];
  recentReports: WasteReportSummaryDto[];
  expiringProviders: DisposalProviderDto[];
}

interface WasteCategoryStatsDto {
  category: string;
  count: number;
  percentage: number;
}

interface MonthlyWasteStatsDto {
  month: string;
  reportCount: number;
}

interface DisposalProviderDto {
  id: number;
  name: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  status: string;
  isActive: boolean;
}

interface WasteCommentDto {
  id: number;
  wasteReportId: number;
  comment: string;
  type: string;
  commentedById: number;
  commentedByName: string;
  createdAt: string;
  createdBy: string;
}

interface WasteAttachmentDto {
  id: number;
  wasteReportId: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  uploadedBy: string;
  uploadedAt: string;
}

interface CreateWasteReportRequest {
  title: string;
  description: string;
  category: string;
  generatedDate: string;
  location: string;
  reporterId?: number;
}

interface UpdateWasteReportRequest {
  id: number;
  title: string;
  description: string;
  category: string;
  generatedDate: string;
  location: string;
}

interface AddWasteCommentRequest {
  comment: string;
  type: string;
}

export const wasteManagementApi = createApi({
  reducerPath: 'wasteManagementApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/WasteReport',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth?.token;
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['WasteReport', 'WasteDashboard', 'WasteComment', 'WasteAttachment'],
  endpoints: (builder) => ({
    // Dashboard
    getWasteDashboard: builder.query<WasteDashboardDto, void>({
      query: () => 'dashboard',
      providesTags: ['WasteDashboard'],
    }),

    // Waste Reports
    getWasteReports: builder.query<PagedList<WasteReportDto>, {
      category?: string;
      status?: string;
      classification?: string;
      fromDate?: string;
      toDate?: string;
      location?: string;
      reporterId?: number;
      search?: string;
      sortBy?: string;
      sortDescending?: boolean;
      page?: number;
      pageSize?: number;
    }>({
      query: (params) => ({
        url: '',
        params: {
          ...params,
          page: params.page || 1,
          pageSize: params.pageSize || 20,
        },
      }),
      providesTags: ['WasteReport'],
    }),

    getMyWasteReports: builder.query<PagedList<WasteReportDto>, {
      page?: number;
      pageSize?: number;
    }>({
      query: (params) => ({
        url: 'my-reports',
        params: {
          page: params.page || 1,
          pageSize: params.pageSize || 20,
        },
      }),
      providesTags: ['WasteReport'],
    }),

    getWasteReportById: builder.query<WasteReportDto, number>({
      query: (id) => `${id}`,
      providesTags: (result, error, id) => [{ type: 'WasteReport', id }],
    }),

    createWasteReport: builder.mutation<WasteReportDto, FormData>({
      query: (formData) => ({
        url: '',
        method: 'POST',
        body: formData,
      }),
      invalidatesTags: ['WasteReport', 'WasteDashboard'],
    }),

    updateWasteReport: builder.mutation<WasteReportDto, { id: number; data: UpdateWasteReportRequest }>({
      query: ({ id, data }) => ({
        url: `${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'WasteReport', id },
        'WasteReport',
        'WasteDashboard',
      ],
    }),

    deleteWasteReport: builder.mutation<void, number>({
      query: (id) => ({
        url: `${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['WasteReport', 'WasteDashboard'],
    }),

    updateWasteReportStatus: builder.mutation<void, { id: number; status: string }>({
      query: ({ id, status }) => ({
        url: `${id}/status`,
        method: 'PUT',
        body: status,
        headers: {
          'Content-Type': 'application/json',
        },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'WasteReport', id },
        'WasteReport',
        'WasteDashboard',
      ],
    }),

    // Comments
    getWasteComments: builder.query<WasteCommentDto[], number>({
      query: (wasteReportId) => `${wasteReportId}/comments`,
      providesTags: (result, error, wasteReportId) => [
        { type: 'WasteComment', id: wasteReportId },
      ],
    }),

    addWasteComment: builder.mutation<WasteCommentDto, { id: number; data: AddWasteCommentRequest }>({
      query: ({ id, data }) => ({
        url: `${id}/comments`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'WasteComment', id },
        { type: 'WasteReport', id },
      ],
    }),

    deleteWasteComment: builder.mutation<void, number>({
      query: (commentId) => ({
        url: `comments/${commentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['WasteComment'],
    }),

    // Attachments
    uploadWasteAttachment: builder.mutation<WasteAttachmentDto, { id: number; file: File }>({
      query: ({ id, file }) => {
        const formData = new FormData();
        formData.append('file', file);
        return {
          url: `${id}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { id }) => [
        { type: 'WasteReport', id },
        { type: 'WasteAttachment', id },
      ],
    }),

    downloadWasteAttachment: builder.query<Blob, number>({
      query: (attachmentId) => ({
        url: `attachments/${attachmentId}`,
        responseHandler: (response) => response.blob(),
      }),
    }),

    deleteWasteAttachment: builder.mutation<void, number>({
      query: (attachmentId) => ({
        url: `attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['WasteAttachment', 'WasteReport'],
    }),
  }),
});

export const {
  useGetWasteDashboardQuery,
  useGetWasteReportsQuery,
  useGetMyWasteReportsQuery,
  useGetWasteReportByIdQuery,
  useCreateWasteReportMutation,
  useUpdateWasteReportMutation,
  useDeleteWasteReportMutation,
  useUpdateWasteReportStatusMutation,
  useGetWasteCommentsQuery,
  useAddWasteCommentMutation,
  useDeleteWasteCommentMutation,
  useUploadWasteAttachmentMutation,
  useLazyDownloadWasteAttachmentQuery,
  useDeleteWasteAttachmentMutation,
} = wasteManagementApi;

export type {
  WasteReportDto,
  WasteDashboardDto,
  WasteCommentDto,
  WasteAttachmentDto,
  CreateWasteReportRequest,
  UpdateWasteReportRequest,
  AddWasteCommentRequest,
  PagedList,
};