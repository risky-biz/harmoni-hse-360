// API endpoints for hazard configuration management
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { RootState } from '../store';

// Define interfaces for hazard configuration
export interface HazardCategory {
  id: number;
  name: string;
  code: string;
  description?: string;
  color?: string;
  riskLevel: 'Low' | 'Medium' | 'High' | 'Critical';
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface HazardType {
  id: number;
  name: string;
  code: string;
  description?: string;
  categoryId?: number;
  categoryName?: string;
  riskMultiplier: number;
  requiresPermit: boolean;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface HazardCategoryCreateRequest {
  name: string;
  code: string;
  description?: string;
  color?: string;
  riskLevel: 'Low' | 'Medium' | 'High' | 'Critical';
  displayOrder: number;
  isActive: boolean;
}

export interface HazardTypeCreateRequest {
  name: string;
  code: string;
  description?: string;
  categoryId?: number;
  riskMultiplier: number;
  requiresPermit: boolean;
  displayOrder: number;
  isActive: boolean;
}

export const hazardConfigurationApi = createApi({
  reducerPath: 'hazardConfigurationApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/configuration/',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['HazardCategory', 'HazardType'],
  endpoints: (builder) => ({
    // Hazard Categories
    getHazardCategories: builder.query<HazardCategory[], void>({
      query: () => 'hazard-categories',
      providesTags: ['HazardCategory'],
    }),
    
    getHazardCategoryById: builder.query<HazardCategory, number>({
      query: (id) => `hazard-categories/${id}`,
      providesTags: (result, error, id) => [{ type: 'HazardCategory', id }],
    }),
    
    createHazardCategory: builder.mutation<HazardCategory, HazardCategoryCreateRequest>({
      query: (newCategory) => ({
        url: 'hazard-categories',
        method: 'POST',
        body: newCategory,
      }),
      invalidatesTags: ['HazardCategory'],
    }),
    
    updateHazardCategory: builder.mutation<HazardCategory, { id: number; updates: HazardCategoryCreateRequest }>({
      query: ({ id, updates }) => ({
        url: `hazard-categories/${id}`,
        method: 'PUT',
        body: updates,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'HazardCategory', id }],
    }),
    
    deleteHazardCategory: builder.mutation<{ success: boolean; id: number }, number>({
      query: (id) => ({
        url: `hazard-categories/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['HazardCategory'],
    }),

    // Hazard Types
    getHazardTypes: builder.query<HazardType[], void>({
      query: () => 'hazard-types',
      providesTags: ['HazardType'],
    }),
    
    getHazardTypeById: builder.query<HazardType, number>({
      query: (id) => `hazard-types/${id}`,
      providesTags: (result, error, id) => [{ type: 'HazardType', id }],
    }),
    
    createHazardType: builder.mutation<HazardType, HazardTypeCreateRequest>({
      query: (newType) => ({
        url: 'hazard-types',
        method: 'POST',
        body: newType,
      }),
      invalidatesTags: ['HazardType'],
    }),
    
    updateHazardType: builder.mutation<HazardType, { id: number; updates: HazardTypeCreateRequest }>({
      query: ({ id, updates }) => ({
        url: `hazard-types/${id}`,
        method: 'PUT',
        body: updates,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'HazardType', id }],
    }),
    
    deleteHazardType: builder.mutation<{ success: boolean; id: number }, number>({
      query: (id) => ({
        url: `hazard-types/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['HazardType'],
    }),
  }),
});

export const {
  // Hazard Categories
  useGetHazardCategoriesQuery,
  useGetHazardCategoryByIdQuery,
  useCreateHazardCategoryMutation,
  useUpdateHazardCategoryMutation,
  useDeleteHazardCategoryMutation,
  
  // Hazard Types
  useGetHazardTypesQuery,
  useGetHazardTypeByIdQuery,
  useCreateHazardTypeMutation,
  useUpdateHazardTypeMutation,
  useDeleteHazardTypeMutation,
} = hazardConfigurationApi;