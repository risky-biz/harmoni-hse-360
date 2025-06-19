import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { useState, useCallback } from 'react';
import { useAppDispatch } from '../store/hooks';
import {
  WorkPermitSettings,
  WorkPermitSafetyVideo,
  CreateWorkPermitSettingsRequest,
  UpdateWorkPermitSettingsRequest,
  UploadVideoRequest,
  UploadVideoResponse,
  WorkPermitSettingsResponse,
  VideoUploadProgress
} from '../types/workPermitSettings';

export const workPermitSettingsApi = createApi({
  reducerPath: 'workPermitSettingsApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/work-permits/settings',
    prepareHeaders: (headers, { getState, endpoint }) => {
      // Get token from auth state
      const token = (getState() as any).auth.token;
      if (token) {
        headers.set('authorization', `Bearer ${token}`);
      }

      // Don't set content-type for FormData uploads (RTK Query will set it automatically)
      if (endpoint?.includes('upload') || endpoint?.includes('videos')) {
        // RTK Query will handle FormData content-type automatically
      } else {
        headers.set('content-type', 'application/json');
      }

      return headers;
    },
  }),
  tagTypes: ['WorkPermitSettings', 'SafetyVideo'],
  endpoints: (builder) => ({
    // Get all work permit settings
    getWorkPermitSettings: builder.query<WorkPermitSettingsResponse[], void>({
      query: () => '',
      providesTags: (result) => [
        'WorkPermitSettings',
        ...(result || []).map(({ id }) => ({ type: 'WorkPermitSettings' as const, id })),
        ...(result || []).flatMap(({ safetyVideos }) => 
          (safetyVideos || []).map(video => ({ type: 'SafetyVideo' as const, id: video.id }))
        )
      ]
    }),

    // Get work permit settings by ID
    getWorkPermitSettingsById: builder.query<WorkPermitSettingsResponse, string>({
      query: (id) => `/${id}`,
      providesTags: (result, error, id) => [
        { type: 'WorkPermitSettings', id },
        ...(result?.safetyVideos || []).map(video => ({ type: 'SafetyVideo' as const, id: video.id }))
      ]
    }),

    // Get active work permit settings
    getActiveWorkPermitSettings: builder.query<WorkPermitSettingsResponse | null, void>({
      query: () => '/active',
      providesTags: (result) => [
        'WorkPermitSettings',
        ...(result?.safetyVideos || []).map(video => ({ type: 'SafetyVideo' as const, id: video.id }))
      ]
    }),

    // Create work permit settings
    createWorkPermitSettings: builder.mutation<WorkPermitSettingsResponse, CreateWorkPermitSettingsRequest>({
      query: (settings) => ({
        url: '',
        method: 'POST',
        body: {
          // Transform camelCase frontend properties to PascalCase backend properties
          RequireSafetyInduction: settings.requireSafetyInduction,
          EnableFormValidation: settings.enableFormValidation,
          AllowAttachments: settings.allowAttachments,
          MaxAttachmentSizeMB: settings.maxAttachmentSizeMB,
          FormInstructions: settings.formInstructions,
          IsActive: settings.isActive
        }
      }),
      invalidatesTags: ['WorkPermitSettings']
    }),

    // Update work permit settings
    updateWorkPermitSettings: builder.mutation<WorkPermitSettingsResponse, UpdateWorkPermitSettingsRequest>({
      query: ({ id, ...settings }) => ({
        url: `/${id}`,
        method: 'PUT',
        body: {
          // Transform camelCase frontend properties to PascalCase backend properties
          Id: id,
          RequireSafetyInduction: settings.requireSafetyInduction,
          EnableFormValidation: settings.enableFormValidation,
          AllowAttachments: settings.allowAttachments,
          MaxAttachmentSizeMB: settings.maxAttachmentSizeMB,
          FormInstructions: settings.formInstructions,
          IsActive: settings.isActive
        }
      }),
      invalidatesTags: (result, error, { id }) => [
        { type: 'WorkPermitSettings', id },
        'WorkPermitSettings'
      ]
    }),

    // Delete work permit settings
    deleteWorkPermitSettings: builder.mutation<void, string>({
      query: (id) => ({
        url: `/${id}`,
        method: 'DELETE'
      }),
      invalidatesTags: ['WorkPermitSettings']
    }),


    // Delete safety video
    deleteSafetyVideo: builder.mutation<void, { settingsId: string; videoId: string }>({
      query: ({ settingsId, videoId }) => ({
        url: `/${settingsId}/videos/${videoId}`,
        method: 'DELETE'
      }),
      invalidatesTags: (result, error, { settingsId, videoId }) => [
        'WorkPermitSettings', // Invalidate all work permit settings queries
        { type: 'WorkPermitSettings', id: settingsId }, // Invalidate specific settings
        { type: 'SafetyVideo', id: parseInt(videoId) } // Invalidate the specific video
      ]
    })
  })
});

// Export hooks for usage in functional components
export const {
  useGetWorkPermitSettingsQuery,
  useGetWorkPermitSettingsByIdQuery,
  useGetActiveWorkPermitSettingsQuery,
  useCreateWorkPermitSettingsMutation,
  useUpdateWorkPermitSettingsMutation,
  useDeleteWorkPermitSettingsMutation,
  useDeleteSafetyVideoMutation
} = workPermitSettingsApi;

// Helper hook for managing upload progress
export const useVideoUpload = (onUploadSuccess?: () => void) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<any>(null);
  const [progress, setProgress] = useState<VideoUploadProgress | null>(null);
  const dispatch = useAppDispatch();

  const upload = useCallback(async (settingsId: string, file: File, description?: string) => {
    setIsLoading(true);
    setError(null);
    setProgress(null);
    
    try {
      const formData = new FormData();
      formData.append('videoFile', file);
      formData.append('description', description || '');
      formData.append('setAsActive', 'true');

      // Create XMLHttpRequest for progress tracking
      const xhr = new XMLHttpRequest();
      
      // Setup progress event listener
      xhr.upload.addEventListener('progress', (event) => {
        if (event.lengthComputable) {
          const progressData: VideoUploadProgress = {
            loaded: event.loaded,
            total: event.total,
            percentage: Math.round((event.loaded / event.total) * 100)
          };
          setProgress(progressData);
        }
      });

      // Return promise that resolves when upload completes
      return new Promise<UploadVideoResponse>((resolve, reject) => {
        xhr.onload = () => {
          setIsLoading(false);
          if (xhr.status >= 200 && xhr.status < 300) {
            try {
              const response = JSON.parse(xhr.responseText) as UploadVideoResponse;
              
              // Invalidate RTK Query cache for work permit settings
              dispatch(workPermitSettingsApi.util.invalidateTags([
                'WorkPermitSettings',
                { type: 'WorkPermitSettings', id: settingsId }
              ]));
              
              // Call success callback to refresh data
              if (onUploadSuccess) {
                onUploadSuccess();
              }
              
              resolve(response);
            } catch (parseError) {
              reject(new Error('Failed to parse response'));
            }
          } else {
            reject(new Error(`Upload failed with status: ${xhr.status}`));
          }
        };

        xhr.onerror = () => {
          setIsLoading(false);
          reject(new Error('Network error during upload'));
        };

        xhr.onabort = () => {
          setIsLoading(false);
          reject(new Error('Upload cancelled'));
        };

        // Get auth token
        const token = localStorage.getItem('token') || sessionStorage.getItem('token');
        
        // Open request and set headers
        xhr.open('POST', `/api/work-permits/settings/${settingsId}/videos`);
        if (token) {
          xhr.setRequestHeader('Authorization', `Bearer ${token}`);
        }
        
        // Send the request
        xhr.send(formData);
      });
    } catch (error) {
      setIsLoading(false);
      setError(error);
      throw error;
    } finally {
      // Reset progress after a delay
      setTimeout(() => setProgress(null), 2000);
    }
  }, [onUploadSuccess, dispatch]);

  return {
    upload,
    isLoading,
    error,
    progress
  };
};

// Helper hook for getting active settings with loading states
export const useActiveWorkPermitSettings = () => {
  const { data, isLoading, error, refetch } = useGetActiveWorkPermitSettingsQuery();
  
  return {
    settings: data,
    isLoading,
    error,
    refetch,
    hasActiveSettings: !!data,
    requiresSafetyVideo: data?.requireSafetyInduction ?? false,
    safetyVideos: data?.safetyVideos ?? []
  };
};