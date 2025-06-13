import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { InspectionDto, InspectionDetailDto, InspectionDashboardDto } from '../../types/inspection';
import { PagedList } from '../../types/common';

// Create Inspection Command Types
export interface CreateInspectionCommand {
  title: string;
  description: string;
  type: string;
  category: string;
  priority: string;
  scheduledDate: string;
  inspectorId: number;
  locationId: number;
  departmentId: number;
  facilityId: number;
  estimatedDurationMinutes: number;
  checklistItems: CreateInspectionItemCommand[];
}

export interface CreateInspectionItemCommand {
  question: string;
  description?: string;
  type: string;
  isRequired: boolean;
  expectedValue?: string;
  unit?: string;
  minValue?: number | null;
  maxValue?: number | null;
  options?: string;
  sortOrder: number;
}

// Update Inspection Command Types
export interface UpdateInspectionCommand {
  id: number;
  title: string;
  description: string;
  type: string;
  category: string;
  priority: string;
  scheduledDate: string;
  estimatedDurationMinutes: number;
  locationId: number;
  departmentId: number;
  facilityId: number;
  itemUpdates: InspectionItemUpdateCommand[];
}

export interface InspectionItemUpdateCommand {
  id: number;
  response?: string;
  notes?: string;
}

// Complete Inspection Command Types
export interface CompleteInspectionCommand {
  inspectionId: number;
  summary: string;
  recommendations?: string;
  findings: CreateInspectionFindingCommand[];
}

export interface CreateInspectionFindingCommand {
  description: string;
  type: string;
  severity: string;
  location?: string;
  equipment?: string;
  rootCause?: string;
  immediateAction?: string;
  correctiveAction?: string;
  dueDate?: string;
  responsiblePersonId?: number;
  regulation?: string;
}

// Query Parameters
export interface GetInspectionsQueryParams {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  type?: string;
  category?: string;
  priority?: string;
  inspectorId?: number;
  departmentId?: number;
  startDate?: string;
  endDate?: string;
  riskLevel?: string;
  isOverdue?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
}

export const inspectionApi = createApi({
  reducerPath: 'inspectionApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/inspections',
    prepareHeaders: (headers, { getState }) => {
      // Add authorization token if available
      const token = (getState() as any).auth?.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['Inspection', 'InspectionDashboard'],
  endpoints: (builder) => ({
    // Create Inspection
    createInspection: builder.mutation<InspectionDto, CreateInspectionCommand>({
      query: (inspection) => ({
        url: '',
        method: 'POST',
        body: inspection,
      }),
      invalidatesTags: ['Inspection', 'InspectionDashboard'],
    }),

    // Get Inspections with filtering and pagination
    getInspections: builder.query<PagedList<InspectionDto>, GetInspectionsQueryParams | void>({
      query: (params = {}) => ({
        url: '',
        params,
      }),
      providesTags: ['Inspection'],
    }),

    // Get Inspection by ID
    getInspectionById: builder.query<InspectionDetailDto, number>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'Inspection', id }],
    }),

    // Update Inspection
    updateInspection: builder.mutation<InspectionDetailDto, UpdateInspectionCommand>({
      query: ({ id, ...inspection }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: inspection,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'Inspection', id },
        'InspectionDashboard'
      ],
    }),

    // Start Inspection
    startInspection: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, id) => [
        { type: 'Inspection', id },
        'InspectionDashboard'
      ],
    }),

    // Complete Inspection
    completeInspection: builder.mutation<void, CompleteInspectionCommand>({
      query: ({ inspectionId, ...data }) => ({
        url: `/${inspectionId}/complete`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId },
        'InspectionDashboard'
      ],
    }),

    // Upload Attachment
    uploadAttachment: builder.mutation<any, { inspectionId: number; file: File; description?: string; category?: string }>({
      query: ({ inspectionId, file, description, category }) => {
        const formData = new FormData();
        formData.append('file', file);
        if (description) formData.append('description', description);
        if (category) formData.append('category', category);

        return {
          url: `/${inspectionId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId }
      ],
    }),

    // Delete Attachment
    deleteAttachment: builder.mutation<void, { inspectionId: number; attachmentId: number }>({
      query: ({ inspectionId, attachmentId }) => ({
        url: `/${inspectionId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId }
      ],
    }),

    // Download Attachment
    downloadAttachment: builder.query<Blob, { inspectionId: number; attachmentId: number }>({
      query: ({ inspectionId, attachmentId }) => ({
        url: `/${inspectionId}/attachments/${attachmentId}/download`,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Add Comment
    addComment: builder.mutation<any, { inspectionId: number; comment: string; isInternal?: boolean; parentCommentId?: number }>({
      query: ({ inspectionId, ...comment }) => ({
        url: `/${inspectionId}/comments`,
        method: 'POST',
        body: comment,
      }),
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId }
      ],
    }),

    // Add Finding
    addFinding: builder.mutation<any, { inspectionId: number } & CreateInspectionFindingCommand>({
      query: ({ inspectionId, ...finding }) => ({
        url: `/${inspectionId}/findings`,
        method: 'POST',
        body: finding,
      }),
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId }
      ],
    }),

    // Update Finding
    updateFinding: builder.mutation<any, { inspectionId: number; findingId: number } & Partial<CreateInspectionFindingCommand>>({
      query: ({ inspectionId, findingId, ...finding }) => ({
        url: `/${inspectionId}/findings/${findingId}`,
        method: 'PUT',
        body: finding,
      }),
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId }
      ],
    }),

    // Close Finding
    closeFinding: builder.mutation<void, { inspectionId: number; findingId: number; closureNotes: string }>({
      query: ({ inspectionId, findingId, closureNotes }) => ({
        url: `/${inspectionId}/findings/${findingId}/close`,
        method: 'POST',
        body: { closureNotes },
      }),
      invalidatesTags: (result, error, { inspectionId }) => [
        { type: 'Inspection', id: inspectionId }
      ],
    }),

    // Get Dashboard
    getDashboard: builder.query<InspectionDashboardDto, void>({
      query: () => '/dashboard',
      providesTags: ['InspectionDashboard'],
    }),

    // Get My Inspections
    getMyInspections: builder.query<PagedList<InspectionDto>, GetInspectionsQueryParams | void>({
      query: (params = {}) => ({
        url: '/my-inspections',
        params,
      }),
      providesTags: ['Inspection'],
    }),

    // Get Overdue Inspections
    getOverdueInspections: builder.query<PagedList<InspectionDto>, GetInspectionsQueryParams | void>({
      query: (params = {}) => ({
        url: '/overdue',
        params,
      }),
      providesTags: ['Inspection'],
    }),

    // Get Statistics
    getStatistics: builder.query<any, { startDate?: string; endDate?: string; departmentId?: number; inspectorId?: number; type?: string; category?: string }>({
      query: (params) => ({
        url: '/statistics',
        params,
      }),
      providesTags: ['InspectionDashboard'],
    }),
  }),
});

export const {
  useCreateInspectionMutation,
  useGetInspectionsQuery,
  useGetInspectionByIdQuery,
  useUpdateInspectionMutation,
  useStartInspectionMutation,
  useCompleteInspectionMutation,
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useLazyDownloadAttachmentQuery,
  useAddCommentMutation,
  useAddFindingMutation,
  useUpdateFindingMutation,
  useCloseFindingMutation,
  useGetDashboardQuery,
  useGetMyInspectionsQuery,
  useGetOverdueInspectionsQuery,
  useGetStatisticsQuery,
} = inspectionApi;