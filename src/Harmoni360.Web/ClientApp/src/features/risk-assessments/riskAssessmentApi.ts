import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface RiskAssessmentDto {
  id: number;
  hazardId: number;
  hazardTitle: string;
  type: string;
  assessorName: string;
  assessmentDate: string;
  probabilityScore: number;
  severityScore: number;
  riskScore: number;
  riskLevel: string;
  potentialConsequences: string;
  existingControls: string;
  recommendedActions: string;
  additionalNotes: string;
  nextReviewDate: string;
  isActive: boolean;
  isApproved: boolean;
  approvedByName?: string;
  approvedAt?: string;
  approvalNotes?: string;
  createdAt: string;
  createdBy?: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface RiskAssessmentDetailDto extends RiskAssessmentDto {
  hazard: HazardSummaryDto;
  assessor: UserSummaryDto;
  approvedBy?: UserSummaryDto;
}

export interface HazardSummaryDto {
  id: number;
  title: string;
  description: string;
  category: string;
  type: string;
  location: string;
  status: string;
  severity: string;
  identifiedDate: string;
  reporterName: string;
}

export interface UserSummaryDto {
  id: number;
  name: string;
  email: string;
  department?: string;
  position?: string;
}

export interface GetRiskAssessmentsParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  status?: string;
  riskLevel?: string;
  assessmentType?: string;
  hazardId?: number;
  isApproved?: boolean;
  isActive?: boolean;
}

export interface GetRiskAssessmentsResponse {
  riskAssessments: RiskAssessmentDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface RiskAssessmentStatistics {
  totalAssessments: number;
  approvedAssessments: number;
  pendingApproval: number;
  dueForReview: number;
  riskLevelDistribution: Record<string, number>;
  assessmentTypeDistribution: Record<string, number>;
  averageRiskScore: number;
  highRiskCount: number;
  completionRate: number;
}

export interface CreateRiskAssessmentRequest {
  hazardId: number;
  type: string;
  assessorName: string;
  assessmentDate: string;
  probabilityScore: number;
  severityScore: number;
  potentialConsequences: string;
  existingControls: string;
  recommendedActions?: string;
  additionalNotes?: string;
  nextReviewDate: string;
}

export interface UpdateRiskAssessmentRequest extends CreateRiskAssessmentRequest {
  id: number;
}

export const riskAssessmentApi = createApi({
  reducerPath: 'riskAssessmentApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/riskassessment',
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
  tagTypes: [
    'RiskAssessment',
    'RiskAssessmentDetail',
    'RiskAssessmentStatistics',
  ],
  endpoints: (builder) => ({
    // Get risk assessments list with filtering and pagination
    getRiskAssessments: builder.query<GetRiskAssessmentsResponse, GetRiskAssessmentsParams>({
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
        'RiskAssessment',
        'RiskAssessmentStatistics',
        ...(result?.riskAssessments.map(({ id }) => ({
          type: 'RiskAssessment' as const,
          id,
        })) ?? []),
      ],
    }),

    // Get risk assessment by ID with detailed information
    getRiskAssessment: builder.query<RiskAssessmentDetailDto, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'GET',
      }),
      providesTags: (_, __, id) => [
        { type: 'RiskAssessment' as const, id },
        { type: 'RiskAssessmentDetail' as const, id },
      ],
    }),

    // Get risk assessments due for review
    getRiskAssessmentsDueForReview: builder.query<GetRiskAssessmentsResponse, GetRiskAssessmentsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/due-for-review${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['RiskAssessment'],
    }),

    // Get risk assessments by risk level
    getRiskAssessmentsByRiskLevel: builder.query<GetRiskAssessmentsResponse, {
      riskLevel: string;
      params?: GetRiskAssessmentsParams;
    }>({
      query: ({ riskLevel, params = {} }) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/by-risk-level/${riskLevel}${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['RiskAssessment'],
    }),

    // Get pending approval risk assessments
    getPendingApprovalRiskAssessments: builder.query<GetRiskAssessmentsResponse, GetRiskAssessmentsParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        
        Object.entries(params).forEach(([key, value]) => {
          if (value !== undefined && value !== null && value !== '') {
            searchParams.append(key, value.toString());
          }
        });

        return {
          url: `/pending-approval${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['RiskAssessment'],
    }),

    // Get risk assessment statistics
    getRiskAssessmentStatistics: builder.query<RiskAssessmentStatistics, void>({
      query: () => ({
        url: '/statistics',
        method: 'GET',
      }),
      providesTags: ['RiskAssessmentStatistics'],
    }),

    // Create new risk assessment
    createRiskAssessment: builder.mutation<RiskAssessmentDto, CreateRiskAssessmentRequest>({
      query: (data) => ({
        url: '',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: [
        'RiskAssessment',
        'RiskAssessmentStatistics',
      ],
    }),

    // Update existing risk assessment
    updateRiskAssessment: builder.mutation<RiskAssessmentDto, UpdateRiskAssessmentRequest>({
      query: ({ id, ...data }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_, __, { id }) => [
        { type: 'RiskAssessment' as const, id },
        { type: 'RiskAssessmentDetail' as const, id },
        'RiskAssessmentStatistics',
      ],
    }),

    // Delete risk assessment
    deleteRiskAssessment: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, id) => [
        { type: 'RiskAssessment' as const, id },
        { type: 'RiskAssessmentDetail' as const, id },
        'RiskAssessmentStatistics',
      ],
    }),

    // Approve risk assessment
    approveRiskAssessment: builder.mutation<RiskAssessmentDto, { id: number; approvalNotes?: string }>({
      query: ({ id, approvalNotes }) => ({
        url: `/${id}/approve`,
        method: 'POST',
        body: { approvalNotes },
      }),
      invalidatesTags: (_, __, { id }) => [
        { type: 'RiskAssessment' as const, id },
        { type: 'RiskAssessmentDetail' as const, id },
        'RiskAssessmentStatistics',
      ],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useGetRiskAssessmentsQuery,
  useGetRiskAssessmentQuery,
  useGetRiskAssessmentsDueForReviewQuery,
  useGetRiskAssessmentsByRiskLevelQuery,
  useGetPendingApprovalRiskAssessmentsQuery,
  useGetRiskAssessmentStatisticsQuery,
  useCreateRiskAssessmentMutation,
  useUpdateRiskAssessmentMutation,
  useDeleteRiskAssessmentMutation,
  useApproveRiskAssessmentMutation,
} = riskAssessmentApi;