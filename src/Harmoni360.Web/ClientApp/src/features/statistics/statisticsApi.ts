import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { HsseStatisticsDto, HsseTrendPointDto } from '../../types/hsse';

export const statisticsApi = createApi({
  reducerPath: 'statisticsApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/statistics',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['HsseStatistics'],
  endpoints: (builder) => ({
    getStatistics: builder.query<HsseStatisticsDto, { module?: string; startDate?: string; endDate?: string }>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        if (params.module) searchParams.append('module', params.module);
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        return {
          url: `?${searchParams.toString()}`,
          method: 'GET',
        };
      },
    }),
    getTrends: builder.query<HsseTrendPointDto[], { module?: string; startDate?: string; endDate?: string }>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        if (params.module) searchParams.append('module', params.module);
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        return {
          url: `trends?${searchParams.toString()}`,
          method: 'GET',
        };
      },
      providesTags: ['HsseStatistics'],
    }),
    exportStatistics: builder.query<Blob, { module?: string; startDate?: string; endDate?: string }>({
      query: (params = {}) => {
        const searchParams = new URLSearchParams();
        if (params.module) searchParams.append('module', params.module);
        if (params.startDate) searchParams.append('startDate', params.startDate);
        if (params.endDate) searchParams.append('endDate', params.endDate);
        return {
          url: `export?${searchParams.toString()}`,
          method: 'GET',
          responseHandler: (response) => response.blob(),
        };
      },
      providesTags: ['HsseStatistics'],
    }),
  }),
});

export const { useGetStatisticsQuery, useLazyExportStatisticsQuery, useGetTrendsQuery } = statisticsApi;
