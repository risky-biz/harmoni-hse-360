import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

// Types for configuration entities
export interface DepartmentDto {
  id: number;
  name: string;
  code: string;
  description?: string;
  headOfDepartment?: string;
  contact?: string;
  location?: string;
  isActive: boolean;
  displayOrder: number;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface IncidentCategoryDto {
  id: number;
  name: string;
  code: string;
  description?: string;
  color: string;
  icon: string;
  isActive: boolean;
  displayOrder: number;
  requiresImmediateAction: boolean;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface IncidentLocationDto {
  id: number;
  name: string;
  code: string;
  description?: string;
  building?: string;
  floor?: string;
  room?: string;
  latitude?: number;
  longitude?: number;
  isActive: boolean;
  displayOrder: number;
  isHighRisk: boolean;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  fullLocation: string;
}

// API slice for configuration management
export const configurationApi = createApi({
  reducerPath: 'configurationApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/configuration',
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
  tagTypes: ['Department', 'IncidentCategory', 'IncidentLocation'],
  endpoints: (builder) => ({
    // Get departments
    getDepartments: builder.query<DepartmentDto[], { isActive?: boolean }>({
      query: ({ isActive = true } = {}) => ({
        url: `/departments?isActive=${isActive}`,
        method: 'GET',
      }),
      providesTags: ['Department'],
    }),

    // Get incident categories
    getIncidentCategories: builder.query<IncidentCategoryDto[], { isActive?: boolean }>({
      query: ({ isActive = true } = {}) => ({
        url: `/incident-categories?isActive=${isActive}`,
        method: 'GET',
      }),
      providesTags: ['IncidentCategory'],
    }),

    // Get incident locations
    getIncidentLocations: builder.query<IncidentLocationDto[], { isActive?: boolean; building?: string }>({
      query: ({ isActive = true, building } = {}) => {
        const params = new URLSearchParams();
        params.append('isActive', isActive.toString());
        if (building) {
          params.append('building', building);
        }
        return {
          url: `/incident-locations?${params.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['IncidentLocation'],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useGetDepartmentsQuery,
  useGetIncidentCategoriesQuery,
  useGetIncidentLocationsQuery,
} = configurationApi;