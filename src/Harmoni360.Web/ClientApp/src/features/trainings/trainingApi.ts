import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type {
  TrainingDto,
  TrainingFormData,
  TrainingParticipantDto,
  TrainingAttachmentDto,
  TrainingCommentDto,
  TrainingCertificationDto
} from '../../types/training';

export interface CreateTrainingRequest extends TrainingFormData {}

export interface UpdateTrainingRequest extends Partial<TrainingFormData> {
  id: number;
}

export interface GetTrainingsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  type?: string;
  category?: string;
  status?: string;
  priority?: string;
  deliveryMethod?: string;
  dateFrom?: string;
  dateTo?: string;
  instructorName?: string;
  isK3Training?: boolean;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface GetTrainingsResponse {
  items: TrainingDto[];
  totalCount: number;
  pageCount: number;
  currentPage: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface TrainingDashboardDto {
  totalTrainings: number;
  scheduledTrainings: number;
  inProgressTrainings: number;
  completedTrainings: number;
  cancelledTrainings: number;
  totalParticipants: number;
  averageCompletionRate: number;
  averagePassRate: number;
  upcomingTrainings: TrainingDto[];
  recentCompletions: TrainingDto[];
  complianceStatus: {
    mandatoryTrainingsDue: number;
    certificationsExpiring: number;
    k3ComplianceRate: number;
  };
  monthlyStatistics: {
    month: string;
    trainingsCompleted: number;
    participantCount: number;
    averageScore: number;
  }[];
}

export interface TrainingStatisticsDto {
  trainingsByType: { type: string; count: number; percentage: number }[];
  trainingsByCategory: { category: string; count: number; percentage: number }[];
  completionRatesByDepartment: { department: string; completionRate: number }[];
  monthlyTrends: { month: string; completed: number; participants: number }[];
  topPerformingTrainings: { training: string; score: number; participants: number }[];
  certificationStatus: { status: string; count: number }[];
}

export interface EnrollParticipantRequest {
  trainingId: number;
  userId: number;
  userName: string;
  department?: string;
  position?: string;
}

export interface MarkAttendanceRequest {
  trainingId: number;
  participantId: number;
  isPresent: boolean;
  notes?: string;
}

export interface RecordAssessmentRequest {
  trainingId: number;
  participantId: number;
  score: number;
  passed: boolean;
  feedback?: string;
}

export interface IssueCertificationRequest {
  trainingId: number;
  participantId: number;
  certificationType: string;
  validityPeriod?: string;
  certifyingBody?: string;
}

export interface UploadAttachmentRequest {
  trainingId: number;
  file: File;
  attachmentType: string;
  description?: string;
  isTrainingMaterial?: boolean;
}

export const trainingApi = createApi({
  reducerPath: 'trainingApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/trainings',
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
    'Training',
    'TrainingDetail',
    'TrainingDashboard',
    'TrainingStatistics',
    'TrainingParticipant',
    'TrainingAttachment',
    'TrainingComment',
    'TrainingCertification',
    'MyTrainings',
    'UpcomingTrainings'
  ],
  endpoints: (builder) => ({
    // CRUD Operations
    createTraining: builder.mutation<TrainingDto, CreateTrainingRequest>({
      query: (training) => ({
        url: '',
        method: 'POST',
        body: training,
      }),
      invalidatesTags: ['Training', 'TrainingDashboard', 'MyTrainings'],
    }),

    getTrainings: builder.query<GetTrainingsResponse, GetTrainingsParams>({
      query: (params) => ({
        url: '',
        params,
      }),
      providesTags: ['Training'],
    }),

    getTrainingById: builder.query<TrainingDto, number>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'TrainingDetail', id }],
    }),

    updateTraining: builder.mutation<TrainingDto, UpdateTrainingRequest>({
      query: ({ id, ...training }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: training,
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'TrainingDetail', id },
        'Training',
        'TrainingDashboard',
        'MyTrainings',
      ],
    }),

    deleteTraining: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Training', 'TrainingDashboard', 'MyTrainings'],
    }),

    // State Transitions
    startTraining: builder.mutation<TrainingDto, number>({
      query: (id) => ({
        url: `/${id}/start`,
        method: 'POST',
      }),
      invalidatesTags: (result, error, id) => [
        { type: 'TrainingDetail', id },
        'Training',
        'TrainingDashboard',
      ],
    }),

    completeTraining: builder.mutation<TrainingDto, { id: number; feedback?: string }>({
      query: ({ id, feedback }) => ({
        url: `/${id}/complete`,
        method: 'POST',
        body: { feedback },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'TrainingDetail', id },
        'Training',
        'TrainingDashboard',
      ],
    }),

    cancelTraining: builder.mutation<TrainingDto, { id: number; reason: string }>({
      query: ({ id, reason }) => ({
        url: `/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'TrainingDetail', id },
        'Training',
        'TrainingDashboard',
      ],
    }),

    // Participant Management
    enrollParticipant: builder.mutation<TrainingParticipantDto, EnrollParticipantRequest>({
      query: ({ trainingId, ...participant }) => ({
        url: `/${trainingId}/participants`,
        method: 'POST',
        body: participant,
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingParticipant',
      ],
    }),

    withdrawParticipant: builder.mutation<void, { trainingId: number; participantId: number }>({
      query: ({ trainingId, participantId }) => ({
        url: `/${trainingId}/participants/${participantId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingParticipant',
      ],
    }),

    markAttendance: builder.mutation<TrainingParticipantDto, MarkAttendanceRequest>({
      query: ({ trainingId, participantId, ...attendance }) => ({
        url: `/${trainingId}/participants/${participantId}/attendance`,
        method: 'POST',
        body: attendance,
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingParticipant',
      ],
    }),

    recordAssessment: builder.mutation<TrainingParticipantDto, RecordAssessmentRequest>({
      query: ({ trainingId, participantId, ...assessment }) => ({
        url: `/${trainingId}/participants/${participantId}/assessment`,
        method: 'POST',
        body: assessment,
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingParticipant',
      ],
    }),

    issueCertification: builder.mutation<TrainingCertificationDto, IssueCertificationRequest>({
      query: ({ trainingId, participantId, ...certification }) => ({
        url: `/${trainingId}/participants/${participantId}/certification`,
        method: 'POST',
        body: certification,
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingCertification',
      ],
    }),

    // Attachments
    uploadAttachment: builder.mutation<TrainingAttachmentDto, UploadAttachmentRequest>({
      query: ({ trainingId, file, attachmentType, description, isTrainingMaterial }) => {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('attachmentType', attachmentType);
        if (description) formData.append('description', description);
        if (isTrainingMaterial !== undefined) formData.append('isTrainingMaterial', String(isTrainingMaterial));

        return {
          url: `/${trainingId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingAttachment',
      ],
    }),

    deleteAttachment: builder.mutation<void, { trainingId: number; attachmentId: number }>({
      query: ({ trainingId, attachmentId }) => ({
        url: `/${trainingId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingAttachment',
      ],
    }),

    downloadAttachment: builder.query<Blob, { trainingId: number; attachmentId: number }>({
      query: ({ trainingId, attachmentId }) => ({
        url: `/${trainingId}/attachments/${attachmentId}/download`,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Comments
    addComment: builder.mutation<TrainingCommentDto, { trainingId: number; content: string; commentType: string }>({
      query: ({ trainingId, ...comment }) => ({
        url: `/${trainingId}/comments`,
        method: 'POST',
        body: comment,
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
        'TrainingComment',
      ],
    }),

    // Requirements
    addRequirement: builder.mutation<any, { trainingId: number; description: string; isMandatory: boolean; dueDate?: string }>({
      query: ({ trainingId, ...requirement }) => ({
        url: `/${trainingId}/requirements`,
        method: 'POST',
        body: requirement,
      }),
      invalidatesTags: (result, error, { trainingId }) => [
        { type: 'TrainingDetail', id: trainingId },
      ],
    }),

    // Dashboard and Statistics
    getTrainingDashboard: builder.query<TrainingDashboardDto, void>({
      query: () => '/dashboard',
      providesTags: ['TrainingDashboard'],
    }),

    getTrainingStatistics: builder.query<TrainingStatisticsDto, { dateFrom?: string; dateTo?: string }>({
      query: (params) => ({
        url: '/statistics',
        params,
      }),
      providesTags: ['TrainingStatistics'],
    }),

    getMyTrainings: builder.query<GetTrainingsResponse, GetTrainingsParams>({
      query: (params) => ({
        url: '/my-trainings',
        params,
      }),
      providesTags: ['MyTrainings'],
    }),

    getUpcomingTrainings: builder.query<TrainingDto[], { limit?: number }>({
      query: (params) => ({
        url: '/upcoming',
        params,
      }),
      providesTags: ['UpcomingTrainings'],
    }),

    getCertifications: builder.query<TrainingCertificationDto[], { userId?: number; status?: string }>({
      query: (params) => ({
        url: '/certifications',
        params,
      }),
      providesTags: ['TrainingCertification'],
    }),

    downloadCertificate: builder.query<Blob, number>({
      query: (certificationId) => ({
        url: `/certifications/${certificationId}/download`,
        responseHandler: (response) => response.blob(),
      }),
    }),
  }),
});

export const {
  // CRUD Operations
  useCreateTrainingMutation,
  useGetTrainingsQuery,
  useGetTrainingByIdQuery,
  useUpdateTrainingMutation,
  useDeleteTrainingMutation,

  // State Transitions
  useStartTrainingMutation,
  useCompleteTrainingMutation,
  useCancelTrainingMutation,

  // Participant Management
  useEnrollParticipantMutation,
  useWithdrawParticipantMutation,
  useMarkAttendanceMutation,
  useRecordAssessmentMutation,
  useIssueCertificationMutation,

  // Attachments
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useLazyDownloadAttachmentQuery,

  // Comments and Requirements
  useAddCommentMutation,
  useAddRequirementMutation,

  // Dashboard and Statistics
  useGetTrainingDashboardQuery,
  useGetTrainingStatisticsQuery,
  useGetMyTrainingsQuery,
  useGetUpcomingTrainingsQuery,
  useGetCertificationsQuery,
  useLazyDownloadCertificateQuery,
} = trainingApi;