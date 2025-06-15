import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import {
  LicenseDto,
  LicenseDashboardDto,
  LicenseFormData,
  LicenseAttachmentDto
} from '../../types/license';
import { PagedList } from '../../types/common';

// Create the licenses API slice
export const licenseApi = createApi({
  reducerPath: 'licenseApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/licenses',
    prepareHeaders: (headers, { getState }) => {
      // Add authorization header if needed
      const token = localStorage.getItem('token');
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }
      return headers;
    },
  }),
  tagTypes: ['License', 'LicenseDashboard', 'MyLicenses'],
  endpoints: (builder) => ({
    // Get paginated licenses with filtering
    getLicenses: builder.query<PagedList<LicenseDto>, {
      page?: number;
      pageSize?: number;
      searchTerm?: string;
      status?: string;
      type?: string;
      priority?: string;
      riskLevel?: string;
      department?: string;
      issuingAuthority?: string;
      isExpiring?: boolean;
      isExpired?: boolean;
      renewalDue?: boolean;
      expiryDateFrom?: string;
      expiryDateTo?: string;
      sortBy?: string;
      sortDirection?: string;
    }>({
      query: (params) => ({
        url: '',
        params: {
          page: params.page || 1,
          pageSize: params.pageSize || 10,
          ...(params.searchTerm && { searchTerm: params.searchTerm }),
          ...(params.status && { status: params.status }),
          ...(params.type && { type: params.type }),
          ...(params.priority && { priority: params.priority }),
          ...(params.riskLevel && { riskLevel: params.riskLevel }),
          ...(params.department && { department: params.department }),
          ...(params.issuingAuthority && { issuingAuthority: params.issuingAuthority }),
          ...(params.isExpiring !== undefined && { isExpiring: params.isExpiring }),
          ...(params.isExpired !== undefined && { isExpired: params.isExpired }),
          ...(params.renewalDue !== undefined && { renewalDue: params.renewalDue }),
          ...(params.expiryDateFrom && { expiryDateFrom: params.expiryDateFrom }),
          ...(params.expiryDateTo && { expiryDateTo: params.expiryDateTo }),
          sortBy: params.sortBy || 'createdAt',
          sortDirection: params.sortDirection || 'desc',
        },
      }),
      providesTags: ['License'],
    }),

    // Get license by ID
    getLicenseById: builder.query<LicenseDto, number>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [{ type: 'License', id }],
    }),

    // Get my licenses (user-specific)
    getMyLicenses: builder.query<PagedList<LicenseDto>, {
      page?: number;
      pageSize?: number;
      searchTerm?: string;
      status?: string;
      type?: string;
      isExpiring?: boolean;
      isExpired?: boolean;
      sortBy?: string;
      sortDirection?: string;
    }>({
      query: (params) => ({
        url: '/my-licenses',
        params: {
          page: params.page || 1,
          pageSize: params.pageSize || 10,
          ...(params.searchTerm && { searchTerm: params.searchTerm }),
          ...(params.status && { status: params.status }),
          ...(params.type && { type: params.type }),
          ...(params.isExpiring !== undefined && { isExpiring: params.isExpiring }),
          ...(params.isExpired !== undefined && { isExpired: params.isExpired }),
          sortBy: params.sortBy || 'createdAt',
          sortDirection: params.sortDirection || 'desc',
        },
      }),
      providesTags: ['MyLicenses'],
    }),

    // Get license dashboard
    getLicenseDashboard: builder.query<LicenseDashboardDto, {
      departmentId?: number;
      fromDate?: string;
      toDate?: string;
    }>({
      query: (params) => ({
        url: '/dashboard',
        params: {
          ...(params.departmentId && { departmentId: params.departmentId }),
          ...(params.fromDate && { fromDate: params.fromDate }),
          ...(params.toDate && { toDate: params.toDate }),
        },
      }),
      providesTags: ['LicenseDashboard'],
    }),

    // Get expiring licenses
    getExpiringLicenses: builder.query<PagedList<LicenseDto>, {
      page?: number;
      pageSize?: number;
      daysAhead?: number;
    }>({
      query: (params) => ({
        url: '/expiring',
        params: {
          page: params.page || 1,
          pageSize: params.pageSize || 10,
          ...(params.daysAhead && { daysAhead: params.daysAhead }),
        },
      }),
      providesTags: ['License'],
    }),

    // Create license
    createLicense: builder.mutation<LicenseDto, Partial<LicenseFormData>>({
      query: (license) => ({
        url: '',
        method: 'POST',
        body: license,
      }),
      invalidatesTags: ['License', 'LicenseDashboard', 'MyLicenses'],
    }),

    // Update license
    updateLicense: builder.mutation<LicenseDto, { id: number; data: Partial<LicenseFormData> }>({
      query: ({ id, data }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: { id, ...data },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Delete license
    deleteLicense: builder.mutation<void, number>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['License', 'LicenseDashboard', 'MyLicenses'],
    }),

    // Submit license
    submitLicense: builder.mutation<LicenseDto, { id: number; submissionNotes?: string }>({
      query: ({ id, submissionNotes }) => ({
        url: `/${id}/submit`,
        method: 'POST',
        body: { id, submissionNotes: submissionNotes || '' },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Approve license
    approveLicense: builder.mutation<LicenseDto, {
      id: number;
      approvalNotes?: string;
      effectiveDate?: string;
      expiryDate?: string;
    }>({
      query: ({ id, approvalNotes, effectiveDate, expiryDate }) => ({
        url: `/${id}/approve`,
        method: 'POST',
        body: {
          id,
          approvalNotes: approvalNotes || '',
          effectiveDate,
          expiryDate,
        },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Reject license
    rejectLicense: builder.mutation<LicenseDto, {
      id: number;
      rejectionReason: string;
      rejectionNotes?: string;
    }>({
      query: ({ id, rejectionReason, rejectionNotes }) => ({
        url: `/${id}/reject`,
        method: 'POST',
        body: {
          id,
          rejectionReason,
          rejectionNotes: rejectionNotes || '',
        },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Activate license
    activateLicense: builder.mutation<LicenseDto, { id: number }>({
      query: ({ id }) => ({
        url: `/${id}/activate`,
        method: 'POST',
        body: { id },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Suspend license
    suspendLicense: builder.mutation<LicenseDto, {
      id: number;
      suspensionReason: string;
      suspensionNotes?: string;
    }>({
      query: ({ id, suspensionReason, suspensionNotes }) => ({
        url: `/${id}/suspend`,
        method: 'POST',
        body: {
          id,
          suspensionReason,
          suspensionNotes: suspensionNotes || '',
        },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Revoke license
    revokeLicense: builder.mutation<LicenseDto, {
      id: number;
      revocationReason: string;
      revocationNotes?: string;
    }>({
      query: ({ id, revocationReason, revocationNotes }) => ({
        url: `/${id}/revoke`,
        method: 'POST',
        body: {
          id,
          revocationReason,
          revocationNotes: revocationNotes || '',
        },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Renew license
    renewLicense: builder.mutation<LicenseDto, {
      id: number;
      renewalNotes?: string;
      newExpiryDate?: string;
    }>({
      query: ({ id, renewalNotes, newExpiryDate }) => ({
        url: `/${id}/renew`,
        method: 'POST',
        body: {
          id,
          renewalNotes: renewalNotes || '',
          newExpiryDate,
        },
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'License', id },
        'License',
        'LicenseDashboard',
        'MyLicenses',
      ],
    }),

    // Upload attachment
    uploadAttachment: builder.mutation<LicenseAttachmentDto, {
      licenseId: number;
      file: File;
      attachmentType: string;
      description?: string;
    }>({
      query: ({ licenseId, file, attachmentType, description }) => {
        const formData = new FormData();
        formData.append('licenseId', licenseId.toString());
        formData.append('file', file);
        formData.append('attachmentType', attachmentType);
        if (description) {
          formData.append('description', description);
        }

        return {
          url: `/${licenseId}/attachments`,
          method: 'POST',
          body: formData,
        };
      },
      invalidatesTags: (result, error, { licenseId }) => [
        { type: 'License', id: licenseId },
        'License',
      ],
    }),

    // Download attachment
    downloadAttachment: builder.query<Blob, { licenseId: number; attachmentId: number }>({
      query: ({ licenseId, attachmentId }) => ({
        url: `/${licenseId}/attachments/${attachmentId}`,
        responseHandler: (response) => response.blob(),
      }),
    }),

    // Delete attachment
    deleteAttachment: builder.mutation<void, { licenseId: number; attachmentId: number }>({
      query: ({ licenseId, attachmentId }) => ({
        url: `/${licenseId}/attachments/${attachmentId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (result, error, { licenseId }) => [
        { type: 'License', id: licenseId },
        'License',
      ],
    }),

    // Add license condition
    addLicenseCondition: builder.mutation<LicenseDto, {
      licenseId: number;
      conditionType: string;
      description: string;
      isMandatory: boolean;
      dueDate?: string;
      responsiblePerson?: string;
      notes?: string;
    }>({
      query: ({ licenseId, ...condition }) => ({
        url: `/${licenseId}/conditions`,
        method: 'POST',
        body: { licenseId, ...condition },
      }),
      invalidatesTags: (result, error, { licenseId }) => [
        { type: 'License', id: licenseId },
        'License',
      ],
    }),

    // Update license condition
    updateLicenseCondition: builder.mutation<LicenseDto, {
      licenseId: number;
      conditionId: number;
      conditionType: string;
      description: string;
      isMandatory: boolean;
      dueDate?: string;
      responsiblePerson?: string;
      notes?: string;
    }>({
      query: ({ licenseId, conditionId, ...condition }) => ({
        url: `/${licenseId}/conditions/${conditionId}`,
        method: 'PUT',
        body: { licenseId, conditionId, ...condition },
      }),
      invalidatesTags: (result, error, { licenseId }) => [
        { type: 'License', id: licenseId },
        'License',
      ],
    }),

    // Complete license condition
    completeLicenseCondition: builder.mutation<LicenseDto, {
      licenseId: number;
      conditionId: number;
      complianceEvidence?: string;
      complianceNotes?: string;
    }>({
      query: ({ licenseId, conditionId, complianceEvidence, complianceNotes }) => ({
        url: `/${licenseId}/conditions/${conditionId}/complete`,
        method: 'POST',
        body: {
          licenseId,
          conditionId,
          complianceEvidence: complianceEvidence || '',
          complianceNotes: complianceNotes || '',
        },
      }),
      invalidatesTags: (result, error, { licenseId }) => [
        { type: 'License', id: licenseId },
        'License',
      ],
    }),
  }),
});

// Export hooks for usage in functional components
export const {
  useGetLicensesQuery,
  useGetLicenseByIdQuery,
  useGetMyLicensesQuery,
  useGetLicenseDashboardQuery,
  useGetExpiringLicensesQuery,
  useCreateLicenseMutation,
  useUpdateLicenseMutation,
  useDeleteLicenseMutation,
  useSubmitLicenseMutation,
  useApproveLicenseMutation,
  useRejectLicenseMutation,
  useActivateLicenseMutation,
  useSuspendLicenseMutation,
  useRevokeLicenseMutation,
  useRenewLicenseMutation,
  useUploadAttachmentMutation,
  useDownloadAttachmentQuery,
  useDeleteAttachmentMutation,
  useAddLicenseConditionMutation,
  useUpdateLicenseConditionMutation,
  useCompleteLicenseConditionMutation,
} = licenseApi;