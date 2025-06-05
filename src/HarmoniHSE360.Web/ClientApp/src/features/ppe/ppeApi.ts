import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// Types
export interface PPECategoryDto {
  id: number;
  name: string;
  description: string;
  type: string;
  requiresCertification: boolean;
  requiresInspection: boolean;
  inspectionIntervalDays?: number;
  requiresExpiry: boolean;
  defaultExpiryDays?: number;
  complianceStandard?: string;
  isActive: boolean;
}

export interface PPEItemDto {
  id: number;
  itemCode: string;
  name: string;
  description: string;
  categoryId: number;
  categoryName: string;
  categoryType: string;
  manufacturer: string;
  model: string;
  size: string;
  color?: string;
  condition: string;
  expiryDate?: string;
  purchaseDate: string;
  cost: number;
  location: string;
  assignedToId?: number;
  assignedToName?: string;
  assignedDate?: string;
  status: string;
  notes?: string;
  
  // Certification Info
  certificationNumber?: string;
  certifyingBody?: string;
  certificationDate?: string;
  certificationExpiryDate?: string;
  certificationStandard?: string;
  
  // Maintenance Info
  maintenanceIntervalDays?: number;
  lastMaintenanceDate?: string;
  nextMaintenanceDate?: string;
  maintenanceInstructions?: string;
  
  // Computed Properties
  isExpired: boolean;
  isExpiringSoon: boolean;
  isMaintenanceDue: boolean;
  isMaintenanceDueSoon: boolean;
  isCertificationExpired: boolean;
  isCertificationExpiringSoon: boolean;
  lastInspectionDate?: string;
  nextInspectionDue?: string;
  isInspectionDue: boolean;
  
  // Audit Info
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface PPEItemSummaryDto {
  id: number;
  itemCode: string;
  name: string;
  categoryName: string;
  manufacturer: string;
  model: string;
  size: string;
  condition: string;
  status: string;
  location: string;
  assignedToName?: string;
  expiryDate?: string;
  isExpired: boolean;
  isExpiringSoon: boolean;
  isMaintenanceDue: boolean;
  isInspectionDue: boolean;
}

export interface PPERequestDto {
  id: number;
  requestNumber: string;
  requesterId: number;
  requesterName: string;
  requesterEmail: string;
  requesterDepartment: string;
  categoryId: number;
  categoryName: string;
  justification: string;
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  status: 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Fulfilled' | 'Cancelled';
  requestDate: string;
  requiredDate?: string;
  reviewerId?: number;
  reviewerName?: string;
  reviewedDate?: string;
  approvedDate?: string;
  approvedBy?: string;
  fulfilledDate?: string;
  fulfilledBy?: string;
  fulfilledPPEItemId?: number;
  fulfilledPPEItemCode?: string;
  rejectionReason?: string;
  notes?: string;
  requestItems: PPERequestItemDto[];
  
  // Computed Properties
  isOverdue: boolean;
  isUrgent: boolean;
  daysUntilRequired?: number;
  processingTimeDisplay: string;
  
  // Audit Info
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface PPERequestItemDto {
  id: number;
  requestId: number;
  itemDescription: string;
  size?: string;
  quantity: number;
  specialRequirements?: string;
}

export interface PPEAssignmentDto {
  id: number;
  ppeItemId: number;
  ppeItemCode: string;
  ppeItemName: string;
  ppeItemCategory: string;
  assignedToId: number;
  assignedToName: string;
  assignedToEmail: string;
  assignedToDepartment: string;
  assignedDate: string;
  returnedDate?: string;
  assignedBy: string;
  returnedBy?: string;
  purpose?: string;
  status: string;
  returnNotes?: string;
  
  // Computed Properties
  daysAssigned: number;
  assignmentDurationDisplay: string;
  isActive: boolean;
  
  // Audit Info
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface PPEDashboardDto {
  totalItems: number;
  availableItems: number;
  assignedItems: number;
  outOfServiceItems: number;
  expiredItems: number;
  expiringSoonItems: number;
  maintenanceDueItems: number;
  inspectionDueItems: number;
  lostItems: number;
  retiredItems: number;
  
  categoryStats: PPECategoryStatsDto[];
  statusStats: PPEStatusStatsDto[];
  conditionStats: PPEConditionStatsDto[];
  expiryWarnings: PPEExpiryWarningDto[];
  maintenanceWarnings: PPEMaintenanceWarningDto[];
}

export interface PPECategoryStatsDto {
  categoryId: number;
  categoryName: string;
  totalItems: number;
  availableItems: number;
  assignedItems: number;
}

export interface PPEStatusStatsDto {
  status: string;
  count: number;
  percentage: number;
}

export interface PPEConditionStatsDto {
  condition: string;
  count: number;
  percentage: number;
}

export interface PPEExpiryWarningDto {
  itemId: number;
  itemCode: string;
  itemName: string;
  expiryDate: string;
  daysUntilExpiry: number;
  isExpired: boolean;
}

export interface PPEMaintenanceWarningDto {
  itemId: number;
  itemCode: string;
  itemName: string;
  dueDate: string;
  daysOverdue: number;
  isOverdue: boolean;
}

// Request types
export interface CreatePPEItemRequest {
  itemCode: string;
  name: string;
  description: string;
  categoryId: number;
  manufacturer: string;
  model: string;
  size: string;
  color?: string;
  purchaseDate: string;
  cost: number;
  location: string;
  expiryDate?: string;
  notes?: string;
  
  // Certification Info
  certificationNumber?: string;
  certifyingBody?: string;
  certificationDate?: string;
  certificationExpiryDate?: string;
  certificationStandard?: string;
  
  // Maintenance Info
  maintenanceIntervalDays?: number;
  lastMaintenanceDate?: string;
  maintenanceInstructions?: string;
}

export interface UpdatePPEItemRequest {
  name: string;
  description: string;
  manufacturer: string;
  model: string;
  size: string;
  color?: string;
  location: string;
  expiryDate?: string;
  notes?: string;
}

export interface AssignPPERequest {
  ppeItemId: number;
  assignedToId: number;
  purpose?: string;
}

export interface ReturnPPERequest {
  ppeItemId: number;
  newCondition?: string;
  returnNotes?: string;
}

export interface MarkPPEAsLostRequest {
  ppeItemId: number;
  notes?: string;
}

export interface PPEItemsListParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: number;
  status?: string;
  condition?: string;
  location?: string;
  assignedToId?: number;
  isExpired?: boolean;
  isExpiringSoon?: boolean;
  isMaintenanceDue?: boolean;
  isInspectionDue?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface PPEDashboardParams {
  fromDate?: string;
  toDate?: string;
  category?: string;
}

export interface GetPPEItemsResponse {
  items: PPEItemSummaryDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// API slice
export const ppeApi = createApi({
  reducerPath: 'ppeApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/ppe',
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
    'PPEItem',
    'PPECategory',
    'PPERequest',
    'PPEAssignment',
    'PPEDashboard',
  ],
  endpoints: (builder) => ({
    // PPE Items
    getPPEItems: builder.query<GetPPEItemsResponse, PPEItemsListParams>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();

        if (params.pageNumber)
          searchParams.append('pageNumber', params.pageNumber.toString());
        if (params.pageSize)
          searchParams.append('pageSize', params.pageSize.toString());
        if (params.searchTerm)
          searchParams.append('searchTerm', params.searchTerm);
        if (params.categoryId)
          searchParams.append('categoryId', params.categoryId.toString());
        if (params.status) searchParams.append('status', params.status);
        if (params.condition) searchParams.append('condition', params.condition);
        if (params.location) searchParams.append('location', params.location);
        if (params.assignedToId)
          searchParams.append('assignedToId', params.assignedToId.toString());
        if (params.isExpired !== undefined)
          searchParams.append('isExpired', params.isExpired.toString());
        if (params.isExpiringSoon !== undefined)
          searchParams.append('isExpiringSoon', params.isExpiringSoon.toString());
        if (params.isMaintenanceDue !== undefined)
          searchParams.append('isMaintenanceDue', params.isMaintenanceDue.toString());
        if (params.isInspectionDue !== undefined)
          searchParams.append('isInspectionDue', params.isInspectionDue.toString());
        if (params.sortBy) searchParams.append('sortBy', params.sortBy);
        if (params.sortDirection)
          searchParams.append('sortDirection', params.sortDirection);

        return {
          url: `?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: (result) => [
        'PPEItem',
        ...(result?.items.map(({ id }) => ({
          type: 'PPEItem' as const,
          id,
        })) ?? []),
      ],
    }),

    getPPEItem: builder.query<PPEItemDto, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'GET',
      }),
      providesTags: (_, __, id) => [{ type: 'PPEItem' as const, id }],
    }),

    createPPEItem: builder.mutation<PPEItemDto, CreatePPEItemRequest>({
      query: (item) => ({
        url: '',
        method: 'POST',
        body: item,
      }),
      invalidatesTags: ['PPEItem', 'PPEDashboard'],
    }),

    updatePPEItem: builder.mutation<
      PPEItemDto,
      { id: number; item: UpdatePPEItemRequest }
    >({
      query: ({ id, item }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: item,
      }),
      invalidatesTags: (_, __, { id }) => [
        { type: 'PPEItem' as const, id },
        'PPEItem',
        'PPEDashboard',
      ],
    }),

    deletePPEItem: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_, __, id) => [
        'PPEItem',
        'PPEDashboard',
        { type: 'PPEItem' as const, id },
      ],
    }),

    // PPE Assignment
    assignPPE: builder.mutation<PPEAssignmentDto, AssignPPERequest>({
      query: ({ ppeItemId, ...data }) => ({
        url: `/${ppeItemId}/assign`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_, __, { ppeItemId }) => [
        { type: 'PPEItem' as const, id: ppeItemId },
        'PPEItem',
        'PPEAssignment',
        'PPEDashboard',
      ],
    }),

    returnPPE: builder.mutation<void, ReturnPPERequest>({
      query: ({ ppeItemId, ...data }) => ({
        url: `/${ppeItemId}/return`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_, __, { ppeItemId }) => [
        { type: 'PPEItem' as const, id: ppeItemId },
        'PPEItem',
        'PPEAssignment',
        'PPEDashboard',
      ],
    }),

    markPPEAsLost: builder.mutation<void, MarkPPEAsLostRequest>({
      query: ({ ppeItemId, ...data }) => ({
        url: `/${ppeItemId}/lost`,
        method: 'POST',
        body: data,
      }),
      invalidatesTags: (_, __, { ppeItemId }) => [
        { type: 'PPEItem' as const, id: ppeItemId },
        'PPEItem',
        'PPEAssignment',
        'PPEDashboard',
      ],
    }),

    // PPE Categories
    getPPECategories: builder.query<PPECategoryDto[], void>({
      query: () => ({
        url: '/categories',
        method: 'GET',
      }),
      providesTags: ['PPECategory'],
    }),

    // PPE Dashboard
    getPPEDashboard: builder.query<PPEDashboardDto, PPEDashboardParams | void>({
      query: (params) => {
        const searchParams = new URLSearchParams();
        
        if (params && params.fromDate) searchParams.append('fromDate', params.fromDate);
        if (params && params.toDate) searchParams.append('toDate', params.toDate);
        if (params && params.category) searchParams.append('category', params.category);

        return {
          url: `/dashboard${searchParams.toString() ? `?${searchParams.toString()}` : ''}`,
          method: 'GET',
        };
      },
      providesTags: ['PPEDashboard'],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useGetPPEItemsQuery,
  useGetPPEItemQuery,
  useCreatePPEItemMutation,
  useUpdatePPEItemMutation,
  useDeletePPEItemMutation,
  useAssignPPEMutation,
  useReturnPPEMutation,
  useMarkPPEAsLostMutation,
  useGetPPECategoriesQuery,
  useGetPPEDashboardQuery,
} = ppeApi;