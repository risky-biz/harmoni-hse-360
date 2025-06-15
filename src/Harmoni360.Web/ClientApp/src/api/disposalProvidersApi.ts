import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface DisposalProviderDto {
  id: number;
  name: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  status: ProviderStatus;
  isActive: boolean;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
  isExpired: boolean;
  isExpiringSoon: boolean;
}

export enum ProviderStatus {
  Active = 1,
  Suspended = 2,
  Expired = 3,
  UnderReview = 4,
  Terminated = 5
}

export interface CreateDisposalProviderCommand {
  name: string;
  licenseNumber: string;
  licenseExpiryDate: string;
}

export interface UpdateDisposalProviderRequest {
  name: string;
  licenseNumber: string;
  licenseExpiryDate: string;
}

export interface ChangeProviderStatusRequest {
  status: ProviderStatus;
}

export interface SearchDisposalProvidersParams {
  searchTerm?: string;
  status?: ProviderStatus;
  includeInactive?: boolean;
  onlyExpiring?: boolean;
  expiringDays?: number;
}

export const disposalProvidersApi = createApi({
  reducerPath: 'disposalProvidersApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/disposal-providers',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth?.token;
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
      headers.set('Content-Type', 'application/json');
      return headers;
    },
  }),
  tagTypes: ['DisposalProvider'],
  endpoints: (builder) => ({
    // Get all disposal providers
    getDisposalProviders: builder.query<DisposalProviderDto[], void>({
      query: () => '',
      providesTags: ['DisposalProvider'],
    }),

    // Get disposal provider by ID
    getDisposalProviderById: builder.query<DisposalProviderDto, number>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'DisposalProvider', id }],
    }),

    // Search disposal providers
    searchDisposalProviders: builder.query<DisposalProviderDto[], SearchDisposalProvidersParams>({
      query: (params) => {
        const queryParams = new URLSearchParams();
        if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
        if (params.status !== undefined) queryParams.append('status', params.status.toString());
        if (params.includeInactive !== undefined) queryParams.append('includeInactive', params.includeInactive.toString());
        if (params.onlyExpiring !== undefined) queryParams.append('onlyExpiring', params.onlyExpiring.toString());
        if (params.expiringDays !== undefined) queryParams.append('expiringDays', params.expiringDays.toString());
        
        return `search?${queryParams.toString()}`;
      },
      providesTags: ['DisposalProvider'],
    }),

    // Get expiring providers
    getExpiringProviders: builder.query<DisposalProviderDto[], number>({
      query: (daysAhead = 30) => `expiring?daysAhead=${daysAhead}`,
      providesTags: ['DisposalProvider'],
    }),

    // Create disposal provider
    createDisposalProvider: builder.mutation<DisposalProviderDto, CreateDisposalProviderCommand>({
      query: (provider) => ({
        url: '',
        method: 'POST',
        body: provider,
      }),
      invalidatesTags: ['DisposalProvider'],
    }),

    // Update disposal provider
    updateDisposalProvider: builder.mutation<DisposalProviderDto, { id: number; provider: UpdateDisposalProviderRequest }>({
      query: ({ id, provider }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: provider,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'DisposalProvider', id }],
    }),

    // Change provider status
    changeProviderStatus: builder.mutation<DisposalProviderDto, { id: number; status: ProviderStatus }>({
      query: ({ id, status }) => ({
        url: `/${id}/status`,
        method: 'PATCH',
        body: { status },
      }),
      invalidatesTags: (result, error, { id }) => [{ type: 'DisposalProvider', id }],
    }),

    // Delete disposal provider
    deleteDisposalProvider: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['DisposalProvider'],
    }),
  }),
});

export const {
  useGetDisposalProvidersQuery,
  useGetDisposalProviderByIdQuery,
  useSearchDisposalProvidersQuery,
  useGetExpiringProvidersQuery,
  useCreateDisposalProviderMutation,
  useUpdateDisposalProviderMutation,
  useChangeProviderStatusMutation,
  useDeleteDisposalProviderMutation,
} = disposalProvidersApi;