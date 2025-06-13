import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { LoginRequest, LoginResponse, User } from '../../types/auth';

export const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/auth',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['Auth'],
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: 'login',
        method: 'POST',
        body: credentials,
      }),
      invalidatesTags: ['Auth'],
    }),
    logout: builder.mutation<void, void>({
      query: () => ({
        url: 'logout',
        method: 'POST',
      }),
      invalidatesTags: ['Auth'],
    }),
    getCurrentUser: builder.query<User, void>({
      query: () => 'me',
      providesTags: ['Auth'],
    }),
    validateToken: builder.query<{ valid: boolean; userId: number }, void>({
      query: () => ({
        url: 'validate',
        method: 'POST',
      }),
      providesTags: ['Auth'],
    }),
    refreshToken: builder.mutation<
      LoginResponse,
      { token: string; refreshToken: string }
    >({
      query: (tokens) => ({
        url: 'refresh',
        method: 'POST',
        body: tokens,
      }),
    }),
    getDemoUsers: builder.query<any, void>({
      query: () => 'demo-users',
    }),
  }),
});

export const {
  useLoginMutation,
  useLogoutMutation,
  useGetCurrentUserQuery,
  useValidateTokenQuery,
  useRefreshTokenMutation,
  useGetDemoUsersQuery,
} = authApi;
