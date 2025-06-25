// Work Permit Settings and Safety Video types for frontend

export interface WorkPermitSetting {
  id: number;
  requireSafetyInduction: boolean;
  enableFormValidation: boolean;
  allowAttachments: boolean;
  maxAttachmentSizeMB: number;
  formInstructions?: string;
  isActive: boolean;
  safetyVideos: WorkPermitSafetyVideo[];
  
  // Computed Properties
  activeSafetyVideo?: WorkPermitSafetyVideo;
  isSafetyInductionConfigured: boolean;
  isConfigurationComplete: boolean;
  
  // Audit Properties
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

export interface WorkPermitSafetyVideo {
  id: number;
  fileName: string;
  originalFileName: string;
  filePath: string;
  fileSize: number;
  contentType: string;
  duration: string; // TimeSpan as ISO 8601 duration string
  isActive: boolean;
  description?: string;
  thumbnailPath?: string;
  resolution?: string;
  bitrate?: number;
  
  // Additional properties for UI compatibility
  title?: string;
  fileUrl?: string;
  
  // Computed Properties
  fileSizeMB: number;
  durationFormatted: string;
  isSupportedFormat: boolean;
  
  // Audit Properties
  createdAt: string;
  createdBy: string;
  lastModifiedAt?: string;
  lastModifiedBy?: string;
}

// API Request/Response Types
export interface CreateWorkPermitSettingRequest {
  requireSafetyInduction?: boolean;
  enableFormValidation?: boolean;
  allowAttachments?: boolean;
  maxAttachmentSizeMB?: number;
  formInstructions?: string;
  isActive?: boolean;
}

export interface UpdateWorkPermitSettingRequest {
  id: number;
  requireSafetyInduction: boolean;
  enableFormValidation: boolean;
  allowAttachments: boolean;
  maxAttachmentSizeMB: number;
  formInstructions?: string;
  isActive: boolean;
}

// Type aliases for API compatibility
export type WorkPermitSettings = WorkPermitSetting;
export type WorkPermitSettingsResponse = WorkPermitSetting;
export type CreateWorkPermitSettingsRequest = CreateWorkPermitSettingRequest;
export type UpdateWorkPermitSettingsRequest = UpdateWorkPermitSettingRequest;

export interface UploadSafetyVideoRequest {
  settingId: number;
  videoFile: File;
  description?: string;
  resolution?: string;
  bitrate?: number;
  setAsActive?: boolean;
}

// API-compatible upload types
export interface UploadVideoRequest {
  settingsId: string;
  file: File;
  fileName?: string;
  originalFileName?: string;
  filePath?: string;
  fileSize?: number;
  contentType?: string;
  duration?: TimeSpan;
  description?: string;
  thumbnailPath?: string;
  resolution?: string;
  bitrate?: number;
  onProgress?: (progress: VideoUploadProgress) => void;
}

export interface UploadVideoResponse {
  success: boolean;
  video?: WorkPermitSafetyVideo;
  errors?: string[];
}

export interface VideoUploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

export interface VideoUploadResponse {
  success: boolean;
  video?: WorkPermitSafetyVideo;
  errors?: string[];
}

// TimeSpan type for compatibility
export interface TimeSpan {
  hours: number;
  minutes: number;
  seconds: number;
}

// Form Validation Types
export interface WorkPermitSettingFormData {
  requireSafetyInduction: boolean;
  enableFormValidation: boolean;
  allowAttachments: boolean;
  maxAttachmentSizeMB: number;
  formInstructions: string;
  isActive: boolean;
}

export interface VideoUploadFormData {
  videoFile: File | null;
  description: string;
  setAsActive: boolean;
}

// Video Player Types
export interface VideoPlayerProps {
  videoUrl: string;
  onProgress: (progress: VideoProgress) => void;
  onComplete: () => void;
  disableControls?: boolean;
  requireFullView?: boolean;
}

export interface VideoProgress {
  currentTime: number;
  duration: number;
  percentageWatched: number;
  hasFinished: boolean;
}

export interface VideoPlayerState {
  isPlaying: boolean;
  currentTime: number;
  duration: number;
  volume: number;
  muted: boolean;
  hasStarted: boolean;
  hasFinished: boolean;
  percentageWatched: number;
  lastSeekTime?: number;
}

// Safety Induction Component Types
export interface SafetyInductionProps {
  setting: WorkPermitSetting;
  onVideoComplete: (videoId: number) => void;
  isRequired: boolean;
  disabled?: boolean;
}

export interface SafetyInductionState {
  hasWatchedVideo: boolean;
  videoProgress: VideoProgress;
  isVideoValid: boolean;
  error?: string;
}

// Constants
export const SUPPORTED_VIDEO_FORMATS = [
  'video/mp4',
  'video/webm', 
  'video/avi',
  'video/quicktime',
  'video/x-msvideo'
] as const;

export const MAX_VIDEO_FILE_SIZE = 104_857_600; // 100MB in bytes
export const MAX_VIDEO_DURATION_MINUTES = 120; // 2 hours

export type SupportedVideoFormat = typeof SUPPORTED_VIDEO_FORMATS[number];

// Utility Types
export interface VideoValidationResult {
  isValid: boolean;
  errors: string[];
}

export interface FileValidationOptions {
  maxSizeBytes?: number;
  allowedTypes?: string[];
  maxDurationMinutes?: number;
}

// Error Types
export interface APIError {
  message: string;
  errors?: string[];
  statusCode?: number;
}

// Loading States
export interface LoadingState {
  isLoading: boolean;
  error?: string;
}

export interface UploadState extends LoadingState {
  progress: number;
  isUploading: boolean;
}

// Settings Management UI Types
export interface SettingsTabProps {
  activeTab: string;
  onTabChange: (tab: string) => void;
}

export interface FormConfigurationTabProps {
  setting?: WorkPermitSetting;
  onSave: (data: WorkPermitSettingFormData) => Promise<void>;
  isLoading?: boolean;
}

export interface VideoManagementTabProps {
  setting?: WorkPermitSetting;
  onVideoUpload: (data: VideoUploadFormData) => Promise<void>;
  onVideoDelete: (videoId: number) => Promise<void>;
  isLoading?: boolean;
}