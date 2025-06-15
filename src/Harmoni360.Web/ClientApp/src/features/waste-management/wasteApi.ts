import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface WasteReportDto {
  id: number;
  title: string;
  description: string;
  category: string;
  generatedDate: string;
  location: string;
  reporterId?: number;
  reporterName?: string;
  attachmentsCount: number;
}

export const wasteApi = createApi({
  reducerPath: 'wasteApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/api/' }),
  endpoints: (builder) => ({
    getWasteReports: builder.query<WasteReportDto[], { category?: string; search?: string; page?: number; pageSize?: number }>({
      query: ({ category, search, page = 1, pageSize = 20 } = {}) => {
        const params = new URLSearchParams();
        if (category) params.append('category', category);
        if (search) params.append('search', search);
        params.append('page', String(page));
        params.append('pageSize', String(pageSize));
        return `WasteReport?${params.toString()}`;
      },
    }),
    createWasteReport: builder.mutation<WasteReportDto, FormData>({
      query: (body) => ({
        url: 'WasteReport',
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const { useGetWasteReportsQuery, useCreateWasteReportMutation } = wasteApi;
