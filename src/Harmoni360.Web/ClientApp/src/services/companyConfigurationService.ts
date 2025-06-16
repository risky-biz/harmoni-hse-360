import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';

export interface CompanyConfiguration {
  id: number;
  companyName: string;
  companyCode: string;
  companyDescription?: string;
  websiteUrl?: string;
  logoUrl?: string;
  faviconUrl?: string;
  
  // Contact Information
  primaryEmail?: string;
  primaryPhone?: string;
  emergencyContactNumber?: string;
  
  // Address Information
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  fullAddress?: string;
  
  // Geographic Coordinates
  defaultLatitude?: number;
  defaultLongitude?: number;
  hasGeographicCoordinates: boolean;
  
  // Branding & Themes
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  hasBrandingColors: boolean;
  
  // Compliance & Industry
  industryType?: string;
  complianceStandards?: string;
  regulatoryAuthority?: string;
  
  // System Settings
  timeZone?: string;
  dateFormat?: string;
  currency?: string;
  language?: string;
  
  // System Information
  isActive: boolean;
  version: number;
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface UpdateCompanyConfigurationRequest {
  // Basic Information
  companyName: string;
  companyCode: string;
  companyDescription?: string;
  websiteUrl?: string;
  logoUrl?: string;
  faviconUrl?: string;
  
  // Contact Information
  primaryEmail?: string;
  primaryPhone?: string;
  emergencyContactNumber?: string;
  
  // Address Information
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  
  // Geographic Coordinates
  defaultLatitude?: number;
  defaultLongitude?: number;
  
  // Branding & Themes
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  
  // Compliance & Industry
  industryType?: string;
  complianceStandards?: string;
  regulatoryAuthority?: string;
  
  // System Settings
  timeZone?: string;
  dateFormat?: string;
  currency?: string;
  language?: string;
}

export const companyConfigurationApi = createApi({
  reducerPath: 'companyConfigurationApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/configuration',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as any).auth?.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['CompanyConfiguration'],
  endpoints: (builder) => ({
    getCompanyConfiguration: builder.query<CompanyConfiguration | null, void>({
      query: () => 'company',
      providesTags: ['CompanyConfiguration'],
    }),
    updateCompanyConfiguration: builder.mutation<CompanyConfiguration, UpdateCompanyConfigurationRequest>({
      query: (data) => ({
        url: 'company',
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['CompanyConfiguration'],
    }),
    createCompanyConfiguration: builder.mutation<CompanyConfiguration, {
      companyName: string;
      companyCode: string;
      companyDescription?: string;
      websiteUrl?: string;
      primaryEmail?: string;
    }>({
      query: (data) => ({
        url: 'company',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['CompanyConfiguration'],
    }),
  }),
});

export const {
  useGetCompanyConfigurationQuery,
  useUpdateCompanyConfigurationMutation,
  useCreateCompanyConfigurationMutation,
} = companyConfigurationApi;