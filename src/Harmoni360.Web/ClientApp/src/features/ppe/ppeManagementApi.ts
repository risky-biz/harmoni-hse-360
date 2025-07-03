import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface PPECategory {
  id: number;
  name: string;
  code: string;
  description: string;
  type: string;
  requiresCertification: boolean;
  requiresInspection: boolean;
  inspectionIntervalDays?: number;
  requiresExpiry: boolean;
  defaultExpiryDays?: number;
  complianceStandard?: string;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  itemCount: number;
}

export interface PPESize {
  id: number;
  name: string;
  code: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface PPEStorageLocation {
  id: number;
  name: string;
  code: string;
  description?: string;
  address?: string;
  contactPerson?: string;
  contactPhone?: string;
  isActive: boolean;
  capacity: number;
  currentStock: number;
  utilizationPercentage: number;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface PPEManagementStats {
  totalCategories: number;
  activeCategories: number;
  totalSizes: number;
  activeSizes: number;
  totalStorageLocations: number;
  activeStorageLocations: number;
  totalPPEItems: number;
  lastUpdated: string;
}

export interface CreatePPECategoryRequest {
  name: string;
  code: string;
  description: string;
  type: string;
  requiresCertification: boolean;
  requiresInspection: boolean;
  inspectionIntervalDays?: number;
  requiresExpiry: boolean;
  defaultExpiryDays?: number;
  complianceStandard?: string;
}

export interface CreatePPESizeRequest {
  name: string;
  code: string;
  description?: string;
  sortOrder: number;
}

export interface CreatePPEStorageLocationRequest {
  name: string;
  code: string;
  description?: string;
  address?: string;
  contactPerson?: string;
  contactPhone?: string;
  capacity: number;
}

export const ppeManagementApi = createApi({
  reducerPath: 'ppeManagementApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/PPEManagement',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['PPECategory', 'PPESize', 'PPEStorageLocation', 'PPEStats'],
  endpoints: (builder) => ({
    // PPE Categories
    getPPECategories: builder.query<PPECategory[], { isActive?: boolean; searchTerm?: string }>({
      query: (params) => ({
        url: 'categories',
        params,
      }),
      providesTags: ['PPECategory'],
    }),
    createPPECategory: builder.mutation<PPECategory, CreatePPECategoryRequest>({
      query: (data) => ({
        url: 'categories',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['PPECategory', 'PPEStats'],
    }),
    updatePPECategory: builder.mutation<PPECategory, { id: number; data: CreatePPECategoryRequest }>({
      query: ({ id, data }) => ({
        url: `categories/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['PPECategory', 'PPEStats'],
    }),
    deletePPECategory: builder.mutation<void, number>({
      query: (id) => ({
        url: `categories/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['PPECategory', 'PPEStats'],
    }),

    // PPE Sizes
    getPPESizes: builder.query<PPESize[], { isActive?: boolean; searchTerm?: string }>({
      query: (params) => ({
        url: 'sizes',
        params,
      }),
      providesTags: ['PPESize'],
    }),
    createPPESize: builder.mutation<PPESize, CreatePPESizeRequest>({
      query: (data) => ({
        url: 'sizes',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['PPESize', 'PPEStats'],
    }),
    updatePPESize: builder.mutation<PPESize, { id: number; data: CreatePPESizeRequest }>({
      query: ({ id, data }) => ({
        url: `sizes/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['PPESize', 'PPEStats'],
    }),
    deletePPESize: builder.mutation<void, number>({
      query: (id) => ({
        url: `sizes/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['PPESize', 'PPEStats'],
    }),

    // PPE Storage Locations
    getPPEStorageLocations: builder.query<PPEStorageLocation[], { isActive?: boolean; searchTerm?: string }>({
      query: (params) => ({
        url: 'storage-locations',
        params,
      }),
      providesTags: ['PPEStorageLocation'],
    }),
    createPPEStorageLocation: builder.mutation<PPEStorageLocation, CreatePPEStorageLocationRequest>({
      query: (data) => ({
        url: 'storage-locations',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['PPEStorageLocation', 'PPEStats'],
    }),
    updatePPEStorageLocation: builder.mutation<PPEStorageLocation, { id: number; data: CreatePPEStorageLocationRequest }>({
      query: ({ id, data }) => ({
        url: `storage-locations/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['PPEStorageLocation', 'PPEStats'],
    }),
    deletePPEStorageLocation: builder.mutation<void, number>({
      query: (id) => ({
        url: `storage-locations/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['PPEStorageLocation', 'PPEStats'],
    }),

    // PPE Management Stats
    getPPEManagementStats: builder.query<PPEManagementStats, void>({
      query: () => 'stats',
      providesTags: ['PPEStats'],
    }),
  }),
});

export const {
  useGetPPECategoriesQuery,
  useCreatePPECategoryMutation,
  useUpdatePPECategoryMutation,
  useDeletePPECategoryMutation,
  useGetPPESizesQuery,
  useCreatePPESizeMutation,
  useUpdatePPESizeMutation,
  useDeletePPESizeMutation,
  useGetPPEStorageLocationsQuery,
  useCreatePPEStorageLocationMutation,
  useUpdatePPEStorageLocationMutation,
  useDeletePPEStorageLocationMutation,
  useGetPPEManagementStatsQuery,
} = ppeManagementApi;